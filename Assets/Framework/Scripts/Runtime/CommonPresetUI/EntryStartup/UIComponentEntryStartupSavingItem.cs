using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.Saving;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 存档项
    /// </summary>
    public class UIComponentEntryStartupSavingItem : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            SetSelect(false, true);
            SetButtonClickListener("m_clickArea", OnScrollItemClick);

            m_selectHint.enabled = false;
        }

        public void SetSaveInfo(int saveIdx, SavingSummary saveSummary)
        {
            m_saveIndex = saveIdx;
            m_saveSummary = saveSummary;

            if (m_saveSummary == null)
            {
                m_saveName.text = $"空存档 {m_saveIndex}";
            }
            else
            {
                m_saveName.text = m_saveSummary.SavingName;
            }
        }

        public void SetSelect(bool isSelect, bool immediateComplete = false)
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

        protected void OnScrollItemClick(UIComponentBase _)
        {
            OnSaveSelect?.Invoke(m_saveIndex);
        }


        protected int m_saveIndex;
        protected SavingSummary m_saveSummary;

        public event Action<int> OnSaveSelect;

        #region 绑定区域

        [AutoBind(".")]
        public Button m_clickArea;

        [AutoBind("./SaveName")]
        public Text m_saveName;

        [AutoBind("./SaveLevel")]
        public Text m_saveLevel;

        [AutoBind("./SelectHint")]
        public Image m_selectHint;

        #endregion
    }
}
