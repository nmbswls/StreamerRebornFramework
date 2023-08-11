using My.Framework.Runtime.UI;
using My.Runtime;
using StreamerReborn.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StreamerReborn
{
    /// <summary>
    /// 和具体actor绑定的ui放至该层
    /// 具体ui逻辑由actor空间自己控制
    /// </summary>
    public class UIControllerWorldOverlay : UIControllerBase
    {
        public UIControllerWorldOverlay(string name) : base(name)
        {
        }
        /// <summary>
        /// 获取当前
        /// </summary>
        /// <returns></returns>
        public static UIControllerWorldOverlay GetCurrent()
        {
            var ctrl = UIManager.Instance.FindUIControllerByName("WorldOverlay") as UIControllerWorldOverlay;
            if (ctrl != null) 
                return ctrl;

            var intent = new UIIntent("WorldOverlay");
            return UIManager.Instance.StartUIController(intent) as UIControllerWorldOverlay;
        }

        /// <summary>
        /// 是否就绪
        /// </summary>
        /// <returns></returns>
        public static bool IsReady()
        {
            var ctrl = UIManager.Instance.FindUIControllerByName("WorldOverlay");
            if (ctrl == null) return false;
            if(!ctrl.IsLayerInStack())
            {
                return false;
            }
            return true;
        }

        protected override void RefreshView()
        {

        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compRoot = m_uiCompArray[0] as UIComponentWorldOverlay;
        }

        protected override void OnTick(float dt)
        {
            // 驱动力
            foreach(var bubble in m_currBubbles)
            {
                bubble.Tick(dt);
            }
        }


        #region 气泡相关

        /// <summary>
        /// 显示actor头顶气泡
        /// </summary>
        /// <returns></returns>
        public UIComponentActorBubble FetchActorBubble(ActorControllerBase actor)
        {
            UIComponentActorBubble bubbleComp = null;
            if (m_cachedBubbles.Count > 0)
            {
                bubbleComp = m_cachedBubbles.Dequeue();
                bubbleComp.gameObject.SetActive(true);
            }
            else
            {
                var newGo = GameObject.Instantiate(m_compRoot.BubblePrefab, m_compRoot.m_bubbleContainer);
                bubbleComp = newGo.GetComponent<UIComponentActorBubble>();
                bubbleComp.BindFields();
            }
            bubbleComp.Initialize(actor);
            m_currBubbles.Add(bubbleComp);
            return bubbleComp;
        }

        /// <summary>
        /// 销毁actor头顶气泡
        /// </summary>
        /// <returns></returns>
        public void ReleaseActorBubble(UIComponentActorBubble compBubble, bool reuse = true)
        {
            m_currBubbles.Remove(compBubble);
            if(reuse)
            {
                compBubble.bindTarget = null;
                m_cachedBubbles.Enqueue(compBubble);
                compBubble.transform.SetParent(m_compRoot.m_poolRoot);
                compBubble.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(compBubble.gameObject);
            }
        }

        /// <summary>
        /// 隐藏actor头顶气泡
        /// </summary>
        /// <returns></returns>
        public void HideActorBubble(ActorControllerBase actor)
        {
            var comp = actor.m_compBubble;
            if (comp == null) return;
            m_currBubbles.Remove(comp);
            comp.bindTarget = null;
            m_cachedBubbles.Enqueue(comp);
        }


        /// <summary>
        /// 当前
        /// </summary>
        private List<UIComponentActorBubble> m_currBubbles = new List<UIComponentActorBubble>();

        /// <summary>
        /// 当前已绑定的bubble
        /// </summary>
        private Queue<UIComponentActorBubble> m_cachedBubbles = new Queue<UIComponentActorBubble>();

        #endregion


        /// <summary>
        /// 主component
        /// </summary>
        public UIComponentWorldOverlay m_compRoot;

        #region 装配信息

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "WorldOverlay",
                m_layerResPath = "Assets/RuntimeAssets/UI/UIPrefab/WorldOverlay.prefab",
            },
        };
        protected override UIComponentDesc[] UIComponentDescArray
        {
            get { return m_uiComponentDescArray; }
        }
        private readonly UIComponentDesc[] m_uiComponentDescArray =
        {
            new UIComponentDesc
            {
                m_attachLayerName = "WorldOverlay",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentWorldOverlay).ToString(),
                m_compName = "WorldOverlay"
            },
        };

        #endregion
    }
}

