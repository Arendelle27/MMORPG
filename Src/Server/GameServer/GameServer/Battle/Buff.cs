﻿using Common;
using Common.Data;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class Buff
    {
        public int BuffId;
        private Creature Owner;
        private BuffDefine Define;
        private BattleContext context;
        private float time=0f;
        private int hit;

        public bool Stoped { get; internal set; }

        public Buff(int buffID, Creature owner, BuffDefine define, BattleContext context)
        {
            this.BuffId = buffID;
            this.Owner = owner;
            this.Define = define;
            this.context = context;

            this.OnAdd();
        }

        private void OnAdd()
        {
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.Owner.EffectMgr.AddEffect(this.Define.Effect);
            }
            this.AddAttr();

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Add
            };
            context.Battle.AddBuffAction(buff);
        }

        private void AddAttr()
        {
            if(this.Define.DEFRatio!=0)
            {
                this.Owner.Attributes.Buff.DEF+=this.Owner.Attributes.Basic.DEF*this.Define.DEFRatio;
            }
        }

        private void OnRemove()
        {
            RemoveAttr();
            Stoped = true;
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.Owner.EffectMgr.RemoveEffect(this.Define.Effect);
            }

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Remove
            };
            context.Battle.AddBuffAction(buff);
        }

        private void RemoveAttr()
        {
            if(this.Define.DEFRatio!=0)
            {
                this.Owner.Attributes.Buff.DEF-=this.Owner.Attributes.Basic.DEF*this.Define.DEFRatio;
                this.Owner.Attributes.InitBasicAttributes();
            }
        }

        internal void Update()
        {
            if(Stoped)
            {
                return;
            }
            this.time+=Time.deltaTime;
            if(this.Define.Interval>0)
            {//间隔时间
                if(this.time>this.Define.Interval*(this.hit+1))
                {
                    this.DoBuffDamage();
                }
            }
            if(time>this.Define.Duration)
            {
                this.OnRemove();
            }
        }

        private void DoBuffDamage()
        {
            this.hit++;
            NDamageInfo damage = this.CalcBuffDamage(context.Caster);
            Log.InfoFormat("Buff[{0}].DoBuffDamage[{1} Damage:{2} Crit:{3}", this.Define.Name, this.Owner.Info.Name, damage.Damage,damage.Crit);
            this.Owner.DoDamage(damage);

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Hit,
                Damage = damage
            };
            context.Battle.AddBuffAction(buff);
        }

        private NDamageInfo CalcBuffDamage(Creature caster)
        {
            float ad=this.Define.AD + caster.Attributes.AD*this.Define.ADFactor;
            float ap=this.Define.AP + caster.Attributes.AP*this.Define.APFactor;

            float addmg=ad*(1-this.Owner.Attributes.DEF/(this.Owner.Attributes.DEF+100));
            float apdmg=ap*(1-this.Owner.Attributes.MDEF/(this.Owner.Attributes.MDEF+ 100));

            float final=addmg+apdmg;

            NDamageInfo damage = new NDamageInfo();
            damage.entityId = this.Owner.entityId;
            damage.Damage = Math.Max(1, (int)final);
            return damage;
        }
    }
}
