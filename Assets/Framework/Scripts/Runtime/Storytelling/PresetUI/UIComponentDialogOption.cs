using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime
{

    
    /// <summary>
    /// 基础对话组件 - 对话
    /// </summary>
    public class UIComponentDialogOption : UIComponentBase
    {
        public int OptionIdx;

        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            m_clickArea.onClick.AddListener(OnClick);
        }


        /// <summary>
        /// 设置播放内容 并从头播放
        /// </summary>
        public void SetOptionContent(string optionText)
        {
            m_text.text = optionText;
        }

        #region 事件

        public void OnClick()
        {
            EventOnClick?.Invoke(OptionIdx);
        }

        public Action<int> EventOnClick;

        #endregion

        #region 绑定区域

        [AutoBind(".")]
        public Button m_clickArea;

        [AutoBind("./Text")]
        public Text m_text;

        #endregion
    }
}


