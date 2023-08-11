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
        /// �Ƿ���Ҫ�ȸ�
        /// </summary>
        protected override bool IsNeedLoadDynamicRes()
        {
            return true;
        }

        /// <summary>
        /// �ռ���Ҫ���صĶ�̬��Դ��·��
        /// </summary>
        /// <returns></returns>
        protected override List<string> CollectAllDynamicResForLoad()
        {
            // ����dialog��Ϣ ������Ҫ��Щ��Դ
            if(string.IsNullOrEmpty(m_dialogId))
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// ����intentΪ�ڲ�����
        /// </summary>
        protected override void ParseIntent()
        {
            m_dialogId = string.Empty;
            // �Ի������ ������Դ����
            m_currIntent.TryGetParam<string>("dialogId", out m_dialogId);
        }

        #region ����ӿ�

        /// <summary>
        /// ��ʾ�Ի�
        /// </summary>
        /// <returns></returns>
        public static void InitDialog(string dialogId, Action<bool> onPipelineEnd = null)
        {
            var uiIntent = new UIIntent("Dialog");
            uiIntent.SetParam("dialogId", dialogId);
            UIManager.Instance.StartUIController(uiIntent, onPipelineEnd: onPipelineEnd);
        }


        /// <summary>
        /// ��ʾ�Ի�����
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

        #region ����¼�

        /// <summary>
        /// ����Ϸ
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
        /// ����Ի���Ϣ�ֳ�
        /// </summary>
        protected string m_dialogId;


        /// <summary>
        /// ��component
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

