using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime
{
    
    /// <summary>
    /// 世界状态机
    /// </summary>
    public class GameWorldStateMachineBase : IGameWorldStateEnvBase
    {
        /// <summary>
        /// 获取指定state
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        public GameWorldStateBase GetState(int stateType)
        {
            GameWorldStateBase retState;
            if (m_stateMap.TryGetValue((int)stateType, out retState))
            {
                return retState;
            }
            return null;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            GenerateStates();
            ChangeState(GameWorldStateTypeDefineBase.None);
        }

        /// <summary>
        /// 创建注册状态
        /// </summary>
        protected virtual void GenerateStates()
        {
            AddMapState(CreateState(GameWorldStateTypeDefineBase.None));
            AddMapState(CreateState(GameWorldStateTypeDefineBase.SimpleHall));
            AddMapState(CreateState(GameWorldStateTypeDefineBase.SimpleMap));
        }


        /// <summary>
        /// 创建状态
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        protected virtual GameWorldStateBase CreateState(int stateType)
        {
            switch (stateType)
            {
                case GameWorldStateTypeDefineBase.None:
                    return new GameWorldStateNone();
                case GameWorldStateTypeDefineBase.SimpleHall:
                    return new GameWorldStateSimpleHall();
                case GameWorldStateTypeDefineBase.SimpleMap:
                    return new GameWorldStateSimpleMap();
            }
            return null;
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <returns></returns>
        public int GetCurStateType()
        {
            if (CurrentState != null)
            {
                return CurrentState.StateType;
            }

            return 0;
        }

        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="isStopPre"></param>
        public void ChangeState(int newState, bool isStopPre = true)
        {
            var state = GetState(newState);
            if (state == null)
            {
                Debug.LogError($"ChangeState Error : {newState}");
                return;
            }
            ChangeStateTo(state, isStopPre);
        }


        /// <summary>
        /// 回退到之前的状态
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="isStopPre"></param>
        public void ReturnState()
        {

        }

        public void Tick()
        {
            if (CurrentState != null)
                CurrentState.OnUpdate();
            m_corutineWrapper.Tick();
        }


        /// <summary>
        /// 暂停当前状态
        /// </summary>
        public void Pause()
        {
            if (CurrentState != null)
                CurrentState.Pause();
        }

        public void Resume()
        {
            var cueeState = GetCurStateType();
            var state = GetState(cueeState);
            state.Resume();
        }


        #region 内部方法

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="state"></param>
        /// <param name="isStopPre"></param>
        protected void ChangeStateTo(GameWorldStateBase state, bool isStopPre = true)
        {
            // 不存在state 或 未发生切换
            if (state == null || state == CurrentState)
            {
                return;
            }

            // TODO 是否保留之前的state
            if (PreState != null && PreState.IsPaused && PreState != state)
            {
                PreState.Exit();
            }

            PreState = CurrentState;
            if (CurrentState != null)
            {
                if (isStopPre)
                {
                    CurrentState.Exit();
                }
                else
                {
                    CurrentState.Pause();
                }
            }

            CurrentState = state;
            CurrentState.Enter();
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        protected void AddMapState(GameWorldStateBase state)
        {
            if (!m_stateMap.ContainsKey(state.StateType))
            {
                m_stateMap.Add(state.StateType, state);
                state.Init(this);
            }
        }

        /// <summary>
        /// 执行携程
        /// </summary>
        /// <param name="corutine"></param>
        public void StartCorutine(IEnumerator corutine)
        {
            m_corutineWrapper.StartCoroutine(corutine);
        }

        #endregion

        #region 内部成员

        /// <summary>
        /// 当前状态
        /// </summary>
        public GameWorldStateBase CurrentState { get; set; }
        public GameWorldStateBase PreState { get; set; }

        public Dictionary<int, GameWorldStateBase> m_stateMap = new Dictionary<int, GameWorldStateBase>();

        protected SimpleCoroutineWrapper m_corutineWrapper = new SimpleCoroutineWrapper();

        #endregion
    }
}
