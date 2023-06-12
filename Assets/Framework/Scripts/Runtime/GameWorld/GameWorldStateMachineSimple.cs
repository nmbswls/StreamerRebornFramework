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
    /// 状态定义
    /// </summary>
    public class GameWorldStateTypeDefineSample : GameWorldStateTypeDefineBase
    {
        
    }

    /// <summary>
    /// 最简单的世界状态机
    /// </summary>
    public class GameWorldStateMachineSimple : GameWorldStateMachineBase, IGameWorldStateEnvBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void GenerateStates()
        {
            base.GenerateStates();
        }
    }

    /// <summary>
    /// 真是的gameworld
    /// </summary>
    public class GameWorldSample : GameWorldBase
    {
        /// <summary>
        /// 进入世界
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="stopLoading"></param>
        /// <param name="onEnterEnd"></param>
        public void EnterWorldMap(int mapId = 0, bool stopLoading = true, Action onEnterEnd = null)
        {
            var worldState = m_stateMachine.GetState(GameWorldStateTypeDefineSample.SimpleMap) as GameWorldStateSimpleMap;
            if (worldState != null)
            {
                m_stateMachine.ChangeState(GameWorldStateTypeDefineSample.SimpleMap, false);
            }
        }

        /// <summary>
        /// 由实现类创建各自的状态机
        /// </summary>
        /// <returns></returns>
        protected override GameWorldStateMachineBase CreateStateMachine()
        {
            return new GameWorldStateMachineSimple();
        }

        /// <summary>
        /// 具体实现类的状态机
        /// </summary>
        protected GameWorldStateMachineSimple StateMachine { get { return (GameWorldStateMachineSimple)m_stateMachine; } }
    }
}
