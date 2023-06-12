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

            SetButtonClickListener("m_enterButton", OnEnterButtonClick);
        }

        #region 事件回调

        /// <summary>
        /// 进入游戏案件
        /// </summary>
        /// <param name="cliecked"></param>
        private void OnEnterButtonClick(UIComponentBase cliecked)
        {
            EventOnEnter?.Invoke();
        }

        #endregion

        protected int m_newSaveIdx = 0;

        #region 事件

        /// <summary>
        /// 点击enter
        /// </summary>
        public Action EventOnEnter;

        #endregion

        #region 绑定区域

        [AutoBind("./EnterButton")]
        public Button m_enterButton;

        [AutoBind("./ExitButton")]
        public Button m_exitButton;

        #endregion
    }
}



