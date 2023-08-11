using My.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Battle.Actor
{
    
    public partial class BattleActor
    {
        /// <summary>
        /// 添加事件listener
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(ILogicEventListener listener)
        {
            m_pipe.AddListener(listener);
        }

        /// <summary>
        /// 移除事件listener
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(ILogicEventListener listener)
        {
            m_pipe.RemoveListener(listener);
        }

        /// <summary>
        /// 将事件push到pipe中
        /// </summary>
        /// <param name="logicEvent"></param>
        public void PushEvent(LogicEvent logicEvent)
        {
            m_pipe.PushEvent(logicEvent);
        }

        /// <summary>
        /// 内部使用的pipe实现
        /// </summary>
        protected LogicEventPipe m_pipe = new LogicEventPipe();
    }
}
