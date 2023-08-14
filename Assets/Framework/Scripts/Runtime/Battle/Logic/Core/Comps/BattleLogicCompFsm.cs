using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{
    
    public interface IBattleMainCompFSM
    {
        /// <summary>
        /// 启动BattleGroup流程
        /// </summary>
        void BattleStart();
    }

    /// <summary>
    /// 状态机
    /// </summary>
    public class BattleLogicCompFsm : BattleLogicCompBase, IBattleMainCompFSM
    {
        public BattleLogicCompFsm(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName { get { return GamePlayerCompNames.FSM; } }

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

            m_compProcessManager = m_owner.CompGet<BattleLogicCompProcessManager>(GamePlayerCompNames.ProcessManager);
            m_compRuler = m_owner.CompGet<BattleLogicCompRuler>(GamePlayerCompNames.Ruler);
            m_compResolver = m_owner.CompGet<BattleLogicCompResolver>(GamePlayerCompNames.Resolver);
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

            

            StateGoto(BattleMainState.Init);

            return true;
        }


        public override void Tick(float dt)
        {
            // 检查抛出事件
            //CheckBattleEnd();

            TickState(dt);
        }

        /// <summary>
        /// Tick推进逻辑进行
        /// </summary>
        protected virtual void TickState(float dt)
        {
            switch (m_currState)
            {
                case BattleMainState.Init:
                    break;
                case BattleMainState.Running:
                {
                    // 如果已经结束 进入下个状态
                    if (m_compRuler.IsFinishRuleMatch())
                    {
                        PushBattleEndEvent();
                        StateGoto(BattleMainState.Ending);
                        break;
                    }
                }
                    break;
                case BattleMainState.Ending:
                    // 当没有process时 进入下个阶段
                    if (m_compProcessManager.IsPlayingProcess())
                    {
                        break;
                    }
                    StateGoto(BattleMainState.Closed);
                    break;
                default:
                    break;
            }
        }


        #endregion

        /// <summary>
        /// 启动战斗开始状态改变
        /// </summary>
        public override void BattleStart()
        {
            // 切换到开始状态
            StateGoto(BattleMainState.Starting);
        }


        /// <summary>
        /// 当战斗结束
        /// </summary>
        protected virtual void OnBattleEnd()
        {
            m_currState = BattleMainState.Closed;
        }

        

        

        


        /// <summary>
        /// 推送战斗结束事件
        /// </summary>
        protected void PushBattleEndEvent()
        {

        }

        #region 状态切换函数


        #endregion

        #region 事件


        

        

        /// <summary>
        /// 战斗 结束事件
        /// </summary>


        /// <summary>
        /// 战斗结束 进入close状态 完成整个关闭流程
        /// </summary>
        public event Action<string> EventOnBattleEnd;

        #endregion

        

        /// <summary>
        /// 表现组件
        /// </summary>
        protected BattleLogicCompProcessManager m_compProcessManager;

        protected BattleLogicCompRuler m_compRuler;
        protected BattleLogicCompResolver m_compResolver;
    }
}
