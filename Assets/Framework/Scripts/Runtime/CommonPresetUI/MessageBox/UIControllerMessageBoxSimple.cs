using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerMessageBoxSimple : UIControllerBase
    {
        public UIControllerMessageBoxSimple(string name) : base(name)
        {
        }

        /// <summary>
        /// œ‘ æmessage Box
        /// </summary>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        /// <returns></returns>
        public static UIControllerMessageBoxSimple ShowMessage(string content, Action onConfirm)
        {
            var intent = new UIIntent("MessageBox");
            var messageBoxParam = new MessageBoxParam();
            messageBoxParam.actionOnBtn1Clicked = onConfirm;
            messageBoxParam.Content = content;
            intent.SetParam("Param", messageBoxParam);
            return UIManager.Instance.StartUIController(intent) as UIControllerMessageBoxSimple;
        }


        protected override void RefreshView()
        {
            if (m_param != null)
            {
                m_compMessageBox.Show(m_param);
                m_param = null;
            }
        }

        protected override void ParseIntent()
        {
            base.ParseIntent();
            m_param = m_currIntent.GetClassParam<MessageBoxParam>("Param");
        }
        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();

            m_compMessageBox = m_uiCompArray[0] as UIComponentMessageBoxSimple;
            if (m_compMessageBox != null)
            {
                m_compMessageBox.actionOnEnd += OnMsgBoxCompEnd;
            }
        }
        private void OnMsgBoxCompEnd()
        {
            m_compMessageBox.Close(Suspend);
        }


        private UIComponentMessageBoxSimple m_compMessageBox;

        private MessageBoxParam m_param;


        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "MessageBox",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/MessageBox.prefab",
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
                m_attachLayerName = "MessageBox",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentMessageBoxSimple).ToString(),
                m_compName = "MessageBox"
            },
        };
    }
}


