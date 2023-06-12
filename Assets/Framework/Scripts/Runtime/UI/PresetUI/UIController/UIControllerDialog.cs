using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerDialog : UIControllerBase
    {
        public UIControllerDialog(string name) : base(name)
        {
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
        }

        protected override void OnTick(float dt)
        {
        }

        #region 点击事件

        /// <summary>
        /// 新游戏
        /// </summary>
        private void OnEnter()
        {
            
        }

        protected override void RefreshView()
        {
            //m_compEntryStartup.Show(m_param);
        }

        /// <summary>
        /// 
        /// </summary>
        public event Action EventOnEnter;

        #endregion


        //protected override LayerDesc[] LayerDescArray
        //{
        //    get { return m_layerDescArray; }
        //}
        //private readonly LayerDesc[] m_layerDescArray =
        //{
        //    new LayerDesc
        //    {
        //        m_layerName = "MainEntry",
        //        m_layerResPath = "Assets/Framework/Resources/UI/Preset/EntryStartup.prefab",
        //    },
        //};
        //protected override UIComponentDesc[] UIComponentDescArray
        //{
        //    get { return m_uiComponentDescArray; }
        //}
        //private readonly UIComponentDesc[] m_uiComponentDescArray =
        //{
        //    new UIComponentDesc
        //    {
        //        m_attachLayerName = "MainEntry",
        //        m_attachPath = "./",
        //        m_compTypeName = typeof(UIComponentEntryStartup).ToString(),
        //        m_compName = "MainEntry"
        //    },
        //};
    }
}

