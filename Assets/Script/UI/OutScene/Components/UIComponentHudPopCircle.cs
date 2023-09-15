using System;
using System.Collections.Generic;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    public class UIComponentHudPopCircle : UIComponentBase
    {
        private Camera m_parentUICamera;

        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            InitButtons();
        }

        /// <summary>
        /// 由脚本驱动
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            //if (m_bindFaclity != null)
            //{
            //    if (m_parentUICamera == null)
            //        m_parentUICamera = gameObject.GetComponentInParent<Camera>();
            //}
        }



        /// <summary>
        /// 初始化buton
        /// </summary>
        protected void InitButtons()
        {
            //for (int i = 0; i < m_buttonGroup.childCount; i++)
            //{
            //    var go = m_buttonGroup.GetChild(i).gameObject;
            //    var compButton = go.GetOrAddComponent<UIComponentHudPopCircleButton>();
            //    compButton.BindFields();
            //    compButton.EventOnClick += OnClickButton;
            //    m_buttons.Add(compButton);

            //    go.SetActive(false);
            //}
        }

        protected void OnClickButton(UIComponentHudPopCircleButton button)
        {
            int index = m_buttons.IndexOf(button);
            if (index < 0) return;
        }

        protected List<UIComponentHudPopCircleButton> m_buttons = new List<UIComponentHudPopCircleButton>();

        #region 绑定区域

        [AutoBind("./ButtonGroup")]
        public Transform m_buttonGroup;

        #endregion
    }

    public class UIComponentHudPopCircleButton : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            m_clickArea.onClick.AddListener(OnClick);
        }

        public void UpdateView(string content)
        {
            m_contentText.text = content;
        }

        protected void OnClick()
        {
            EventOnClick?.Invoke(this);
        }

        public event Action<UIComponentHudPopCircleButton> EventOnClick;

        #region 绑定区域

        [AutoBind(".")]
        public Button m_clickArea;

        [AutoBind("./Text")]
        public Text m_contentText;

        #endregion
    }
}
