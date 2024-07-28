﻿using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public interface IEntityController
    {
        void PlayAnim(string name);
        void SetStandby(bool standby);
    }
}