﻿using Common.Battle;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Battle
{
    public class EffectManager
    {
        private Creature Owner;

        Dictionary<BuffEffect, int> Effects = new Dictionary<BuffEffect, int>();

        public EffectManager(Creature owner)
        {
            this.Owner = owner;
        }

        public bool HasEffect(BuffEffect effect)
        {
            if(this.Effects.TryGetValue(effect,out int val))
            {
                return val > 0;
            }
            return false;
        }


        internal void AddEffect(BuffEffect effect)
        {
            Debug.LogFormat("[{0}] AddEffect:{1}", this.Owner.Name, effect);
            if (!this.Effects.ContainsKey(effect))
            {
                this.Effects[effect]=1;
            }
            else
            {
                this.Effects[effect] ++;
            }
        }

        internal void RemoveEffect(BuffEffect effect)
        {
            Debug.LogFormat("[{0}] RemoveEffect:{1}", this.Owner.Name, effect);
            if (this.Effects[effect] >0)
            {
                this.Effects[effect]--;
            }
        }
    }
}
