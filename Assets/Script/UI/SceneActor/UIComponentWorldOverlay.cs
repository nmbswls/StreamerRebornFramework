using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{
    /// <summary>
    /// ����� ���ṩ����
    /// </summary>
    public class UIComponentWorldOverlay : UIComponentBase
    {

        public GameObject BubblePrefab;

        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
        }

        /// <summary>
        /// </summary>
        /// <param name="uiName"></param>
        public override void Initlize(string uiName)
        {
            base.Initlize(uiName);
        }

        #region ������

        [AutoBind("./PoolRoot")]
        public Transform m_poolRoot;

        [AutoBind("./Bubble")]
        public Transform m_bubbleContainer;

        #endregion
    }
}

