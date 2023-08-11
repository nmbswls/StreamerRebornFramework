using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
{
    /// <summary>
    /// 逻辑事件定义
    /// </summary>
    [Serializable]
    public abstract class LogicEvent
    {
        protected LogicEvent()
        {

        }

        protected LogicEvent(int id)
        {
            m_eventId = id;
        }

        /// <summary>
        /// 消息id，在当前分类中唯一
        /// </summary>
        public int m_eventId;
    }



    /// <summary>
    /// 事件监听者
    /// </summary>
    public interface ILogicEventListener
    {
        /// <summary>
        /// 触发事件调用
        /// </summary>
        /// <param name="logicEvent"></param>
        void OnEvent(LogicEvent logicEvent);
    }

    /// <summary>
    /// 事件管线
    /// </summary>
    [Serializable]
    public class LogicEventPipe
    {
        /// <summary>
        /// 添加listener
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(ILogicEventListener listener)
        {
            m_listenerList.Add(listener);
        }

        /// <summary>
        /// 移除listener
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(ILogicEventListener listener)
        {
            m_listenerList.Remove(listener);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="logicEvent"></param>
        public void PushEvent(LogicEvent logicEvent)
        {
            foreach (var listener in m_listenerList)
            {
                listener.OnEvent(logicEvent);
            }
        }

        /// <summary>
        /// 监听器列表
        /// </summary>
        protected List<ILogicEventListener> m_listenerList = new List<ILogicEventListener>();
    }
}
