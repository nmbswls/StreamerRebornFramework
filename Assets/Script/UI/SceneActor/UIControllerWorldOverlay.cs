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
    /// �;���actor�󶨵�ui�����ò�
    /// ����ui�߼���actor�ռ��Լ�����
    /// </summary>
    public class UIControllerWorldOverlay : UIControllerBase
    {
        public UIControllerWorldOverlay(string name) : base(name)
        {
        }
        /// <summary>
        /// ��ȡ��ǰ
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
        /// �Ƿ����
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
            // ������
            foreach(var bubble in m_currBubbles)
            {
                bubble.Tick(dt);
            }
        }


        #region �������

        /// <summary>
        /// ��ʾactorͷ������
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
        /// ����actorͷ������
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
        /// ����actorͷ������
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
        /// ��ǰ
        /// </summary>
        private List<UIComponentActorBubble> m_currBubbles = new List<UIComponentActorBubble>();

        /// <summary>
        /// ��ǰ�Ѱ󶨵�bubble
        /// </summary>
        private Queue<UIComponentActorBubble> m_cachedBubbles = new Queue<UIComponentActorBubble>();

        #endregion


        /// <summary>
        /// ��component
        /// </summary>
        public UIComponentWorldOverlay m_compRoot;

        #region װ����Ϣ

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

