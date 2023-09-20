using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;

namespace My.Framework.Runtime.UI
{
    public abstract class UIControllerSceneHudBase : UIControllerBase
    {
        public UIControllerSceneHudBase(string name) : base(name)
        {
        }

        /// <summary>
        /// 包装参数
        /// </summary>
        /// <returns></returns>
        public static UIControllerSceneHudBase Create()
        {
            var intent = new UIIntent("SceneHud");
            return UIManager.Instance.StartUIController(intent) as UIControllerSceneHudBase;
        }

        /// <summary>
        /// 更新
        /// </summary>
        protected override void RefreshView()
        {
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compHud = m_uiCompArray[0] as UIComponentSceneHudBase;
        }

        protected override void OnTick(float dt)
        {
            m_compHud?.Tick(dt);

            TickDestroyMode();
        }

        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentSceneHudBase m_compHud;

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

        // 不实现 子类必须实现
        protected abstract override LayerDesc[] LayerDescArray
        {
            get;
        }

        protected abstract override UIComponentDesc[] UIComponentDescArray
        {
            get;
        }
    }

}
