﻿using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    public class GuildService : Singleton<GuildService>
    {

        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(this.OnGuildCreate);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(this.OnGuildList);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);

            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(this.OnGuildLeave);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(this.OnGuildAdmin);
        }

        public void Init()
        {
            GuildManager.Instance.Init();
        }

        void OnGuildCreate(NetConnection<NetSession> sender, GuildCreateRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildCreate::GuildName:{0} character:[{1}] {2}", request.GuildName, character.Id, character.Info.Name);
            sender.Session.Response.guildCreate = new GuildCreateResponse();
            if(character.Guild!=null)
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "已经有公会";
                sender.SendResponse();
                return;
            }

            if(GuildManager.Instance.CheckNameExisted(request.GuildName))
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "该工会已经存在";
                sender.SendResponse();
                return;
            }
            GuildManager.Instance.CreateGuild(request.GuildName, request.GuildName,character);
            sender.Session.Response.guildCreate.guildInfo=character.Guild.GuildInfo(character);
            sender.Session.Response.guildCreate.Result = Result.Success;
            sender.SendResponse();
        }

        void OnGuildList(NetConnection<NetSession> sender, GuildListRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildList::character:[{0} {1}] ", character.Id, character.Info.Name);
            sender.Session.Response.guildList = new GuildListResponse();
            sender.Session.Response.guildList.Guilds.AddRange(GuildManager.Instance.GetGuildsInfo());
            sender.Session.Response.guildList.Result = Result.Success;
            sender.SendResponse();
        }

        /// <summary>
        /// 收到公会加入请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void OnGuildJoinRequest(NetConnection<NetSession> sender, GuildJoinRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinRequest::GuildId:{0} characterId:[{1}] {2}", request.Apply.GuildId,request.Apply.characterId,request.Apply.Name);
            var guild=GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if (guild==null)
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "公会不存在";
                sender.SendResponse();
                return;
            }
            request.Apply.characterId = character.Data.ID;
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;

            if(guild.JoinApply(request.Apply))
            {
                var leader=SessionManager.Instance.GetSession(guild.Data.LeaderID);
                if(leader!=null)
                {//给会长发送申请加入请求
                    leader.Session.Response.guildJoinReq = request;
                    leader.SendResponse();
                }
            }
            else
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "已经申请过了";
                sender.SendResponse();
            }
        }

        /// <summary>
        /// 收到加入公会响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        void OnGuildJoinResponse(NetConnection<NetSession> sender, GuildJoinResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinResponse::GuildId:{0} characterId:[{1}] {2}", response.Apply.GuildId, response.Apply.characterId, response.Apply.Name);
            
            var guild = GuildManager.Instance.GetGuild(response.Apply.GuildId);
            if(response.Result==Result.Success)
            {
                guild.JoinAppove(response.Apply);
            }

            var requester=SessionManager.Instance.GetSession(response.Apply.characterId);
            if (requester!=null)
            {
                requester.Session.Response.guildJoinRes = response;
                requester.Session.Response.guildJoinRes.Result = Result.Success;
                requester.Session.Response.guildJoinRes.Errormsg = "加入公会成功";
                requester.Session.Response.guildJoinRes.guildInfo = guild.GuildInfo(null);
                requester.SendResponse();
            }
        }

        void OnGuildLeave(NetConnection<NetSession> sender, GuildLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildLeave::character:{0}", character.Id);
            sender.Session.Response.guildLeave = new GuildLeaveResponse();

            if(character.Guild.Leave(character))
            {
                DBService.Instance.Entities.GuildMembers.RemoveRange(character.Guild.Data.Members);
                DBService.Instance.Entities.GuildApplies.RemoveRange(character.Guild.Data.Applies);
                DBService.Instance.Entities.Guilds.Remove(character.Guild.Data);
            }
            else
            {
                //DBService.Instance.Entities.GuildMembers.RemoveRange(character.Guild.Data.Members);
                DBService.Instance.Entities.GuildApplies.RemoveRange(character.Guild.Data.Applies);
            }
            character.Guild = null;
            sender.Session.Response.guildLeave.Result = Result.Success;

            DBService.Instance.Save();

            sender.SendResponse();
        }

        void OnGuildAdmin(NetConnection<NetSession> sender, GuildAdminRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildAdmin::character:{0}", character.Id);
            sender.Session.Response.guildAdmin = new GuildAdminResponse();

            if(character.Guild==null)
            {
                sender.Session.Response.guildAdmin.Result = Result.Failed;
                sender.Session.Response.guildAdmin.Errormsg = "您还没有公会";
                sender.SendResponse();
                return;
            }

            character.Guild.ExecuteAdmin(message.Command,message.Target, character.Id);

            var target=SessionManager.Instance.GetSession(message.Target);
            if(target!=null)
            {
                target.Session.Response.guildAdmin = new GuildAdminResponse();
                target.Session.Response.guildAdmin.Result = Result.Success;
                target.Session.Response.guildAdmin.Command = message;
                target.SendResponse();
            }

            sender.Session.Response.guildAdmin.Result = Result.Success;
            sender.Session.Response.guildAdmin.Command = message;
            sender.SendResponse();
        }
    }
}
