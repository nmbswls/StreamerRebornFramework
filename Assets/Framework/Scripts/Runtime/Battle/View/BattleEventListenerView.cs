using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Battle.View
{

    public static class BattleEventIds
    {
        public static int BattleEnd = 100;
        public static int FlushProcess = 120;
    }

    /// <summary>
    /// 表现层事件监听器
    /// </summary>
    public class BattleEventListenerView : IBattleMainEventListener
    {
        #region 初始化

        public void Init(BattleLogic battle)
        {
            m_battle = battle;
            m_battle.EventListenerAdd(this);
        }

        public void Uninit()
        {
            m_actionListMap.Clear();
            if (m_battle != null)
            {
                m_battle.EventListenerRemove(this);
            }
            m_battle = null;
        }

        private void RegisterListenerInternal(int eventId, Delegate d)
        {
            List<Delegate> list;
            if (!m_actionListMap.TryGetValue((int)eventId, out list))
            {
                list = new List<Delegate>();
                m_actionListMap.Add((int)eventId, list);
            }
            if (!list.Contains(d))
            {
                list.Add(d);
            }
        }

        private bool CheckAndGetActionList(int eventId, out List<Delegate> list)
        {
            if (!m_actionListMap.TryGetValue((int)eventId, out list))
            {
                Debug.LogError($"{eventId}消息没有注册却被调用");
                return false;
            }
            return true;
        }

        public void RegisterListener(int eventId, Action action)
        {
            RegisterListenerInternal(eventId, action);
        }

        public void RegisterListener<T1>(int eventId, Action<T1> action)
        {
            RegisterListenerInternal(eventId, action);
        }

        public void RegisterListener<T1, T2>(int eventId, Action<T1, T2> action)
        {
            RegisterListenerInternal(eventId, action);
        }

        public void RegisterListener<T1, T2, T3>(int eventId, Action<T1, T2, T3> action)
        {
            RegisterListenerInternal(eventId, action);
        }

        public void RegisterListener<T1, T2, T3, T4>(int eventId, Action<T1, T2, T3, T4> action)
        {
            RegisterListenerInternal(eventId, action);
        }

        public void Broadcast(int eventId)
        {
            List<Delegate> list;
            if (!CheckAndGetActionList(eventId, out list))
            {
                return;
            }
            foreach (var d in list)
            {
                var action = d as Action;
                if (action != null)
                {
                    action();
                }
                else
                {
                    Debug.LogErrorFormat("{0}的参数不一致", eventId);
                }
            }
        }

        public void Broadcast<T1>(int eventId, T1 arg1)
        {
            List<Delegate> list;
            if (!CheckAndGetActionList(eventId, out list))
            {
                return;
            }
            foreach (var d in list)
            {
                var action = d as Action<T1>;
                if (action != null)
                {
                    action(arg1);
                }
                else
                {
                    Debug.LogErrorFormat("{0}的参数不一致", eventId);
                }
            }
        }

        public void Broadcast<T1, T2>(int eventId, T1 arg1, T2 arg2)
        {
            List<Delegate> list;
            if (!CheckAndGetActionList(eventId, out list))
            {
                return;
            }
            foreach (var d in list)
            {
                var action = d as Action<T1, T2>;
                if (action != null)
                {
                    action(arg1, arg2);
                }
                else
                {
                    Debug.LogErrorFormat("{0}的参数不一致", eventId);
                }
            }
        }

        public void Broadcast<T1, T2, T3>(int eventId, T1 arg1, T2 arg2, T3 arg3)
        {
            List<Delegate> list;
            if (!CheckAndGetActionList(eventId, out list))
            {
                return;
            }
            foreach (var d in list)
            {
                var action = d as Action<T1, T2, T3>;
                if (action != null)
                {
                    action(arg1, arg2, arg3);
                }
                else
                {
                    Debug.LogErrorFormat("{0}的参数不一致", eventId);
                }
            }
        }

        public void Broadcast<T1, T2, T3, T4>(int eventId, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            List<Delegate> list;
            if (!CheckAndGetActionList(eventId, out list))
            {
                return;
            }
            foreach (var d in list)
            {
                var action = d as Action<T1, T2, T3, T4>;
                if (action != null)
                {
                    action(arg1, arg2, arg3, arg4);
                }
                else
                {
                }
            }
        }

        #endregion

        /// <summary>
        /// 内部战斗逻辑
        /// </summary>
        protected BattleLogic m_battle;

        /// <summary>
        /// action列表
        /// </summary>
        private Dictionary<int, List<Delegate>> m_actionListMap = new Dictionary<int, List<Delegate>>();

        #region 事件广播

        public void OnBattleEnd(string resultInfo)
        {
            Broadcast(BattleEventIds.BattleEnd, resultInfo);
        }

        /// <summary>
        /// 推送process
        /// </summary>
        /// <param name="processList"></param>
        public void OnFlushProcess(List<BattleShowProcess> processList)
        {
            Broadcast(BattleEventIds.FlushProcess, processList);
        }

        #endregion

    }


}
