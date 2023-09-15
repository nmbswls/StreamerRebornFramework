using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;

namespace StreamerReborn
{
    public class UIControllerSceneHud : UIControllerBase
    {
        public UIControllerSceneHud(string name) : base(name)
        {
        }

        /// <summary>
        /// 包装参数
        /// </summary>
        /// <returns></returns>
        public static UIControllerSceneHud Create()
        {
            var intent = new UIIntent("SceneHud");
            return UIManager.Instance.StartUIController(intent) as UIControllerSceneHud;
        }

        /// <summary>
        /// 更新
        /// </summary>
        protected override void RefreshView()
        {
            //// 初始化已在场的人的血条
            //foreach (var actor in GameModeMain.Instance.ActorContainer.Values)
            //{
            //    if (actor.m_logicActor.ActorTypeGet() == SceneActorType.Character)
            //    {
            //        m_compHud.ShowCharStat((TempActorControllerCharacter)actor);
            //    }
            //}
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compHud = m_uiCompArray[0] as UIComponentSceneHud;
            m_compHud.SetButtonClickListener("m_toolButton", OnToolButtonClick);
        }

        protected override void OnTick(float dt)
        {
            m_compHud?.Tick(dt);

            TickDestroyMode();
        }

        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentSceneHud m_compHud;

        protected void OnToolButtonClick(UIComponentBase comp)
        {
        }


        #region 拆除逻辑

        /// <summary>
        /// 拆除逻辑
        /// </summary>
        protected void TickDestroyMode()
        {
            if (!m_isInDestroyFundationMode)
            {
                return;
            }


        }

        private bool m_isInDestroyFundationMode;

        #endregion

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "Hud",
                m_layerResPath = "Assets/RuntimeAssets/Main/UI/Hud.prefab",
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
                m_attachLayerName = "Hud",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentSceneHud).ToString(),
                m_compName = "Hud"
            },
        };
    }

}
