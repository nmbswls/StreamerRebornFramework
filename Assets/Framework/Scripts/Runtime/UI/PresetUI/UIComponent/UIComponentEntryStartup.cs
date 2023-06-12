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

            SetButtonClickListener("m_enterButton", OnEnterButtonClick);
        }

        #region �¼��ص�

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="cliecked"></param>
        private void OnEnterButtonClick(UIComponentBase cliecked)
        {
            EventOnEnter?.Invoke();
        }

        #endregion

        protected int m_newSaveIdx = 0;

        #region �¼�

        /// <summary>
        /// ���enter
        /// </summary>
        public Action EventOnEnter;

        #endregion

        #region ������

        [AutoBind("./EnterButton")]
        public Button m_enterButton;

        [AutoBind("./ExitButton")]
        public Button m_exitButton;

        #endregion
    }
}



