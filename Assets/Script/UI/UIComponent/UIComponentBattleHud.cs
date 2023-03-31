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
            m_audienceContainer.BindFields();

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
        }

        #region �¼�

        /// <summary>
        /// ע��
        /// </summary>
        private void RegisterEvents()
        {
        }

        /// <summary>
        /// ����
        /// </summary>
        private void UnregisterEvents()
        {
        }

        #endregion

        #region ������

        [AutoBind("./CardContainer")]
        public BattleCardContainer m_cardContainer;

        [AutoBind("./AudienceContainer")]
        public BattleAudienceContainer m_audienceContainer;

        [AutoBind("./ArrowHint")]
        public ParabolaArrowController m_arrowHint;

        #endregion
    }
}

