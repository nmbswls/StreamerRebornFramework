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

            // 装配状态切换回调
            m_stateEnterCbDict.Clear();
            m_stateEnterCbDict[BattleMainState.Starting] = OnStateEnter_Starting;
            m_stateEnterCbDict[BattleMainState.Running] = OnStateEnter_Running;
            m_stateEnterCbDict[BattleMainState.Ending] = OnStateEnter_Ending;
            m_stateEnterCbDict[BattleMainState.Closed] = OnStateEnter_Closed;

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
        /// 状态
        /// </summary>
        public enum BattleMainState
        {
            Init = 0, // 无效状态 初始化

            Starting,

            Running,

            Ending,
            
            Closed,
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <param name="enterState"></param>
        protected void StateGoto(BattleMainState enterState)
        {
            // 1. 设置当前状态
            m_currState = enterState;

            // 2. 调用state的enter流程
            StateEnter(m_currState);
        }

        /// <summary>
        /// 状态进入
        /// </summary>
        /// <param name="enterState"></param>
        protected void StateEnter(BattleMainState enterState)
        {
            // 触发回调
            if (m_stateEnterCbDict.ContainsKey(enterState))
            {
                m_stateEnterCbDict[enterState]?.Invoke();
            }
        }


        /// <summary>
        /// 推送战斗结束事件
        /// </summary>
        protected void PushBattleEndEvent()
        {

        }

        #region 状态切换函数

        /// <summary>
        /// 切换状态 starting
        /// </summary>
        protected virtual void OnStateEnter_Starting()
        {
            // 推入process
            m_compProcessManager.PushProcessToCache(new BattleShowProcess_Print("Now Battle Start Yeah."));

            // 触发效果
            m_compResolver.TriggerResolve(new TriggerNodeBattleStart());
            EventOnBattleStateStarting?.Invoke();

            // 显示层表现
            m_compProcessManager.FlushAndRaiseEvent();
        }

        /// <summary>
        /// 切换状态 running
        /// </summary>
        protected virtual void OnStateEnter_Running()
        {
            // handle process
            // 
            EventOnBattleStateRunning?.Invoke();
        }

        protected virtual void OnStateEnter_Ending()
        {
            // handle process

            // 
            EventOnBattleStateEnding?.Invoke();

            // 显示层表现
            m_compProcessManager.FlushAndRaiseEvent();
        }

        protected virtual void OnStateEnter_Closed()
        {
            // 抛出事件
            EventOnBattleEnd?.Invoke("nmsl");

            // 显示层表现
            m_compProcessManager.FlushAndRaiseEvent();
        }

        #endregion

        #region 事件


        /// <summary>
        /// 状态切换字典
        /// </summary>
        protected Dictionary<BattleMainState, Action> m_stateEnterCbDict =
            new Dictionary<BattleMainState, Action>();

        // 状态切换函数
        public event Action EventOnBattleStateStarting;
        public event Action EventOnBattleStateRunning;
        public event Action EventOnBattleStateEnding;

        /// <summary>
        /// 战斗 结束事件
        /// </summary>


        /// <summary>
        /// 战斗结束 进入close状态 完成整个关闭流程
        /// </summary>
        public event Action<string> EventOnBattleEnd;

        #endregion

        /// <summary>
        /// 当前的状态
        /// </summary>
        protected BattleMainState m_currState;

        /// <summary>
        /// 表现组件
        /// </summary>
        protected BattleLogicCompProcessManager m_compProcessManager;

        protected BattleLogicCompRuler m_compRuler;
        protected BattleLogicCompResolver m_compResolver;
    }
}
