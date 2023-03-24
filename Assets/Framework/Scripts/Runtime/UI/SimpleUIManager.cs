using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{

    public partial class UIManager
    {
        private UIManager() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            return true;
        }
        /// <summary>
        /// 返回intent栈上的一个
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public UIControllerBase StartUIController(UIIntent intent, bool pushIntentToStack = false)
        {
            if (!m_uiControllerRegDict.ContainsKey(intent.TargetUIName))
            {
                Debug.LogError($"Not found ui {intent.TargetUIName}");
                return null;
            }
            // 得到ui实例
            var targetUIController = GetOrCreateUIController(intent.TargetUIName);
            if (targetUIController == null)
            {
                Debug.LogError($"StartUITask fail for GetOrCreateUITask null TargetTaskName={intent.TargetUIName}");
                return null;
            }

            if (!StartUIControllerInternal(targetUIController, intent, pushIntentToStack))
            {
                Debug.LogError($"StartUITask fail for GetOrCreateUITask null TargetTaskName={intent.TargetUIName}");
                return null;
            }
            return targetUIController;
        }

        /// <summary>
        /// 返回intent栈上的一个task，可能为null
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="onPipelineEnd"></param>
        /// <returns></returns>
        public UIControllerBase ReturnUIController(UIIntent intent)
        {
            Debug.Log(string.Format("ReturnUITask task={0}", intent.TargetUIName));

            // 得到uitask的实例
            var targetTask = GetOrCreateUIController(intent.TargetUIName);

            // 将intent栈数据pop到指定intent为止
            if (!PopIntentUntilReturnTarget(intent))
            {
                Debug.LogError(string.Format("ReturnUITask fail intent not in stack task={0}", intent.TargetUIName));
                return null;
            }

            // 启动uitask
            if (!StartUIControllerInternal(targetTask, intent, false))
            {
                targetTask = null;
            }

            return targetTask;
        }


        /// <summary>
        /// 根据ui名称获取controller
        /// </summary>
        /// <returns></returns>
        public UIControllerBase FindUIControllerByName(string uiName)
        {
            // 查看task是否已经存在
            UIControllerBase targetTask;
            if (!m_uiControllerDict.TryGetValue(uiName, out targetTask))
            {
                return null;
            }

            return targetTask;
        }

        #region 代码

        public static UIManager CreateUIManager()
        {
            if (m_instance == null)
            {
                m_instance = new UIManager();
            }
            return m_instance;
        }
        /// <summary>
        /// 单例访问器
        /// </summary>
        public static UIManager Instance { get { return m_instance; } }
        private static UIManager m_instance;


        #endregion

        public void Tick(float dt)
        {
            foreach (var kv in m_uiControllerDict)
            {
                m_ctrlList4TickLoop.Add(kv.Value);
            }
            foreach (var ctrl in m_ctrlList4TickLoop)
            {
                {
                    ctrl.Tick(dt);
                }
            }
            m_ctrlList4TickLoop.Clear();
        }

        private bool StartUIControllerInternal(UIControllerBase ctrl, UIIntent intent, bool pushIntentToStack)
        {
            // 首先关闭所有存在冲突的ui
            CloseAllConflictUI(intent.TargetUIName);

            // 如果intent需要压栈
            if (pushIntentToStack)
            {
                m_uiIntentStack.Add(intent);
            }

            var ret = StartOrResumeControllerUI(ctrl, intent);
            if (!ret)
            {
                Debug.LogError(string.Format("StartUITask fail task={0}", ctrl.Name));
                return false;
            }

            return true;
        }

        private void CloseAllConflictUI(string uiName)
        {
            if (!m_uiControllerRegDict.TryGetValue(uiName, out var srcTaskItem))
            {
                return;
            }

            var srcTaskGroup = srcTaskItem.m_uiGroup;
            List<int> conlictGroupList = null;
            if (m_uiGroupConflictList.Count > srcTaskGroup)
            {
                conlictGroupList = m_uiGroupConflictList[srcTaskGroup];
            }

            if (conlictGroupList == null || conlictGroupList.Count == 0)
            {
                return;
            }

            // 收集冲突的task
            UIRegItem taskItem;
            foreach (var item in m_uiControllerDict)
            {
                if (!m_uiControllerRegDict.TryGetValue(item.Value.Name, out taskItem))
                {
                    continue;
                }

                var destTaskGroup = taskItem.m_uiGroup;
                if (conlictGroupList.Contains(destTaskGroup))
                {
                    m_uiList4Stop.Add(item.Value);
                }
            }

            // 停止所有需要停止的
            if (m_uiList4Stop.Count != 0)
            {
                foreach (var destUI in m_uiList4Stop)
                {
                    destUI.Stop();
                }
                m_uiList4Stop.Clear();
            }
        }

        private UIControllerBase GetOrCreateUIController(string uiName)
        {
            UIControllerBase retController;
            // 查看task是否已经存在
            if (m_uiControllerDict.TryGetValue(uiName, out retController))
                return retController;

            if (!m_uiControllerRegDict.TryGetValue(uiName, out var item))
            {
                Debug.LogError($"GetOrCreateUIController fail {uiName} not registed");
                return null;
            }

            Type type = Type.GetType(item.m_ctrlTypeName);
            if (type == null)
            {
                Debug.LogError($"GetOrCreateUIController fail {uiName} not registed");
                return null;
            }
            // 如果不存在创建新的task
            retController = Activator.CreateInstance(type, uiName) as UIControllerBase;

            // 进行必要的初始化
            retController.InitlizeBeforeManagerStartIt();
            // 注册到task字典
            m_uiControllerDict[uiName] = retController;
            return retController;
        }

        private bool StartOrResumeControllerUI(UIControllerBase uiCtrl, UIIntent intent)
        {
            switch (uiCtrl.State)
            {
                case UIControllerBase.UIState.Init:
                    return uiCtrl.Start(intent);
                case UIControllerBase.UIState.Suspend:
                    return uiCtrl.Start(intent);
                case UIControllerBase.UIState.Running:
                    return uiCtrl.OnNewIntent(intent);
                case UIControllerBase.UIState.Stopped:
                    Debug.LogError($"StartOrResumeUI fail in TaskState.Stopped task={uiCtrl}");
                    return false;
                default:
                    return false;
            }
        }



        /// <summary>
        /// 将intent栈数据pop到指定intent为止
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        private bool PopIntentUntilReturnTarget(UIIntent intent)
        {
            if (!m_uiIntentStack.Contains(intent))
            {
                return false;
            }
            for (var i = m_uiIntentStack.Count - 1; i >= 0; i--)
            {
                var lintent = m_uiIntentStack[i];
                if (lintent != intent)
                {
                    m_uiIntentStack.Remove(lintent);
                }
                else
                {
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// uitask停止的回调
        /// </summary>
        /// <param name="task"></param>
        private void OnUIStop(UIControllerBase ctrl)
        {
            Debug.Log("UIManager::OnUIStop ");

            if (m_uiControllerDict.ContainsKey(ctrl.Name))
            {
                m_uiControllerDict.Remove(ctrl.Name);
            }
        }

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
        /// 需要停止的ui的列表
        /// </summary>
        private List<UIControllerBase> m_uiList4Stop = new List<UIControllerBase>();

        /// <summary>
        /// 冲突信息
        /// </summary>
        private List<List<int>> m_uiGroupConflictList = new List<List<int>>();
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

        /// <summary>
        /// uiController 字典
        /// </summary>
        private Dictionary<string, UIControllerBase> m_uiControllerDict = new Dictionary<string, UIControllerBase>();
        private List<UIControllerBase> m_ctrlList4TickLoop = new List<UIControllerBase>();

        /// <summary>
        /// Intent栈
        /// </summary>
        private List<UIIntent> m_uiIntentStack = new List<UIIntent>();
    }
}