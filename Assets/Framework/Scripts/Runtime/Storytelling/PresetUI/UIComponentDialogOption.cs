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
    /// �����Ի���� - �Ի�
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
        /// ���ò������� ����ͷ����
        /// </summary>
        public void SetOptionContent(string optionText)
        {
            m_text.text = optionText;
        }

        #region �¼�

        public void OnClick()
        {
            EventOnClick?.Invoke(OptionIdx);
        }

        public Action<int> EventOnClick;

        #endregion

        #region ������

        [AutoBind(".")]
        public Button m_clickArea;

        [AutoBind("./Text")]
        public Text m_text;

        #endregion
    }
}


