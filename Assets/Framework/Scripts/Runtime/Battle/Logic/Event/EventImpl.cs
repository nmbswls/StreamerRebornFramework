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
    public class LogicEventDefault : LogicEvent
    {
        public LogicEventDefault()
        {

        }

        public LogicEventDefault(int id, int p1 = 0, int p2 = 0, int p3 = 0, int p4 = 0, int p5 = 0, int p6 = 0)
            : base(id)
        {
            m_p1 = p1;
            m_p2 = p2;
            m_p3 = p3;
            m_p4 = p4;
            m_p5 = p5;
            m_p6 = p6;
        }

        public int m_p1;
        public int m_p2;
        public int m_p3;
        public int m_p4;
        public int m_p5;
        public int m_p6;

        public object m_objP1;
        public object m_objP2;
        public object m_objP3;
    }


    /// <summary>
    /// 逻辑事件 战斗事件
    /// </summary>
    [Serializable]
    public class LogicEventCauseDamage : LogicEvent
    {
        public LogicEventCauseDamage(int eventId, uint targetId, uint sourceId, long damage, long realDamage)
            : base(eventId)
        {
            m_targetId = targetId;
            m_sourceId = sourceId;

            m_damage = damage;
            m_realDamage = realDamage;
        }

        public uint m_targetId;
        public uint m_sourceId;
        public long m_damage;
        public long m_realDamage;
    }
}
