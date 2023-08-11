using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{
    
    public interface IBattleLogicCompRuler
    {
        /// <summary>
        /// 战斗是否结束
        /// </summary>
        /// <returns></returns>
        bool IsFinishRuleMatch();

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        void CheckBattleFinish();
    }

    /// <summary>
    /// 状态机
    /// </summary>
    public class BattleLogicCompRuler : BattleLogicCompBase, IBattleLogicCompRuler
    {
        public BattleLogicCompRuler(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName { get { return GamePlayerCompNames.Ruler; } }


        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {

        }

        public bool IsFinishRuleMatch()
        {
            return m_isFinish;
        }


        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        public void CheckBattleFinish()
        {
            //检查是否结束
            if (m_isFinish)
            {
                return;
            }

            var conf = m_owner.ConfigGet();
            foreach (var rule in conf.BattleFinishRule)
            {
                if (IsSingleRuleMatch(rule))
                {
                    m_isFinish = true;
                    break;
                }
            }
        }



        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!base.Initialize())
            {
                return false;
            }

            m_compActorContainer = m_owner.CompGet<BattleLogicCompActorContainer>(GamePlayerCompNames.ActorManager);
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            if (!base.PostInitialize())
            {
                return false;
            }


            return true;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 战斗 结束事件
        /// </summary>
        public event Action EventOnBattleFinish;

        #endregion

        #region 内部方法

        /// <summary>
        /// 检查单条是否满足
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected bool IsSingleRuleMatch(BattleFinishRule rule)
        {
            switch (rule.RuleType)
            {
                case "EnemyDie":
                {
                    var actor = m_compActorContainer.GetActor(100);
                    if (actor == null || actor.CompBasic.IsDead)
                    {
                        return true;
                    }
                    return false;
                }
                case "Turn":
                {
                    if (m_compTurnManager.TurnNumber >= 5) return true;
                    return false;
                }
                    break;
                default:
                    break;
            }

            return false;
        }

        #endregion


        #region 内部变量

        protected bool m_isFinish;
        protected string m_finishReason;


        /// <summary>
        /// actor
        /// </summary>
        protected BattleLogicCompActorContainer m_compActorContainer;

        /// <summary>
        /// 回合控制
        /// </summary>
        protected BattleLogicCompTurnManager m_compTurnManager;

        #endregion


    }
}
