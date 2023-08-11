using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class UIComponentEntryStartupSaving : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            SetButtonClickListener("m_confirmButton", OnEnterButtonClick);
            SetButtonClickListener("m_deleteButton", OnDeleteButtonClick);
            m_confirmButton.enabled = false;
        }

        public void UpdateSaveList(Dictionary<int, SavingSummary> summarys)
        {
            m_summarys = summarys;

            while (m_saveItemList.Count < SavingManager.MaxSaveCount)
            {
                var newGo = GameObject.Instantiate(m_saveItemPrefab, m_saveScrollContainer);
                var saveItem = newGo.AddComponent<UIComponentEntryStartupSavingItem>();
                saveItem.BindFields();
                m_saveItemList.Add(saveItem);
                newGo.transform.SetParent(m_saveScrollContainer);
                newGo.transform.localScale = Vector3.one;
                saveItem.OnSaveSelect += OnSaveItemClick;
                newGo.SetActive(true);
            }

            for (int saveIdx = 1; saveIdx <= SavingManager.MaxSaveCount; saveIdx++)
            {
                m_summarys.TryGetValue(saveIdx, out var summary);
                m_saveItemList[saveIdx - 1].SetSaveInfo(saveIdx, summary);
                m_saveItemList[saveIdx - 1].SetSelect(false);
            }

            m_selectSaveIdx = 0;
            m_deleteButton.interactable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnDeleteButtonClick(UIComponentBase _)
        {
            if (!m_summarys.ContainsKey(m_selectSaveIdx))
            {
                return;
            }

            UIControllerMessageBoxSimple.ShowMessage("确认删除吗？", () => {
                GameManager.Instance.SavingManager.DeleteSavingFile(m_selectSaveIdx);
                UpdateSaveList(GameManager.Instance.SavingManager.CollectedSavingSummary);
            });
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="savingIndex"></param>
        protected void OnSaveItemClick(int savingIndex)
        {
            if (m_selectSaveIdx == savingIndex)
            {
                return;
            }
            if (m_selectSaveIdx != 0)
            {
                m_saveItemList[m_selectSaveIdx - 1].SetSelect(false);
            }
            m_saveItemList[savingIndex - 1].SetSelect(true);

            m_selectSaveIdx = savingIndex;
            m_confirmButton.enabled = true;

            if (!m_summarys.ContainsKey(m_selectSaveIdx))
            {
                m_deleteButton.interactable = false;
            }
            else
            {
                m_deleteButton.interactable = true;
            }
        }

        protected void OnEnterButtonClick(UIComponentBase _)
        {
            bool isEmpty = !m_summarys.ContainsKey(m_selectSaveIdx);
            EventOnConfirmSaving?.Invoke(m_selectSaveIdx, isEmpty);
        }

        private int m_selectSaveIdx = 0;

        /// <summary>
        /// 存档列表
        /// </summary>
        protected Dictionary<int, SavingSummary> m_summarys = new Dictionary<int, SavingSummary>();

        /// <summary>
        /// 子组件列表
        /// </summary>
        private List<UIComponentEntryStartupSavingItem> m_saveItemList = new List<UIComponentEntryStartupSavingItem>();

        /// <summary>
        /// 确认加载事件
        /// </summary>
        public event Action<int, bool> EventOnConfirmSaving;

        #region 绑定

        [AutoBind("./ConfirmButton")]
        public Button m_confirmButton;

        [AutoBind("./DeleteButton")]
        public Button m_deleteButton;

        [AutoBind("./SaveList/Viewport/Content")]
        public Transform m_saveScrollContainer;

        [AutoBind("./SaveList/HeadItem")]
        public GameObject m_saveItemPrefab;

        #endregion
    }
}
