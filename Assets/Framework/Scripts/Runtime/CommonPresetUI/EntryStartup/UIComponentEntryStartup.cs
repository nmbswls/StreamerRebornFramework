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
        /// ��ɰ󶨻ص�
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

        #region �¼��ص�

        /// <summary>
        /// ����Ϸ
        /// </summary>
        private void OnNewGameButtonClick(UIComponentBase _)
        {
            m_savingPanel.gameObject.SetActive(false);
            EventOnNewGame?.Invoke();
        }

        /// <summary>
        /// ����Ϸ
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
        /// ȷ�Ͻ�����Ϸ
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <param name="isNew"></param>
        private void OnConfirmSaving(int saveIdx, bool isNew)
        {
            if (isNew)
            {
                // ����
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

        #region �¼�

        /// <summary>
        /// �½���Ϸ
        /// </summary>
        public Action EventOnNewGame;

        /// <summary>
        /// ȷ�ϼ��ش浵 p1 �浵�±�
        /// </summary>
        public Action<int> EventOnLoadSaving;

        #endregion

        #region ������

        [AutoBind("./EnterButton")]
        public Button m_enterButton;

        [AutoBind("./ExitButton")]
        public Button m_exitButton;

        [AutoBind("./SavingPanel")]
        public UIComponentEntryStartupSaving m_savingPanel;

        #endregion
    }
}



