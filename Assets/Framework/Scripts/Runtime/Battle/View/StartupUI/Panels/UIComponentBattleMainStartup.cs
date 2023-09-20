using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class UIComponentBattleMainStartup : UIComponentBase
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
            SetButtonClickListener(nameof(m_fakeUseSkilButton), OnFakeUseSkilButtonClick);
            SetButtonClickListener(nameof(m_fakeEndTurnButton), OnFakeNextTurnButtonClick);
            
        }

        /// <summary>
        /// ��ʾ����ѡ��ui
        /// </summary>
        public void ShowChoosePanel(Action<int> onChooseEnd)
        {
            Debug.Log("��ʾѡ�����");
            onChooseEnd?.Invoke(0);
        }


        #region �¼��ص�

        /// <summary>
        /// fake ʹ�ü��ܱ����
        /// </summary>
        /// <param name=""></param>
        protected void OnFakeUseSkilButtonClick(UIComponentBase comp)
        {
            EventOnFakeUseSkill?.Invoke();
        }


        protected void OnFakeNextTurnButtonClick(UIComponentBase comp)
        {
            EventOnFakeEndTurn?.Invoke();
        }

        #endregion

        #region �¼�

        /// <summary>
        /// ʹ�ü���
        /// </summary>
        public event Action EventOnFakeUseSkill;

        /// <summary>
        /// �����غ�
        /// </summary>
        public event Action EventOnFakeEndTurn;

        #endregion

        #region ������

        [AutoBind("./UseSkilButton")]
        public Button m_fakeUseSkilButton;

        [AutoBind("./EndTurnButton")]
        public Button m_fakeEndTurnButton;

        #endregion
    }
}



