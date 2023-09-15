using System;
using System.Collections.Generic;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    /// <summary>
    /// 可选择对象 队列中
    /// </summary>
    public class UIComponentSelectableInList : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            SetSelect(false);
            SetButtonClickListener("m_clickArea", OnClick);
        }

        public void SetInfo(int index)
        {
            m_index = index;
        }

        public virtual void SetSelect(bool isSelect)
        {
            if (isSelect)
            {
                m_selectHint.enabled = true;
            }
            else
            {
                m_selectHint.enabled = false;
            }
        }

        protected void OnClick(UIComponentBase _)
        {
            OnSelect?.Invoke(m_index);
        }

        protected int m_index;

        public event Action<int> OnSelect;

        #region 绑定区域

        [AutoBind(".")]
        public Button m_clickArea;

        [AutoBind("./SelectHint")]
        public Image m_selectHint;

        #endregion
    }
}
