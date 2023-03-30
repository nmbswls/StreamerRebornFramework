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

            m_arrowHint.gameObject.SetActive(false);
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

        public override void Tick(float dt)
        {
            base.Tick(dt);
            m_cardContainer?.Tick(dt);

            // 显示hint
            if(m_cardContainer.PreviewCards.Count > 0)
            {
                m_arrowHint.gameObject.SetActive(true);
                var fromPos = m_cardContainer.UseCardPreviewRoot.position;
                var toPos = Vector3.zero;
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                    Vector3 mousePosOnScreen = Input.mousePosition;
                    mousePosOnScreen.z = screenPos.z;
                    toPos = Camera.main.ScreenToWorldPoint(mousePosOnScreen);
                    toPos.z = fromPos.z;
                }
                m_arrowHint.SetParabolaPoints(fromPos, toPos, 10, m_arrowHint.HightDirection);
            }
            else
            {
                m_arrowHint.gameObject.SetActive(false);
            }
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


        [AutoBind("./ArrowHint")]
        public ParabolaArrowController m_arrowHint;

        #endregion
    }
}

