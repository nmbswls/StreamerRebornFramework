using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class UIComponentEntryStartup : UIComponentBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiName"></param>
        public override void Initlize(string uiName)
        {
            base.Initlize(uiName);
        }

        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            SetButtonClickListener("m_newGameButton", OnNewGameButtonClick);
            SetButtonClickListener("m_oldGameButton", OnOldGameButtonClick);

            m_savingPanel.BindFields();
            m_savingPanel.gameObject.SetActive(false);
            m_savingPanel.EventOnConfirmSaving += OnConfirmSaving;
        }

        #region 事件回调

        /// <summary>
        /// 新游戏
        /// </summary>
        private void OnNewGameButtonClick(UIComponentBase _)
        {
            m_savingPanel.gameObject.SetActive(false);
            EventOnNewGame?.Invoke();
        }

        /// <summary>
        /// 老游戏
        /// </summary>
        /// <param name="cliecked"></param>
        private void OnOldGameButtonClick(UIComponentBase _)
        {
            GameManager.Instance.SavingManager.CollectSaveSummaryInfo();
            var summaries = GameManager.Instance.SavingManager.CollectedSavingSummary;
            m_savingPanel.UpdateSaveList(summaries);
            m_savingPanel.gameObject.SetActive(false);

            EventOnNewGame?.Invoke();
        }

        /// <summary>
        /// 确认进入游戏
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <param name="isNew"></param>
        private void OnConfirmSaving(int saveIdx, bool isNew)
        {
            if (isNew)
            {
                // 缓存
                m_newSaveIdx = saveIdx;
                EventOnNewGame?.Invoke();
            }
            else
            {
                EventOnLoadSaving?.Invoke(saveIdx);
            }
        }


        #endregion

        protected int m_newSaveIdx = 0;

        #region 事件

        /// <summary>
        /// 新建游戏
        /// </summary>
        public Action EventOnNewGame;

        /// <summary>
        /// 确认加载存档 p1 存档下标
        /// </summary>
        public Action<int> EventOnLoadSaving;

        #endregion

        #region 绑定区域

        [AutoBind("./EnterButton")]
        public Button m_enterButton;

        [AutoBind("./ExitButton")]
        public Button m_exitButton;

        [AutoBind("./SavingPanel")]
        public UIComponentEntryStartupSaving m_savingPanel;

        #endregion
    }
}



