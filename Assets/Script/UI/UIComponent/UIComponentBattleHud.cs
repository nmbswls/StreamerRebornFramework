using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{
    public class UIComponentBattleHud : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            RegisterEvents();


            m_cardContainer.BindFields();
        }

        /// <summary>
        /// </summary>
        /// <param name="uiName"></param>
        public override void Initlize(string uiName)
        {
            base.Initlize(uiName);
        }

        public override void Clear()
        {
            base.Clear();
            UnregisterEvents();

            if (m_cardContainer != null)
                m_cardContainer.Clear();
        }

        #region 事件

        /// <summary>
        /// 注册
        /// </summary>
        private void RegisterEvents()
        {
        }

        /// <summary>
        /// 销毁
        /// </summary>
        private void UnregisterEvents()
        {
        }

        #endregion

        #region 绑定区域

        [AutoBind("./CardContainer")]
        public BattleCardContainer m_cardContainer;

        #endregion
    }
}

