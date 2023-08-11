using System;
using System.Collections;
using System.Collections.Generic;
using My.Framework.Runtime.Saving;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerBattleStartup : UIControllerBase
    {
        public UIControllerBattleStartup(string name) : base(name)
        {
        }
        public static UIControllerBattleStartup Instance
        {
            get { return m_instance; }
        }
        private static UIControllerBattleStartup m_instance;
        public static void StartUITask(Action<bool> onEnd)
        {
            UIIntent uiIntent = new UIIntent(typeof(UIControllerBattleStartup).Name);
            m_instance = UIManager.Instance.StartUIController(uiIntent, false, onEnd) as UIControllerBattleStartup;
        }


        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            if (m_uiCompArray.Length > 0 && m_compMain == null)
            {
                m_compMain = m_uiCompArray[0] as UIComponentBattleMainStartup;
            }
            if (m_uiCompArray.Length > 1 && m_compMain == null)
            {
                m_compFloating = m_uiCompArray[1] as UIComponentBattleFloatingStartup;
            }
            if (m_uiCompArray.Length > 2 && m_compMain == null)
            {
                m_compOther = m_uiCompArray[2] as UIComponentBattleOtherStartup;
            }
        }

        protected override void OnTick(float dt)
        {
        }

        protected override void RefreshView()
        {
            
        }


        protected void RegisterEvents()
        {
            //EventManager.Instance.RegisterListener(EventID.BattleMessage_MainBattleStart, EventOnBattleStart);
        }

        #region µã»÷ÊÂ¼þ


        #endregion

        /// <summary>
        /// »º´æcomponent
        /// </summary>
        private UIComponentBattleMainStartup m_compMain;
        private UIComponentBattleFloatingStartup m_compFloating;
        private UIComponentBattleOtherStartup m_compOther;


        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "BattleMain",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/BattleStartup.prefab",
            },
            new LayerDesc
            {
                m_layerName = "BattleFloating",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/BattleStartup.prefab",
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
                m_attachLayerName = "BattleMain",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBattleMainStartup).ToString(),
                m_compName = "BattleMain"
            },
            new UIComponentDesc
            {
                m_attachLayerName = "BattleFloating",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBattleFloatingStartup).ToString(),
                m_compName = "BattleFloating"
            },
            new UIComponentDesc
            {
                m_attachLayerName = "BattleMain",
                m_attachPath = "./Root/MainRoot/WithdrawRoot",
                m_compTypeName = typeof(UIComponentBattleOtherStartup).ToString(),
                m_compName = "BattleOther"
            },
        };
    }
}

