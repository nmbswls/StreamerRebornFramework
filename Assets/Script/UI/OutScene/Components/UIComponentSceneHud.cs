using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    /// <summary>
    /// hud 界面
    /// </summary>
    public class UIComponentSceneHud : UIComponentBase
    {

        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            RegisterEvents();

            m_compPopCircle.BindFields();
            m_compPopCircle.gameObject.SetActive(false);

            SetButtonClickListener(nameof(m_testEnterBattleButton), OnEnterBattleButtonClick);
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

            if (m_compPopCircle != null)
                m_compPopCircle.Clear();
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
            m_compPopCircle.Tick(dt);
        }

        /// <summary>
        /// 摇人按钮
        /// </summary>
        protected void OnAddCaptiveButtonClick()
        {
            //GameStatic.GamePlayer.GainRandomCaptive(100);
        }

        #region ui回调

        /// <summary>
        /// 点击按钮
        /// </summary>
        protected void OnEnterBattleButtonClick(UIComponentBase _)
        {
            MyGameManager.Instance.LaunchBattle();
        }

        #endregion


        #region 绑定区域

        [AutoBind("./EnterBattleButton")]
        public Button m_testEnterBattleButton;

        [AutoBind("./PopCircle")]
        public UIComponentHudPopCircle m_compPopCircle;

        #endregion
    }

}
