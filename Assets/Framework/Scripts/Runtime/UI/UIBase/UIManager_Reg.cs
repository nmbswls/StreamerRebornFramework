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
        /// 执行注册
        /// </summary>
        public virtual void DoRegister()
        {
            UIManager.Instance.RegisterUIController("EntryStartup", typeof(UIControllerEntryStartup).ToString(), 0);
            UIManager.Instance.RegisterUIController("Loading", typeof(UIControllerLoading).ToString(), 99);
            UIManager.Instance.RegisterUIController("MessageBox", typeof(UIControllerMessageBoxSimple).ToString(), 99);
            UIManager.Instance.RegisterUIController("ScreenEffect", typeof(UIControllerScreenEffectSimple).ToString(), 99);

            UIManager.Instance.RegisterUIController("Dialog", typeof(UIControllerDialogSimple).ToString(), 2);
            UIManager.Instance.RegisterUIController("SceneHud", typeof(UIControllerSceneHudBase).ToString(), 2);


            UIManager.Instance.RegisterUIController("BattleStartup", typeof(UIControllerBattleStartup).ToString(), 10);
            UIManager.Instance.RegisterUIController("BattlePerform", typeof(UIControllerBattlePerform).ToString(), 10);


        }
    }

    public partial class UIManager
    {
        /// <summary>
        /// 初始化UItask的注册信息
        /// </summary>
        public virtual bool InitUITaskRegister<T>() where T : UIRegisterBase
        {
            // 初始化UItask的注册信息
            var uiRegister =  Activator.CreateInstance(typeof(T)) as UIRegisterBase;
            if (uiRegister == null)
            {
                Debug.LogError("UITaskRegister fail");
                return false;
            }
            uiRegister.DoRegister();
            return true;
        }

        #region 注册UI

        /// <summary>
        /// 注册ui
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
        /// ui注册条目
        /// </summary>
        private class UIRegItem
        {
            public string m_ctrlTypeName;
            public int m_uiGroup;

            /// <summary>
            /// 类的标签列表
            /// </summary>
            public List<string> m_tagList = new List<string>();
        }

        /// <summary>
        /// ui注册信息
        /// </summary>
        private Dictionary<string, UIRegItem> m_uiControllerRegDict = new Dictionary<string, UIRegItem>();

        #endregion
    }

}

