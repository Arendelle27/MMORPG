using Services;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIGUILD
{
    public class UIGuildList : UIWindow
    {
        public GameObject itemPrefab;
        public ListView listMain;
        public Transform itemRoot;
        public UIGuildInfo uiInfo;
        public UIGuildItem selectedItem;
        void Start()
        {
            this.listMain.onItemSelected += this.OnGuildMemberSelected;
            this.uiInfo.Info = null;
            GuildService.Instance.OnGuildListResult += UpdateGuildList;

            GuildService.Instance.SendGuildListRequest();
        }

        private void OnDestroy()
        {
            GuildService.Instance.OnGuildListResult -= UpdateGuildList;
        }

        void UpdateGuildList(List<NGuildInfo> guilds)
        {
            ClearList();
            InitItems(guilds);
        }

        public void OnGuildMemberSelected(ListView.ListViewItem item)
        {
            this.selectedItem = item as UIGuildItem;
            this.uiInfo.Info = this.selectedItem.Info;
        }

        /// <summary>
        /// 初始化公会列表
        /// </summary>
        /// <param name="guilds"></param>
        void InitItems(List<NGuildInfo> guilds)
        {
            foreach (var item in guilds)
            {
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);
                UIGuildItem ui = go.GetComponent<UIGuildItem>();
                ui.SetGuildInfo(item);
                this.listMain.AddItem(ui);
            }
        }

        void ClearList()
        {
            this.listMain.RemoveAll();
        }

        public void OnClickJoin()
        {
            if (this.selectedItem == null)
            {
                MessageBox.Show("请选择要加入的公会");
                return;
            }
            MessageBox.Show(string.Format("确定要加入[{0}]公会吗？", this.selectedItem.Info.GuildName), "申请加入公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
            {
                GuildService.Instance.SendGuildJoinRequest(this.selectedItem.Info.Id);
            };
        }
    }
}
