using My.Framework.Runtime.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime
{
    
    /// <summary>
    /// 游戏世界
    /// </summary>
    public class GameWorldBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            // 创建状态机
            m_stateMachine = CreateStateMachine();
            m_stateMachine.Init();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            m_stateMachine.Pause();
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            m_stateMachine.Resume();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Tick()
        {
            m_stateMachine?.Tick();
        }

        #region 业务相关

        /// <summary>
        /// 回到初始状态
        /// </summary>
        public void ResetToOrigin()
        {
            m_stateMachine.ChangeState(GameWorldStateTypeDefineBase.None);
        }

        /// <summary>
        /// 进入大厅
        /// </summary>
        public void EnterHall()
        {
            m_stateMachine.ChangeState(GameWorldStateTypeDefineBase.SimpleHall);
        }
        
        #endregion

        #region 子类继承

        /// <summary>
        /// 由实现类创建各自的状态机
        /// </summary>
        /// <returns></returns>
        protected virtual GameWorldStateMachineBase CreateStateMachine()
        {
            return new GameWorldStateMachineBase();
        }

        #endregion

        /// <summary>
        /// 状态机
        /// </summary>
        protected GameWorldStateMachineBase m_stateMachine;
    }
}
