using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 简单ui管理器
    /// </summary>
    public partial class UIManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize()
        {
            InitializeLayer();
            return true;
        }

        #region 对外方法

        /// <summary>
        /// 返回intent栈上的一个
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public UIControllerBase StartUIController(UIIntent intent, bool pushIntentToStack = false, Action<bool> onPipelineEnd = null)
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

            if (!StartUIControllerInternal(targetUIController, intent, pushIntentToStack, onPipelineEnd))
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
        /// 停止
        /// </summary>
        /// <param name="uiName"></param>
        public void StopUIController(string uiName)
        {
            var ctrl = FindUIControllerByName(uiName);
            if (ctrl != null)
            {
                Debug.Log(string.Format("StopUIController ctrl={0}", ctrl));
                ctrl.Stop();
            }
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

        /// <summary>
        /// 隐藏组中的所有ui
        /// </summary>
        public void HideAllByGroup(int groupId)
        {
            var uiList = new List<UIControllerBase>();
            foreach (var kv in m_uiControllerDict)
            {
                uiList.Add(kv.Value);
            }

            foreach (var ctrl in uiList)
            {
                if(!m_uiControllerRegDict.TryGetValue(ctrl.Name, out var regItem))
                {
                    continue;
                }
                if(regItem.m_uiGroup == groupId)
                {
                    ctrl.Stop();
                }
            }
        }

        #endregion

        #region 单例相关

        public static UIManager CreateUIManager()
        {
            if (m_instance == null)
            {
                m_instance = new UIManager();
            }
            return m_instance;
        }
        private UIManager() { }
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

            m_corutineHelper.Tick();
            TickLayerStack();
        }

        #region 内部方法

        /// <summary>
        /// 获取或创建已经存在的
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
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
            // 注册到ui字典
            m_uiControllerDict[uiName] = retController;

            return retController;
        }

        /// <summary>
        /// 内部启动一个ui controller
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="intent"></param>
        /// <param name="pushIntentToStack"></param>
        /// <returns></returns>
        private bool StartUIControllerInternal(UIControllerBase ctrl, UIIntent intent, bool pushIntentToStack, Action<bool> onPipelineEnd = null)
        {
            // 首先关闭所有存在冲突的ui
            CloseAllConflictUI(intent.TargetUIName);

            // 如果intent需要压栈
            if (pushIntentToStack)
            {
                m_uiIntentStack.Add(intent);
            }

            var ret = StartOrResumeControllerUI(ctrl, intent, onPipelineEnd);
            if (!ret)
            {
                Debug.LogError(string.Format("StartUITask fail task={0}", ctrl.Name));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 关闭所有冲突的ui组
        /// </summary>
        /// <param name="uiName"></param>
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

        #endregion

        private bool StartOrResumeControllerUI(UIControllerBase uiCtrl, UIIntent intent, Action<bool>  onPipelineEnd)
        {
            switch (uiCtrl.State)
            {
                case UIControllerBase.UIState.Init:
                    uiCtrl.EventOnStop += OnUIStop;
                    return uiCtrl.Start(intent, onPipelineEnd);
                case UIControllerBase.UIState.Suspend:
                    return uiCtrl.Resume(intent, onPipelineEnd);
                case UIControllerBase.UIState.Running:
                    return uiCtrl.OnNewIntent(intent, onPipelineEnd);
                case UIControllerBase.UIState.Stopped:
                    Debug.LogError($"StartOrResumeUI fail. Not Valid state.");
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
            if (m_uiControllerDict.ContainsKey(ctrl.Name))
            {
                m_uiControllerDict.Remove(ctrl.Name);
            }
        }


        #region 内部变量

        /// <summary>
        /// corutine管理
        /// </summary>
        private SimpleCoroutineWrapper m_corutineHelper = new SimpleCoroutineWrapper();

        /// <summary>
        /// 冲突信息
        /// </summary>
        private List<List<int>> m_uiGroupConflictList = new List<List<int>>();

        /// <summary>
        /// 需要停止的ui的列表
        /// </summary>
        private List<UIControllerBase> m_uiList4Stop = new List<UIControllerBase>();

        /// <summary>
        /// uiController 字典
        /// </summary>
        private Dictionary<string, UIControllerBase> m_uiControllerDict = new Dictionary<string, UIControllerBase>();
        private List<UIControllerBase> m_ctrlList4TickLoop = new List<UIControllerBase>();

        /// <summary>
        /// Intent栈
        /// </summary>
        private List<UIIntent> m_uiIntentStack = new List<UIIntent>();

        #endregion
    }
}