using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Scene
{
    public class SceneLayerUI : SceneLayerBase
    {
        protected override void OnInit()
        {
            m_layerCanvas = GetComponent<Canvas>();
            m_layerCanvasGroup = GetComponent<CanvasGroup>();
        }
        public Canvas LayerCanvas
        {
            get { return m_layerCanvas; }
        }

        public CanvasGroup LayerCanvasGroup
        {
            get { return m_layerCanvasGroup; }
        }

        protected Canvas m_layerCanvas;
        protected CanvasGroup m_layerCanvasGroup;

        public void UpdateLayerSortingOrder()
        {

        }

        /// <summary>
        /// 保持更换parent之前的offset
        /// </summary>
        /// <param name="go"></param>
        protected override void OnAttachGameObject(GameObject go)
        {
            RectTransform rTrans = go.transform as RectTransform;
            Vector2 anchorMin = rTrans.anchorMin;
            Vector2 anchorMax = rTrans.anchorMax;
            Vector2 offsetMin = rTrans.offsetMin;
            Vector2 offsetMax = rTrans.offsetMax;
            base.OnAttachGameObject(go);
            rTrans.anchorMin = anchorMin;
            rTrans.anchorMax = anchorMax;
            rTrans.offsetMin = offsetMin;
            rTrans.offsetMax = offsetMax;
        }


        private List<Canvas> m_childCanvasCache = new List<Canvas>();
    }
}

