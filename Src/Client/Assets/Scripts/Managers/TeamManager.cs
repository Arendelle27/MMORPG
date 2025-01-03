﻿using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers
{
    public class TeamManager:Singleton<TeamManager>
    {
        public void Init()
        {

        }

        public void UpdateTeamInfo(NTeamInfo team)
        {
            User.Instance.TeamInfo = team;
            this.ShowTeamUI(team!=null);
        }

        public void ShowTeamUI(bool show)
        {
            if(UIMain.Instance!=null)
            {
                UIMain.Instance.ShowTeamUI(show);
            }
        }
    }
}
