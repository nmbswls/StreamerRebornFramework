using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// hud 界面
    /// </summary>
    public class UIComponentSceneHudBase : UIComponentBase
    {

        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            RegisterEvents();
        }


        /// <summary>
        /// </summary>
        /// <param name="uiName"></param>
        public override void Initlize(string uiName)
        {
            base.Initlize(uiName);
        }

        public override void Clear()
        {
            base.Clear();
            UnregisterEvents();
        }

        /// <summary>
        /// 注册
        /// </summary>
        private void RegisterEvents()
        {
        }

        /// <summary>
        /// 销毁
        /// </summary>
        private void UnregisterEvents()
        {
        }


        public override void Tick(float dt)
        {
        }

        /// <summary>
        /// 摇人按钮
        /// </summary>
        protected void OnAddCaptiveButtonClick()
        {
        }

        #region ui回调


        #endregion

        #region 绑定区域


        #endregion
    }

}
