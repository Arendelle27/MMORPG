﻿using Common;
using Common.Data;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Models
{
    public class Item
    {
        public int Id;
        public int Count;
        public ItemDefine Define;
        public EquipDefine EquipInfo;
        public Item(NItemInfo item):
            this(item.Id,item.Count)
        {

        }

        public Item(int id,int count)
        {
            this.Id = id;
            this.Count = count;
            DataManager.Instance.Items.TryGetValue(this.Id, out this.Define);
            DataManager.Instance.Equips.TryGetValue(this.Id, out this.EquipInfo);
        }

        public override string ToString()
        {
            return string.Format("Id:{0},Count:{1}", this.Id, this.Count);
        }

    }
}