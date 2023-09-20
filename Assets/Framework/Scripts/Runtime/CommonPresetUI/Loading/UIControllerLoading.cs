using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerLoading : UIControllerBase
    {
        public UIControllerLoading(string name) : base(name)
        {
        }

        /// <summary>
        /// 使用指定参数创建加载
        /// </summary>
        public static UIControllerLoading ShowLoadingUI(int type, string content, Action onShowAnimationEnd = null)
        {
            var uiIntent = new UIIntent("Loading");
            uiIntent.SetParam("type", type);
            uiIntent.SetParam("content", content);
            uiIntent.SetParam(OnShowAnimationEndParam, onShowAnimationEnd);
            return UIManager.Instance.StartUIController(uiIntent) as UIControllerLoading;
        }

        /// <summary>
        /// 关闭 当前单例
        /// </summary>
        /// <param name="onCloseAnimationEnd"></param>
        public static void StopLoadingUI(Action onCloseAnimationEnd = null)
        {
            var ctrl = UIManager.Instance.FindUIControllerByName("Loading") as UIControllerLoading;
            if (ctrl != null)
            {
                ctrl.RegisterOnCloseAnimationEndAction(onCloseAnimationEnd);
                ctrl.TryClose();
            }
            else
            {
                //如果当前没有开启，则直接触发回调
                onCloseAnimationEnd?.Invoke();
            }
        }

        protected override void ParseIntent()
        {
            m_currIntent.TryGetParam<int>("type", out m_loadingType);
            m_currIntent.TryGetParam<string>("content", out m_loadingContent);
            RegisterOnShowAnimationEndAction(m_currIntent.GetClassParam<Action>(OnShowAnimationEndParam));
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compLoading = m_uiCompArray[0] as UIComponentLoading;

            m_loadingTimer = 0;
        }

        protected override void OnTick(float dt)
        {
            m_loadingTimer += dt;
            if (m_isCloseSignaled && m_loadingTimer > MinLoadingTime)
            {
                Close();
            }
        }

        private void TryClose()
        {
            Debug.Log($"Loading Try Close. Curr Loading Source {m_loadingSource}");
            if (!m_showAnimationOpenEnd || m_loadingTimer < MinLoadingTime)
            {
                m_isCloseSignaled = true;
                return;
            }

            Close();
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        protected override void RefreshView()
        {
            m_isCloseSignaled = false;
            m_showAnimationOpenEnd = false;
            if (!m_isOpeningUI)
            {
                OnShowAnimationEnd(true);
                return;
            }
            m_compLoading.ShowLoadingUI(m_loadingContent, OnShowAnimationEnd);
        }

        private void Close()
        {
            m_compLoading.HideLoadingUI(OnCloseAnimationEnd);
        }

        private void RegisterOnShowAnimationEndAction(Action actionOnEnd)
        {
            if (actionOnEnd != null && !m_actionOnShowEndList.Contains(actionOnEnd))
                m_actionOnShowEndList.Add(actionOnEnd);
        }

        private void RegisterOnCloseAnimationEndAction(Action actionOnEnd)
        {
            if (actionOnEnd != null && !m_actionOnCloseEndList.Contains(actionOnEnd))
                m_actionOnCloseEndList.Add(actionOnEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        private void OnShowAnimationEnd(bool completed)
        {
            m_showAnimationOpenEnd = true;

            List<Action> actionList = new List<Action>(m_actionOnShowEndList);
            m_actionOnShowEndList.Clear();
            foreach (Action action in actionList)
            {
                action();
            }

            if (m_isCloseSignaled)
            {
                m_isCloseSignaled = false;
                //如果打开途中收到关闭请求
                Close();
            }
        }

        private void OnCloseAnimationEnd()
        {
            List<Action> actionList = new List<Action>(m_actionOnCloseEndList);
            m_actionOnCloseEndList.Clear();
            foreach (Action action in actionList)
            {
                action();
            }

            // delay stop
            Stop();
        }

        protected override void InitLayerStateOnLoadAllResCompleted()
        {
            base.InitLayerStateOnLoadAllResCompleted();
            MainLayer.Priority = 200;
        }

        /// <summary>
        /// 最小加载时间
        /// </summary>
        public const float MinLoadingTime = 0.8f;

        /// <summary>
        /// 是否需要关闭
        /// </summary>
        private bool m_isCloseSignaled;

        private float m_loadingTimer = 0;
        /// <summary> Show动画播放结束回调 </summary>
        private readonly List<Action> m_actionOnShowEndList = new List<Action>();
        /// <summary> Close动画播放结束回调 </summary>
        private readonly List<Action> m_actionOnCloseEndList = new List<Action>();

        #region 点击事件

        #endregion

        protected int m_loadingType;
        public string m_loadingSource = string.Empty;

        /// <summary>
        /// loading字串
        /// </summary>
        public string m_loadingContent;

        public bool IsOpen { get { return m_showAnimationOpenEnd; } }
        private bool m_showAnimationOpenEnd;
        /// <summary>
        /// 
        /// </summary>
        protected UIComponentLoading m_compLoading;

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "Loading",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/Loading.prefab",
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
                m_attachLayerName = "Loading",
                m_attachPath = ".",
                m_compTypeName = typeof(UIComponentLoading).ToString(),
                m_compName = "Loading"
            },
        };

        private const String LoadingTypeParam = "LoadingTypeParam";
        private const String OnShowAnimationEndParam = "OnShowAnimationEndParam";
    }
}

