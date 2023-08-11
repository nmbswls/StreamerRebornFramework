using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace My.Framework.Battle.Actor
{

    public interface IBattleActorHandlerBuff
    {
        /// <summary>
        /// 增加buf
        /// </summary>
        void AddBuff(int buffId, int layer = 1);
    }


    public class BattleActorHandlerBuff : BattleActorHandlerBase, IBattleActorHandlerBuff, ILogicEventListener, IBattleActorBuffEnv
    {
        public BattleActorHandlerBuff(IBattleActorOperatorEnv env) : base(env)
        {
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            m_compBuff = m_env.BattleActorCompBuffGet();
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            return true;
        }


        #endregion

        #region 对外方法

        /// <summary>
        /// 增加buf
        /// </summary>
        public void AddBuff(int buffId, int layer = 1)
        {
            BattleActorBuff targetBuff = null;
            int currLayer = 0;
            foreach (var buff in m_compBuff.BuffList)
            {
                if (buff.BuffId == buffId)
                {
                    targetBuff = buff;
                    break;
                }
            }

            if (targetBuff == null)
            {
                targetBuff = new BattleActorBuff(buffId, this);
                targetBuff.Initialize();
                m_compBuff.BuffList.Add(targetBuff);
                EventOnAddBuff?.Invoke(targetBuff);
            }
            else
            {
                // 覆盖规则
            }

            targetBuff.OnAddOrModified();
            targetBuff.OnTrigger(EnumTriggerType.OwnerLayerReach);
            m_env.PushProcessAndFlush(new BattleShowProcess_Announce(m_env.GetActorId(), 0.5f));
        }


        #endregion

        #region 事件

        /// <summary>
        /// 触发事件调用
        /// </summary>
        /// <param name="logicEvent"></param>
        void ILogicEventListener.OnEvent(LogicEvent logicEvent)
        {
            switch (logicEvent.m_eventId)
            {
                case EventIDConsts.BattleStart:
                {
                    foreach (var buff in m_compBuff.BuffList)
                    {
                        buff.OnTrigger(EnumTriggerType.BattleStart);
                    }
                    
                    break;
                }
                case EventIDConsts.CauseDamage:
                {
                    var realEvent = (LogicEventCauseDamage) logicEvent;
                    foreach (var buff in m_compBuff.BuffList)
                    {
                        buff.OnTrigger(EnumTriggerType.BattleStart, realEvent.m_sourceId, realEvent.m_targetId, realEvent.m_damage, realEvent.m_realDamage);
                    }
                    break;
                }
            }
        }


        protected Dictionary<EnumTriggerType, List<BattleActorBuff>> m_triggerType2Buffs =
            new Dictionary<EnumTriggerType, List<BattleActorBuff>>();

        #endregion

        #region IBattleActorBuffEnv

        /// <summary>
        /// 获取持有者
        /// </summary>
        /// <returns></returns>
        public IBattleActor GetOwner()
        {
            return m_env as IBattleActor;
        }

        #endregion

        #region 引用

        protected BattleActorCompBuff m_compBuff;

        /// <summary>
        /// 
        /// </summary>
        protected BattleActorHandlerAttribute m_handlerAttribute;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public event Action<BattleActorBuff> EventOnAddBuff;

        
    }
}
