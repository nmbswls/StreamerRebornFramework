using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class BattleAudiencePerk
    {
        BattleAudience Owner;

        public int ConfInfo;

        public BattleAudiencePerk(BattleAudience owner)
        {
            this.Owner = owner;
        }

        public virtual bool Trigger()
        {
            if (!CanBeTrigger())
            {
                return false;
            }

            // 进入连锁
            //PushContextState(this);

            EventOnPreTrigger?.Invoke(this);

            // 增加生效次数
            IncreaseTirggerTimes();

            // 开始冷却
            StartCoolDown();

            // 执行效果
            DoTriggerAction();


            EventOnPostTrigger?.Invoke(this);

            // 退出连锁
            //PushContextState(this);

            return true;
        }

        public virtual bool CanBeTrigger()
        {
            return false;
        }

        /// <summary>
        /// buff触发 
        /// </summary>
        protected void OnTrigger()
        {
        }

        /// <summary>
        /// CD
        /// </summary>
        public void StartCoolDown()
        {
            
        }

        /// <summary>
        /// 增加触发次数
        /// </summary>
        public void IncreaseTirggerTimes()
        {
            m_triggeredTimes++;
            m_triggeredTimesOfCurTurn++;
        }


        /// <summary>
        /// 执行触发行为
        /// </summary>
        protected virtual void DoTriggerAction()
        {
            Owner.OnPerkTrigger(ConfInfo);
        }

        /// <summary>
        /// 配置数据 类型 参数
        /// </summary>
        protected int m_confInfo;

        /// <summary>
        /// 触发计数器
        /// </summary>
        protected int m_triggeredTimes;
        protected int m_triggeredTimesOfCurTurn;

        #region 事件

        public event Action<BattleAudiencePerk> EventOnPreTrigger;

        public event Action<BattleAudiencePerk> EventOnPostTrigger;

        #endregion

        public bool m_isPassiveSkill;
        public int m_sourceSkillId;
    }
}

