//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using My.Framework.Runtime.UI;
//using UnityEngine;

//namespace My.Framework.Runtime
//{
//    using WaitorTable = Dictionary<UIControllerBase, WaitForUIState>;
//    public enum WaitForUITStateType
//    {
//        Stop = 0,
//        LoadComplete,
//        Ready,

//        Max
//    }

//    public class WaitForUIState : CustomYieldInstruction
//    {
//        public string Result
//        {
//            get { return m_result; }
//            set
//            {
//                m_result = value;
//                if (CustomAction != null)
//                    CustomAction(value);
//            }
//        }

//        #region static members for all waitors;

//        /// <summary>
//        /// 新建或找到task的WaitUITask对象
//        /// </summary>
//        /// <param name="uiCtrl"></param>
//        /// <param name="type"></param>
//        /// <param name="action"></param>
//        /// <returns></returns>
//        public static WaitForUIState New(UIControllerBase uiCtrl, WaitForUITStateType type, Action<string> action = null)
//        {
//            if (uiCtrl == null || (type == WaitForUITStateType.LoadComplete && !uiCtrl.IsAnyPipelineRunning())
//                               || (type == WaitForUITStateType.Stop && uiCtrl.State == UIControllerBase.UIState.Stopped))
//            {
//                return null;
//            }

//            var table = GetTable(type);
//            WaitForUIState waitor;
//            if (!table.TryGetValue(uiCtrl, out waitor))
//            {
//                waitor = new WaitForUIState();

//                // subscribe event on UITask
//                if (type == WaitForUITStateType.Stop)
//                {
//                    uiCtrl.EventOnStop += m_onStop;
//                }
//                else if (type == WaitForUITStateType.LoadComplete)
//                {
//                    if (table.Count == 0)
//                        UIManager.Instance.EventOnUITaskNotify += m_onUITaskNotifyUpdateViewComplete;
//                }
//                else if (type == WaitForUITStateType.Ready)
//                {
//                    if (table.Count == 0)
//                        UIManager.Instance.EventOnUITaskNotify += m_onUiTaskNotifyCustom;
//                }

//                table.Add(uiCtrl, waitor);
//            }

//            if (waitor != null && action != null)
//                waitor.CustomAction += action;

//            return waitor;
//        }

//        /// <summary>
//        /// UITask 事件回调
//        /// </summary>
//        /// <param name="task"></param>
//        /// <param name="ntf"></param>
//        /// <param name="desc"></param>
//        private static void OnUiControllerNotify(UIControllerBase task, string ntf, string desc)
//        {
//            WaitForUITStateType type = ntf == "Stop"
//                ? WaitForUITStateType.Stop
//                : ntf == UIManager.UITaskNotifyType.UpdateViewComplete.ToString()
//                    ? WaitUITaskNtfType.LoadComplete
//                    : (desc == "Ready" ? WaitUITaskNtfType.Ready : WaitUITaskNtfType.Count);

//            if (type == WaitUITaskNtfType.Count)
//                return;

//            var table = GetTable(type);
//            WaitUITask waitor;
//            if (table.TryGetValue(task, out waitor) && waitor != null)
//            {
//                table.Remove(task);

//                string res = "";
//                if (type == WaitUITaskNtfType.Stop)
//                {
//                    var ires = task as IStopWithResult;
//                    if (ires != null)
//                        res = ires.Result();

//                    if (res == null)    //返回的null转为"", 这样才能让wait正常结束；
//                        res = "";
//                }
//                waitor.Result = res;
//            }

//            // desubscribe event on UITask
//            if (type == WaitUITaskNtfType.Stop)
//            {
//                task.EventOnStop -= m_onStop;
//            }
//            else if (type == WaitUITaskNtfType.LoadComplete)
//            {
//                if (table.Count == 0)
//                    UIManager.Instance.EventOnUITaskNotify -= m_onUITaskNotifyUpdateViewComplete;
//            }
//            else if (type == WaitUITaskNtfType.Ready)
//            {
//                if (table.Count == 0)
//                    UIManager.Instance.EventOnUITaskNotify -= m_onUiTaskNotifyCustom;
//            }
//        }

//        private static WaitorTable GetTable(WaitForUITStateType type)
//        {
//            if (m_tables[(int)type] == null)
//                m_tables[(int)type] = new WaitorTable();

//            return m_tables[(int)type];
//        }

//        #endregion static members for all waitors;

//        #region CustomYieldInstruction

//        public override bool keepWaiting
//        {
//            get { return m_result == null; }
//        }

//        #endregion CustomYieldInstruction


//        private string m_result;
//        public event Action<string> CustomAction;

//        private static readonly WaitorTable[] m_tables = new WaitorTable[(int)WaitForUITStateType.Max];

//        private static Action<UIControllerBase> m_onStop = (p) => OnUiControllerNotify((UIControllerBase)p, "Stop", "");
//        private static Action<UIControllerBase, string, string> m_onUiTaskNotifyCustom = (task, ntf, desc) => OnUiControllerNotify(task, ntf, desc);
//        private static Action<UIControllerBase, string, string> m_onUITaskNotifyUpdateViewComplete = (task, ntf, desc) => OnUiControllerNotify(task, ntf, desc);

//    }
//}
