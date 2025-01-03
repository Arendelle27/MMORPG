﻿using Common;
using Common.Utils;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Services;
using log4net.Core;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.Models
{
    class Guild
    {
        public int Id { get { return this.Data.Id; } }
        public string Name { get { return this.Data.Name; } }
        public List<Character> Members = new List<Character>();

        public double timestamp;
        public TGuild Data;

        public Guild(TGuild guild)
        {
            this.Data = guild;
        }

        public bool JoinApply(NGuildApplyInfo apply)
        {
            var oldApply = this.Data.Applies.FirstOrDefault(a => a.CharacterId == apply.characterId);
            //if(oldApply!=null)
            //{
            //    return false;
            //}

            var dbApply=DBService.Instance.Entities.GuildApplies.Create();
            dbApply.GuildId = apply.GuildId;
            dbApply.CharacterId = apply.characterId;
            dbApply.Class = apply.Class;
            dbApply.Level = apply.Level;
            dbApply.Name = apply.Name;
            dbApply.ApplyTime = DateTime.Now;

            DBService.Instance.Entities.GuildApplies.Add(dbApply);
            this.Data.Applies.Add(dbApply);

            DBService.Instance.Save();

            this.timestamp = Time.timestamp;
            return true;
        }
        
        public bool JoinAppove(NGuildApplyInfo apply)
        {
            var oldApply = this.Data.Applies.FirstOrDefault(a => a.CharacterId == apply.characterId&&a.Result==0);
            if(oldApply==null)
            {
                return false;
            }

            oldApply.Result = (int)apply.Result;

            if(apply.Result==ApplyResult.Accept)
            {
                this.AddMember(apply.characterId,apply.Name,apply.Class,apply.Level,GuildTitle.None);
            }

            DBService.Instance.Save();

            this.timestamp = Time.timestamp;
            return true;
        }

        public void AddMember(int characterId,string name,int @class,int level,GuildTitle title)
        {
            DateTime now = DateTime.Now;
            TGuildMember dbMember = new TGuildMember()
            {
                CharacterId = characterId,
                Name = name,
                Class = @class,
                Level = level,
                Title = (int)title,
                JoinTime = now,
                LastTime = now
            };
            this.Data.Members.Add(dbMember);
            var character=CharacterManager.Instance.GetCharacter(characterId);
            if(character!=null)
            {
                character.Data.GuildId=this.Id;
                character.Guild=this;
            }
            else
            {
                //DBService.Instance.Entities.Database.ExecuteSqlCommand($"UPDATE characters SET GuildId=@p0",this.Id,characterId);
                TCharacter dbChar=DBService.Instance.Entities.Characters.SingleOrDefault(c=>c.ID==characterId);
                dbChar.GuildId=this.Id;
            }
            timestamp = Time.timestamp;
        }

        public bool Leave(Character member)
        {
            Log.InfoFormat("Leave Guild : {0}:{1}", member.Id, member.Info.Name);
            bool isClearGuild = false;
            if(this.Data.LeaderID==member.Id)
            {
                foreach (var m in this.Members)
                {
                    m.Data.GuildId = 0;
                }
                GuildManager.Instance.RemoveGuild(this);
                isClearGuild = true;
            }
            else
            {
                foreach (var m in this.Data.Members)
                {
                    if (m.CharacterId == member.Id)
                    {
                        member.Data.GuildId = 0;
                        DBService.Instance.Entities.GuildMembers.RemoveRange(this.Data.Members);
                        this.Data.Members.Remove(m);
                        break;
                    }
                }
            }
            timestamp = TimeUtil.timestamp;
            return isClearGuild;
        }

        public void PostProcess(Character from,NetMessageResponse message)
        {
            if(message.Guild==null)
            {
                message.Guild = new GuildResponse();
                message.Guild.Result = Result.Success;
                message.Guild.guildInfo = this.GuildInfo(from);
            }
        }

        public NGuildInfo GuildInfo(Character from)
        {
            NGuildInfo info = new NGuildInfo()
            {
                Id=this.Id,
                GuildName=this.Name,
                Notice=this.Data.Notice,
                leaderId=this.Data.LeaderID,
                leaderName=this.Data.LeaderName,
                createTime=(long)TimeUtil.GetTimestamp(this.Data.CreateTime),
                memberCount=this.Data.Members.Count,
            };

            if(from!=null)
            {
                info.Members.AddRange(GetMemberInfos());
                if(from.Id==this.Data.LeaderID)
                {
                    info.Applies.AddRange(GetApplyInfos());
                }
            }
            return info;
        }

        List<NGuildMemberInfo> GetMemberInfos()
        {
            List<NGuildMemberInfo> members = new List<NGuildMemberInfo>();

            foreach (var member in this.Data.Members)
            {
                var memberInfo = new NGuildMemberInfo
                {
                    Id = member.Id,
                    characterId=member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime),
                };

                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);
                if (character != null) 
                {
                    memberInfo.Info=character.GetBasicInfo();
                    memberInfo.Status = 1;
                }
                else
                {
                    memberInfo.Info=this.GetMemberInfo(member);
                    memberInfo.Status = 0;
                }
                members.Add(memberInfo);
            }
            return members;
        }

        NCharacterInfo GetMemberInfo(TGuildMember member)
        {
            return new NCharacterInfo()
            {
                Id = member.CharacterId,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level,
            };
        }

        List<NGuildApplyInfo> GetApplyInfos()
        { 
            List<NGuildApplyInfo> applies=new List<NGuildApplyInfo>();
            foreach(var apply in this.Data.Applies)
            {
                if(apply.Result!=(int)ApplyResult.None)
                {
                    continue;
                }
                applies.Add(new NGuildApplyInfo()
                {
                    characterId=apply.CharacterId,
                    GuildId=apply.GuildId,
                    Name=apply.Name,
                    Class=apply.Class,
                    Level=apply.Level,
                    Result = (ApplyResult)apply.Result,
                });
            }
            return applies;
        }

        TGuildMember GetDBMember(int characterId)
        {
            foreach(var member in this.Data.Members)
            {
                if(member.CharacterId==characterId)
                {
                    return member;
                }
            }
            return null;
        }

        public void ExecuteAdmin(GuildAdminCommand command,int targetId,int sourceId)
        {
            var target = GetDBMember(targetId);
            var source = GetDBMember(sourceId);
            switch(command)
            {
                case GuildAdminCommand.Promote:
                    target.Title=(int)GuildTitle.VicePresident;
                    break;
                case GuildAdminCommand.Depost:
                    target.Title=(int)GuildTitle.None;
                    break;
                case GuildAdminCommand.Transfer:
                    target.Title = (int)GuildTitle.President;
                    source.Title=(int)GuildTitle.None;
                    this.Data.LeaderID=targetId;
                    this.Data.LeaderName=target.Name;
                    break;
                case GuildAdminCommand.Kickout:
                    this.Data.Members.Remove(target);
                    var character = CharacterManager.Instance.GetCharacter(target.Id);
                    if (character != null)
                    {
                        character.Data.GuildId = 0;
                    }
                    break;
            }
            DBService.Instance.Save();
            this.timestamp = TimeUtil.timestamp;
        }
    }
}
