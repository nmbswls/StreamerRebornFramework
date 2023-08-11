using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime
{
    public class UIControllerDialogSimple : UIControllerBase
    {
        public UIControllerDialogSimple(string name) : base(name)
        {
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();

            m_compDialogSimple = m_uiCompArray[0] as UIComponentDialogSimple;
            m_compDialogSimple.HideAll();
        }

        protected override void OnTick(float dt)
        {
            m_compDialogSimple?.Tick(dt);
        }

        /// <summary>
        /// 是否需要热更
        /// </summary>
        protected override bool IsNeedLoadDynamicRes()
        {
            return true;
        }

        /// <summary>
        /// 收集需要加载的动态资源的路径
        /// </summary>
        /// <returns></returns>
        protected override List<string> CollectAllDynamicResForLoad()
        {
            // 根据dialog信息 计算需要哪些资源
            if(string.IsNullOrEmpty(m_dialogId))
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// 解析intent为内部变量
        /// </summary>
        protected override void ParseIntent()
        {
            m_dialogId = string.Empty;
            // 对话组概念 控制资源加载
            m_currIntent.TryGetParam<string>("dialogId", out m_dialogId);
        }

        #region 对外接口

        /// <summary>
        /// 显示对话
        /// </summary>
        /// <returns></returns>
        public static void InitDialog(string dialogId, Action<bool> onPipelineEnd = null)
        {
            var uiIntent = new UIIntent("Dialog");
            uiIntent.SetParam("dialogId", dialogId);
            UIManager.Instance.StartUIController(uiIntent, onPipelineEnd: onPipelineEnd);
        }


        /// <summary>
        /// 显示对话内容
        /// </summary>
        public static void Say(string content, string speaker, Action callback)
        {
            var dialog = UIManager.Instance.FindUIControllerByName("Dialog") as UIControllerDialogSimple;
            if (dialog != null)
            {
                dialog.m_compDialogSimple.SetDialogContent(content);
                dialog.m_compDialogSimple.SetDialogSpeaker(speaker);

                if(callback != null)
                {
                    dialog.m_compDialogSimple.SetDialogCallback(callback);
                }
                dialog.m_compDialogSimple.StartPlayText();
            }
        }

        #endregion

        #region 点击事件

        /// <summary>
        /// 新游戏
        /// </summary>
        private void OnEnter()
        {
            
        }

        protected override void RefreshView()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public event Action EventOnEnter;

        #endregion

        /// <summary>
        /// 保存对话信息现场
        /// </summary>
        protected string m_dialogId;


        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentDialogSimple m_compDialogSimple;


        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "Dialog",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/Dialog.prefab",
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
                m_attachLayerName = "Dialog",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentDialogSimple).ToString(),
                m_compName = "Dialog"
            },
        };
    }
}

