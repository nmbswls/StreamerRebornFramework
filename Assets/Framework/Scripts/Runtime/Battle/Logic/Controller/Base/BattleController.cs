using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Battle
{

    //public interface IBattleControllerEnv
    //{
    //    /// <summary>
    //    /// Push战斗指令
    //    /// </summary>
    //    /// <param name="cmd"></param>
    //    /// <returns></returns>
    //    bool OptPush(BattleOpt cmd);

    //    /// <summary>
    //    /// OptSeq生成
    //    /// </summary>
    //    /// <returns></returns>
    //    int OptSeqAlloc();
    //}
    
    public partial class BattleController : IBattleControllerInput
    {
        protected BattleController()
        {
            //m_env = env;
        }

        public virtual void Tick(float dt)
        {
            TurnActionStateTick();
        }

        #region 回合状态

        /// <summary>
        /// 状态
        /// </summary>
        public enum TurnActionState
        {
            /// <summary>
            /// 空闲状态
            /// </summary>
            Idle = 0,

            /// <summary>
            /// 回合开始中 - 等待处理回合开始事件
            /// 当处理完毕后 结束该阶段
            /// </summary>
            TurnActionStart,

            /// <summary>
            /// 等待指令输入
            /// </summary>
            WaitOptCmdInput,

            /// <summary>
            /// 回合流程中
            /// </summary>
            TurnActionExecuting,

            /// <summary>
            /// 回合结束中 - 等待处理回合开始事件
            /// 当处理完毕后 结束该阶段
            /// </summary>
            TurnActionEnd
        }

        /// <summary>
        /// Tick回合状态
        /// </summary>
        private void TurnActionStateTick()
        {
            switch (m_currState)
            {
                case TurnActionState.TurnActionStart:
                    // 仍有表现层播放 跳过
                    if (Owner.CompProcessManager.IsPlayingProcess())
                    {
                        break;
                    }
                    // 完成表现 等待输入
                    TurnActionStateGoto(TurnActionState.WaitOptCmdInput);
                    EventOnExitTurnStart?.Invoke(this);
                    break;
                case TurnActionState.TurnActionEnd:
                    // 仍有表现层播放 跳过
                    if (Owner.CompProcessManager.IsPlayingProcess())
                    {
                        break;
                    }
                    TurnActionStateGoto(TurnActionState.Idle);
                    // 触发事件
                    EventOnExitTurnEnd?.Invoke(this);
                    break;
                
                case TurnActionState.WaitOptCmdInput:
                    {
                        break;
                    }
                case TurnActionState.TurnActionExecuting:
                    {
                        // 仍有表现层播放 跳过
                        if (Owner.CompProcessManager.IsPlayingProcess())
                        {
                            break;
                        }
                        // 切换状态
                        TurnActionStateGoto(TurnActionState.WaitOptCmdInput);
                        break;
                    }
                case TurnActionState.Idle:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <param name="newState"></param>
        protected void TurnActionStateGoto(TurnActionState newState)
        {
            Debug.Log($"TurnActionStateGoto newState {newState}");
            m_currState = newState;
            switch(newState)
            {
                case TurnActionState.TurnActionEnd:
                    EventOnEnterTurnEnd?.Invoke(this);
                    break;
                case TurnActionState.TurnActionStart:
                    EventOnEnterTurnStart?.Invoke(this);
                    break;
                case TurnActionState.WaitOptCmdInput:
                    EventOnWaitOptCmdInput?.Invoke(this);
                    break;
            }
        }

        /// <summary>
        /// 当前的回合状态
        /// </summary>
        public TurnActionState CurrState
        {
            get { return m_currState; }
        }
        protected TurnActionState m_currState;

        #endregion

        #region 操作指令到来

        /// <summary>
        /// 当操作指令输入完成
        /// </summary>
        protected void OnEventOptCmdInputEnd()
        {
            // 1. 设置回合回合流程中
            TurnActionStateGoto(TurnActionState.TurnActionExecuting);
        }

        #endregion

        

        public void DoTurnStart()
        {
            // 触发类效果
            Owner.CompResolver.OpenResolveCtx(EnumTriggereSourceType.TurnStart);

            foreach (var actor in Owner.GetActorsByCamp(ControllerId))
            {
                actor.OnTrigger(EnumBuffTriggerType.TurnStart);
            }
            Owner.CompResolver.TickResolve();

            TurnActionStateGoto(TurnActionState.TurnActionStart);
        }

        /// <summary>
        /// 结束回合 由cmd组件调用
        /// </summary>
        public void DoTurnEnd()
        {
            // 触发类效果
            Owner.CompResolver.OpenResolveCtx(EnumTriggereSourceType.TurnEnd);

            foreach (var actor in Owner.GetActorsByCamp(ControllerId))
            {
                actor.OnTrigger(EnumBuffTriggerType.TurnEnd);
            }
            Owner.CompResolver.TickResolve();

            // 切换状态
            TurnActionStateGoto(TurnActionState.TurnActionEnd);
        }

        public bool CheckOptInput(BattleOpt opt)
        {
            return m_input.CheckOptInput(opt);
        }

        public bool ExecOptCmd(BattleOpt opt)
        {

            var execRet = m_input.ExecOptCmd(opt);
            if(execRet)
            {
                // 切换状态
                TurnActionStateGoto(TurnActionState.TurnActionExecuting);
            }
            return execRet;
        }

        #region 事件

        /// <summary>
        /// 回合开始环节 - 开始
        /// </summary>
        public event Action<BattleController> EventOnEnterTurnStart;

        /// <summary>
        /// 回合开始环节 - 结束
        /// </summary>
        public event Action<BattleController> EventOnExitTurnStart;

        /// <summary>
        /// 回合结束环节 - 开始
        /// </summary>
        public event Action<BattleController> EventOnEnterTurnEnd;
        /// <summary>
        /// 回合结束环节 - 完成
        /// </summary>
        public event Action<BattleController> EventOnExitTurnEnd;

        /// <summary>
        /// 开始等待输入
        /// </summary>
        public event Action<BattleController> EventOnWaitOptCmdInput;

        /// <summary>
        /// 等待操作指令输入的事件
        /// </summary>
        public event Action<BattleController> EventOnOptWait4Input;

        #endregion

        #region 组件

        /// <summary>
        /// input
        /// </summary>
        protected BattleControllerInput m_input;

        #endregion

        #region 状态变量

        /// <summary>
        /// 
        /// </summary>
        protected bool m_flagAuto = false;
        public bool FlagAuto { get { return m_flagAuto; } }

        #endregion

        #region 内部变量

        public BattleLogic Owner;
        //public IBattleControllerEnv m_env;

        /// <summary>
        /// 动态的controllerId
        /// </summary>
        protected int m_controllerId;

        public virtual int ControllerId
        {
            get { return m_controllerId; }
        }

        #endregion
    }
}
