using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// ��ui������
    /// </summary>
    public partial class UIManager
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize()
        {
            InitializeLayer();
            return true;
        }

        #region ���ⷽ��

        /// <summary>
        /// ����intentջ�ϵ�һ��
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
            // �õ�uiʵ��
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
        /// ����intentջ�ϵ�һ��task������Ϊnull
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="onPipelineEnd"></param>
        /// <returns></returns>
        public UIControllerBase ReturnUIController(UIIntent intent)
        {
            Debug.Log(string.Format("ReturnUITask task={0}", intent.TargetUIName));

            // �õ�uitask��ʵ��
            var targetTask = GetOrCreateUIController(intent.TargetUIName);

            // ��intentջ����pop��ָ��intentΪֹ
            if (!PopIntentUntilReturnTarget(intent))
            {
                Debug.LogError(string.Format("ReturnUITask fail intent not in stack task={0}", intent.TargetUIName));
                return null;
            }

            // ����uitask
            if (!StartUIControllerInternal(targetTask, intent, false))
            {
                targetTask = null;
            }

            return targetTask;
        }

        /// <summary>
        /// ֹͣ
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
        /// ����ui���ƻ�ȡcontroller
        /// </summary>
        /// <returns></returns>
        public UIControllerBase FindUIControllerByName(string uiName)
        {
            // �鿴task�Ƿ��Ѿ�����
            UIControllerBase targetTask;
            if (!m_uiControllerDict.TryGetValue(uiName, out targetTask))
            {
                return null;
            }

            return targetTask;
        }

        /// <summary>
        /// �������е�����ui
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

        #region �������

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
        /// ����������
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

        #region �ڲ�����

        /// <summary>
        /// ��ȡ�򴴽��Ѿ����ڵ�
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
        private UIControllerBase GetOrCreateUIController(string uiName)
        {
            UIControllerBase retController;
            // �鿴task�Ƿ��Ѿ�����
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
            // ��������ڴ����µ�task
            retController = Activator.CreateInstance(type, uiName) as UIControllerBase;

            // ���б�Ҫ�ĳ�ʼ��
            retController.InitlizeBeforeManagerStartIt();
            // ע�ᵽui�ֵ�
            m_uiControllerDict[uiName] = retController;

            return retController;
        }

        /// <summary>
        /// �ڲ�����һ��ui controller
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="intent"></param>
        /// <param name="pushIntentToStack"></param>
        /// <returns></returns>
        private bool StartUIControllerInternal(UIControllerBase ctrl, UIIntent intent, bool pushIntentToStack, Action<bool> onPipelineEnd = null)
        {
            // ���ȹر����д��ڳ�ͻ��ui
            CloseAllConflictUI(intent.TargetUIName);

            // ���intent��Ҫѹջ
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
        /// �ر����г�ͻ��ui��
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

            // �ռ���ͻ��task
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

            // ֹͣ������Ҫֹͣ��
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
        /// ��intentջ����pop��ָ��intentΪֹ
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
        /// uitaskֹͣ�Ļص�
        /// </summary>
        /// <param name="task"></param>
        private void OnUIStop(UIControllerBase ctrl)
        {
            if (m_uiControllerDict.ContainsKey(ctrl.Name))
            {
                m_uiControllerDict.Remove(ctrl.Name);
            }
        }


        #region �ڲ�����

        /// <summary>
        /// corutine����
        /// </summary>
        private SimpleCoroutineWrapper m_corutineHelper = new SimpleCoroutineWrapper();

        /// <summary>
        /// ��ͻ��Ϣ
        /// </summary>
        private List<List<int>> m_uiGroupConflictList = new List<List<int>>();

        /// <summary>
        /// ��Ҫֹͣ��ui���б�
        /// </summary>
        private List<UIControllerBase> m_uiList4Stop = new List<UIControllerBase>();

        /// <summary>
        /// uiController �ֵ�
        /// </summary>
        private Dictionary<string, UIControllerBase> m_uiControllerDict = new Dictionary<string, UIControllerBase>();
        private List<UIControllerBase> m_ctrlList4TickLoop = new List<UIControllerBase>();

        /// <summary>
        /// Intentջ
        /// </summary>
        private List<UIIntent> m_uiIntentStack = new List<UIIntent>();

        #endregion
    }
}