using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;

namespace StreamerReborn
{
    public class UIControllerSceneHudSimple : UIControllerSceneHudBase
    {
        public UIControllerSceneHudSimple(string name) : base(name)
        {
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
            CompHud.SetButtonClickListener("m_toolButton", OnToolButtonClick);
        }

        protected override void OnTick(float dt)
        {
            m_compHud?.Tick(dt);

            TickDestroyMode();
        }


        protected void OnToolButtonClick(UIComponentBase comp)
        {
        }


        #region 拆除逻辑

        ///// <summary>
        ///// 拆除逻辑
        ///// </summary>
        //protected void TickDestroyMode()
        //{
        //    if (!m_isInDestroyFundationMode)
        //    {
        //        return;
        //    }


        //}

        //private bool m_isInDestroyFundationMode;

        #endregion

        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentSceneHudSimple CompHud{get{return (UIComponentSceneHudSimple)m_compHud;} }

        #region 信息


        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }

        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "Hud",
                m_layerResPath = "Assets/RuntimeAssets/UI/UIPrefab/OutScene/Hud.prefab",
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
                m_attachPath = ".",
                m_compTypeName = typeof(UIComponentSceneHudSimple).ToString(),
                m_compName = "Hud"
            },
        };

        #endregion
    }

}
