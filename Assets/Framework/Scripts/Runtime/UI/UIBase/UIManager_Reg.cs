using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class UIRegisterBase
    {
        /// <summary>
        /// ִ��ע��
        /// </summary>
        public virtual void DoRegister()
        {
            UIManager.Instance.RegisterUIController("EntryStartup", typeof(UIControllerEntryStartup).ToString(), 0);
            UIManager.Instance.RegisterUIController("Loading", typeof(UIControllerLoading).ToString(), 0);
            UIManager.Instance.RegisterUIController("MessageBox", typeof(UIControllerMessageBoxSimple).ToString(), 0);
        }
    }

    public partial class UIManager
    {
        /// <summary>
        /// ��ʼ��UItask��ע����Ϣ
        /// </summary>
        public virtual bool InitUITaskRegister<T>() where T : UIRegisterBase
        {
            // ��ʼ��UItask��ע����Ϣ
            var uiRegister =  Activator.CreateInstance(typeof(T)) as UIRegisterBase;
            if (uiRegister == null)
            {
                Debug.LogError("UITaskRegister fail");
                return false;
            }
            uiRegister.DoRegister();
            return true;
        }

        #region ע��UI

        /// <summary>
        /// ע��ui
        /// </summary>
        public void RegisterUIController(string uiName, string ctrlTypeName, int uiGroup)
        {
            if (!m_uiControllerRegDict.TryGetValue(uiName, out var item))
            {
                item = new UIRegItem();
                m_uiControllerRegDict.Add(uiName, item);
            }

            item.m_ctrlTypeName = ctrlTypeName;
            item.m_uiGroup = uiGroup;
        }


        /// <summary>
        /// uiע����Ŀ
        /// </summary>
        private class UIRegItem
        {
            public string m_ctrlTypeName;
            public int m_uiGroup;

            /// <summary>
            /// ��ı�ǩ�б�
            /// </summary>
            public List<string> m_tagList = new List<string>();
        }

        /// <summary>
        /// uiע����Ϣ
        /// </summary>
        private Dictionary<string, UIRegItem> m_uiControllerRegDict = new Dictionary<string, UIRegItem>();

        #endregion
    }

}

