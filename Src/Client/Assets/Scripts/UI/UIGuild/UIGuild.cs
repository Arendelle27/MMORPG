using Managers;
using Services;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIGUILD
{
    public class UIGuild : UIWindow
    {
        public GameObject itemPrefab;
        public ListView listMain;
        public Transform itemRoot;
        public UIGuildInfo uiInfo;
        public UIGuildMemberItem selectedItem;

        public GameObject panelAdmin;
        public GameObject panelLeader;
        void Start()
        {
            GuildService.Instance.OnGuildUpdate = UpdateUI;
            this.listMain.onItemSelected += this.OnGuildMemberSelected;
            this.UpdateUI();
        }

        private void OnDestroy()
        {
            GuildService.Instance.OnGuildUpdate -= UpdateUI;
        }

        void UpdateUI()
        {
            this.uiInfo.Info = GuildManager.Instance.guildInfo;
            ClearList();
            InitItems();

            this.panelAdmin.SetActive(GuildManager.Instance.myMemberInfo.Title > GuildTitle.None);
            this.panelLeader.SetActive(GuildManager.Instance.myMemberInfo.Title == GuildTitle.President);
        }

        public void OnGuildMemberSelected(ListView.ListViewItem item)
        {
            this.selectedItem = item as UIGuildMemberItem;
        }

        /// <summary>
        /// 初始化公会成员列表
        /// </summary>
        /// <param name="guilds"></param>
        void InitItems()
        {
            foreach (var item in GuildManager.Instance.guildInfo.Members)
            {
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);
                UIGuildMemberItem ui = go.GetComponent<UIGuildMemberItem>();
                ui.SetGuildMemberInfo(item);
                this.listMain.AddItem(ui);
            }
        }

        void ClearList()
        {
            this.listMain.RemoveAll();
        }

        public void OnClickAppliesList()
        {
            UIManager.Instance.Show<UIGuildApplyLIst>();
        }

        public void OnClickLeave()
        {
            GuildService.Instance.SendGuildLeaveRequest();
        }

        public void OnClickChat()
        {

        }

        public void OnClickKickout()
        {
            if (selectedItem == null)
            {
                MessageBox.Show("请选择要踢出的成员");
                return;
            }
            MessageBox.Show(string.Format("要踢【{0}】出公会吗？", this.selectedItem.Info.Info.Name), "踢出公会", MessageBoxType.Confirm, "踢", "取消").OnYes = () => {
                GuildService.Instance.SendAdminCommand(GuildAdminCommand.Kickout, this.selectedItem.Info.Info.Id);
            };
        }

        public void OnClickPromote()
        {
            if (selectedItem == null)
            {
                MessageBox.Show("请选择要晋升的成员");
                return;
            }
            if (this.selectedItem.Info.Title != GuildTitle.None)
            {
                MessageBox.Show("对方已经不是普通成员了");
                return;
            }
            MessageBox.Show(string.Format("要晋升【{0}】为副会长吗？", this.selectedItem.Info.Info.Name), "晋升副会长", MessageBoxType.Confirm, "晋升", "取消").OnYes = () => {
                GuildService.Instance.SendAdminCommand(GuildAdminCommand.Promote, this.selectedItem.Info.Info.Id);
            };
        }

        public void OnClickDepose()
        {
            if (selectedItem == null)
            {
                MessageBox.Show("请选择要罢免的成员");
            }
            if (selectedItem.Info.Title == GuildTitle.None)
            {
                MessageBox.Show("对方已经是普通成员了");
                return;
            }
            if (selectedItem.Info.Title == GuildTitle.President)
            {
                MessageBox.Show("会长不能被罢免");
                return;
            }
            MessageBox.Show(string.Format("要罢免【{0}】的职务吗？", this.selectedItem.Info.Info.Name), "罢免职务", MessageBoxType.Confirm, "罢免", "取消").OnYes = () =>
            {
                GuildService.Instance.SendAdminCommand(GuildAdminCommand.Depost, this.selectedItem.Info.Info.Id);
            };
        }

        public void OnClickTransfer()
        {
            if (selectedItem == null)
            {
                MessageBox.Show("请选择要转让的成员");
                return;
            }
            MessageBox.Show(string.Format("要将会长转让给【{0}】吗？", this.selectedItem.Info.Info.Name), "转让会长", MessageBoxType.Confirm, "转让", "取消").OnYes = () =>
            {
                GuildService.Instance.SendAdminCommand(GuildAdminCommand.Transfer, this.selectedItem.Info.Info.Id);
            };
        }

        public void OnClickSetNotice()
        {
            MessageBox.Show("拓展作业");
        }
    }
}

