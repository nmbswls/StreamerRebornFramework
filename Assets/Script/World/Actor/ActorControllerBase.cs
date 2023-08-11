using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.Resource;
using My.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StreamerReborn.World
{
    /// <summary>
    /// 角色控制器基类 
    /// 
    /// </summary>
    public abstract class ActorControllerBase : PrefabComponentBase, IPointerClickHandler
    {
        public SceneActor m_logicActor;

        public virtual bool Initialize(int configId, SceneActor logicActor)
        {
            m_logicActor = logicActor;

            var childSpriteRenderers = m_showRoot.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in childSpriteRenderers)
            {
                m_showSpriteList.Add(sr);
            }

            return true;
        }

        public virtual void UnInitialize()
        {
        }

        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
        }


        protected virtual void Update()
        {
        }

        public void OnUILayerPrepare()
        {

        }

        #region 显示相关

        /// <summary>
        /// bubble组件 缓存
        /// </summary>
        public UIComponentActorBubble m_compBubble;

        /// <summary>
        /// 显示头顶气泡
        /// </summary>
        public void ShowHeadBubble(string bubbleStyle, float duration)
        {
            if(m_compBubble == null)
            {
                var overlayUI = UIControllerWorldOverlay.GetCurrent();
                m_compBubble = overlayUI.FetchActorBubble(this);
            }
            //m_compBubble.ShowBubble(bubbleStyle, duration);
        }

        /// <summary>
        /// 显示头顶气泡
        /// </summary>
        public void HideHeadBubble()
        {
            var overlayUI = UIControllerWorldOverlay.GetCurrent();
            overlayUI?.HideActorBubble(this);
        }

        /// <summary>
        /// 显示淡入淡出
        /// </summary>
        public virtual void SampleFadeIn(float normalisedTime)
        {
            gameObject.SetActive(true);
            foreach(var sr in m_showSpriteList)
            {
                sr.color = new Color(1,1,1, normalisedTime);
            }
        }

        #endregion

        public uint ActorInstIdGet()
        {
            return 0;
        }

        public virtual bool OnClick()
        {
            return true;
        }

        /// <summary>
        /// 监听点击
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick();
        }

        #region 绑定ui

        protected void GetUIRoot()
        {

        }

        /// <summary>
        /// 需要做显示的sprite部分
        /// </summary>
        protected List<SpriteRenderer> m_showSpriteList = new List<SpriteRenderer>(); 

        #endregion

        #region 绑定

        /// <summary>
        /// 绑定单个对象的实现
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="path"></param>
        /// <param name="initState"></param>
        /// <param name="fieldName"></param>
        /// <param name="ctrlName"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        protected override UnityEngine.Object BindFieldImpl(Type fieldType, string path, AutoBindAttribute.InitState initState, string fieldName, string ctrlName = null, bool optional = false)
        {
            UnityEngine.Object field = null;
            GameObject go = GetChildByPath(path);
            // 获取子节点
            if (go == null)
            {
                if (!optional)
                    Debug.LogError(string.Format("BindFields fail can not found child {0} uictrl:{1}", path, this.name), gameObject);
                return null;
            }

            // 如果域的类型的higameobject直接赋值为找到的节点
            field = base.BindFieldImpl(fieldType, path, initState, fieldName, ctrlName, optional);

            // 初始化节点的active状态
            if (initState == AutoBindAttribute.InitState.Active)
                go.SetActive(true);
            else if (initState == AutoBindAttribute.InitState.Inactive)
                go.SetActive(false);

            return field;
        }

        [AutoBind("./StatRoot")]
        public Transform m_statUIRoot;

        [AutoBind("./ShowRoot")]
        public Transform m_showRoot;

        #endregion
    }
}
