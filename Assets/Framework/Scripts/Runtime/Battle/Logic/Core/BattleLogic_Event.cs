using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{

    /// <summary>
    /// 上层事件监听器
    /// </summary>
    public interface IBattleMainEventListener
    {
        /// <summary>
        /// 战斗结束回调
        /// </summary>
        /// <param name="resultInfo"></param>
        void OnBattleEnd(string resultInfo);

        /// <summary>
        /// 推送process
        /// </summary>
        /// <param name="processList"></param>
        void OnFlushProcess(List<BattleShowProcess> processList);

        /// <summary>
        /// 通知上层 效果被handle
        /// </summary>
        /// <param name="effectNode"></param>
        void OnEffectNodeHandled(EffectNode effectNode);
    }

    public partial class BattleLogic
    {
        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="eventListener"></param>
        public void EventListenerAdd(IBattleMainEventListener eventListener)
        {
            m_battleLogicEventListenerList.Add(eventListener);
        }

        /// <summary>
        /// 移除监听  
        /// </summary>
        /// <param name="eventListener"></param>
        public void EventListenerRemove(IBattleMainEventListener eventListener)
        {
            m_battleLogicEventListenerList.Remove(eventListener);
        }

        #region 注册需要分发的事件

        protected void RegEvent4Dispatch()
        {
            CompMain.EventOnBattleStateClosed += FireEventOnBattleEnd;

            CompProcessManager.EventOnProcessFlush += FireEventOnFlushProcess;
            CompResolver.EventOnEffectNodeHandled += FireEventOnEffectNodeHandled;
        }

        #endregion


        #region 分发事件

        /// <summary>
        /// 通知上层 战斗结束
        /// </summary>
        protected void FireEventOnBattleEnd()
        {
            foreach (var listener in m_battleLogicEventListenerList)
            {
                listener.OnBattleEnd("");
            }
        }


        protected void FireEventOnBattleStarting()
        {

        }
        /// <summary>
        /// 通知上层 战斗结束
        /// </summary>
        /// <param name="processList"></param>
        protected void FireEventOnFlushProcess(List<BattleShowProcess> processList)
        {
            Debug.Log($"FireEventOnFlushProcess processCount:{processList.Count}");
            foreach (var listener in m_battleLogicEventListenerList)
            {
                listener.OnFlushProcess(processList);
            }
        }

        /// <summary>
        /// 通知上层 效果被handle
        /// </summary>
        /// <param name="effectNode"></param>
        protected void FireEventOnEffectNodeHandled(EffectNode effectNode)
        {
            Debug.Log($"FireEventOnEffectNodeHandled effectNode:{effectNode}");
            foreach (var listener in m_battleLogicEventListenerList)
            {
                listener.OnEffectNodeHandled(effectNode);
            }
        }
        


        #endregion

        /// <summary>
        /// 事件监听者集合
        /// </summary>
        protected List<IBattleMainEventListener> m_battleLogicEventListenerList = new List<IBattleMainEventListener>();
    }
}
