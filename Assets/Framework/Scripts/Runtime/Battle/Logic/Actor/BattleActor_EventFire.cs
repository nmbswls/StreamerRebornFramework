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

    /// <summary>
    /// 上层事件监听器
    /// </summary>
    public interface IBattleActorEventListener
    {
        /// <summary>
        /// 添加buff 事件
        /// </summary>
        void OnAddBuff(BattleActor actor, BattleActorBuff buff);

        /// <summary>
        /// actor 造成伤害
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="sourceId"></param>
        /// <param name="dmg"></param>
        /// <param name="realDmg"></param>
        void OnCauseDamage(uint targetId, uint sourceId, long dmg, long realDmg);
    }


    public partial class BattleActor
    {
        protected void RegEvent4Fire()
        {
            m_handlerBuff.EventOnAddBuff += FireEventOnAddBuff;
            m_handlerHpState.EventOnCauseDamage += FireEventOnCauseDamage;
        }

        #region 事件抛出函数


        protected void FireEventOnAddBuff(BattleActorBuff buff)
        {
            foreach (var listener in m_battleActorEventListenerList)
            {
                listener.OnAddBuff(this, buff);
            }
        }

        /// <summary>
        /// 抛出伤害事件
        /// </summary>
        protected void FireEventOnCauseDamage(uint targetId, uint sourceId, long dmg, long realDmg)
        {
            foreach (var listener in m_battleActorEventListenerList)
            {
                listener.OnCauseDamage(targetId, sourceId, dmg, realDmg);
            }
        }

        #endregion

        /// <summary>
        /// 事件监听者集合
        /// </summary>
        protected List<IBattleActorEventListener> m_battleActorEventListenerList = new List<IBattleActorEventListener>();
    }
}
