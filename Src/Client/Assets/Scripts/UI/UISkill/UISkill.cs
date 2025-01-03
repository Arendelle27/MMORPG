using Common.Battle;
using Models;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace UISKILL
{
    public class UISkill : UIWindow
    {
        public Text descript;
        public GameObject itemPrefab;
        public ListView ListMain;
        private UISkillItem selectedItem;

        void Start()
        {
            RefreshUI();
            this.ListMain.onItemSelected += this.OnItemSelected;
        }

        private void OnDestroy()
        {
            
        }

        public void OnItemSelected(ListView.ListViewItem item)
        {
            this.selectedItem = item as UISkillItem;
            this.descript.text = this.selectedItem.item.Define.Description;
        }

        void RefreshUI()
        {
            ClearItems();
            InitItems();
        }

        void InitItems()
        {
            var Skills = User.Instance.CurrentCharacter.SkillMgr.Skills;
            foreach (var skill in Skills)
            { 
                if(skill.Define.Type== SkillType.SKill)
                {
                    GameObject go=Instantiate(itemPrefab,this.ListMain.transform);
                    UISkillItem uiItem = go.GetComponent<UISkillItem>();
                    uiItem.SetItem(skill,this,false);
                     this.ListMain.AddItem(uiItem);
                }
            }
        }

        void ClearItems()
        {
            this.ListMain.RemoveAll();
        }

    }
}

