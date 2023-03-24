using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class UIControllerBattleHud : UIControllerBase
    {
        public UIControllerBattleHud(string name) : base(name)
        {
        }

        /// <summary>
        /// 包装参数
        /// </summary>
        /// <returns></returns>
        public static UIControllerBattleHud Create()
        {
            var intent = new UIIntent("BattleHud");
            return UIManager.Instance.StartUIController(intent) as UIControllerBattleHud;
        }

        protected override void RefreshView()
        {

        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compHud = m_uiCompArray[0] as UIComponentBattleHud;
        }

        protected override void OnTick(float dt)
        {
            m_compHud?.Tick(dt);
        }

        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentBattleHud m_compHud;

        #region 装配信息

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "BattleHud",
                m_layerResPath = "Assets/RuntimeAssets/UI/UIPrefab/Battle/BattleHud.prefab",
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
                m_attachLayerName = "BattleHud",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBattleHud).ToString(),
                m_compName = "BattleHud"
            },
        };

        #endregion
    }
}

