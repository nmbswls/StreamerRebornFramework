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
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            SetButtonClickListener(nameof(m_fakeUseSkilButton), OnFakeUseSkilButtonClick);
            SetButtonClickListener(nameof(m_fakeEndTurnButton), OnFakeNextTurnButtonClick);
            
        }

        /// <summary>
        /// 显示特殊选怎ui
        /// </summary>
        public void ShowChoosePanel(Action<int> onChooseEnd)
        {
            Debug.Log("显示选择界面");
            onChooseEnd?.Invoke(0);
        }


        #region 事件回调

        /// <summary>
        /// fake 使用技能被点击
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

        #region 事件

        /// <summary>
        /// 使用技能
        /// </summary>
        public event Action EventOnFakeUseSkill;

        /// <summary>
        /// 结束回合
        /// </summary>
        public event Action EventOnFakeEndTurn;

        #endregion

        #region 绑定区域

        [AutoBind("./UseSkilButton")]
        public Button m_fakeUseSkilButton;

        [AutoBind("./EndTurnButton")]
        public Button m_fakeEndTurnButton;

        #endregion
    }
}



