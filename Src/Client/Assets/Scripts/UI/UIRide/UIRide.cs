using Managers;
using Models;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIRIDE
{
    public class UIRide : UIWindow
    {
        public Text descript;
        public GameObject itemPrefab;
        public ListView listMain;
        private UIRideItem selectedItem;
        void Start()
        {
            RefreshUI();
            EquipManager.Instance.OnEquipChanaged+=RefreshUI;
            this.listMain.onItemSelected+=this.OnItemSelected;

        }

        private void OnDestroy()
        {
            EquipManager.Instance.OnEquipChanaged -= RefreshUI;
        }

        public void OnItemSelected(ListView.ListViewItem item)
        {
            this.selectedItem=item as UIRideItem;
            this.descript.text=this.selectedItem.item.Define.Description;
        }

        void RefreshUI()
        {
            ClearItems();
            InitItems();
        }

        /// <summary>
        /// 初始化所有的装备列表
        /// </summary>
        void InitItems()
        {
            foreach(var kv in ItemManager.Instance.Items)
            {
                if(kv.Value.Define.Type==ItemType.Ride&&(kv.Value.Define.LimitClass==CharacterClass.None||kv.Value.Define.LimitClass==User.Instance.CurrentCharacter.Class))
                {
                    //以装备的就不显示了
                    if (EquipManager.Instance.Contains(kv.Key))
                        continue;
                    GameObject go=Instantiate(itemPrefab,this.listMain.transform);
                    UIRideItem ui=go.GetComponent<UIRideItem>();
                    ui.SetEquipItem(kv.Value,this,false);
                    this.listMain.AddItem(ui);
                }
            }
        }

        void ClearItems()
        {
            this.listMain.RemoveAll();
        }

        public void DoRide()
        {
            if(this.selectedItem==null)
            {
                MessageBox.Show("请选择要召唤的坐骑","提示");
                return;
            }
            User.Instance.Ride(this.selectedItem.item.Id);
        }
    }
}
