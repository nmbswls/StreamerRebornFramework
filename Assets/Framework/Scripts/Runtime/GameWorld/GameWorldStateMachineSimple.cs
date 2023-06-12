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
    /// ״̬����
    /// </summary>
    public class GameWorldStateTypeDefineSample : GameWorldStateTypeDefineBase
    {
        
    }

    /// <summary>
    /// ��򵥵�����״̬��
    /// </summary>
    public class GameWorldStateMachineSimple : GameWorldStateMachineBase, IGameWorldStateEnvBase
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        protected override void GenerateStates()
        {
            base.GenerateStates();
        }
    }

    /// <summary>
    /// ���ǵ�gameworld
    /// </summary>
    public class GameWorldSample : GameWorldBase
    {
        /// <summary>
        /// ��������
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
        /// ��ʵ���ഴ�����Ե�״̬��
        /// </summary>
        /// <returns></returns>
        protected override GameWorldStateMachineBase CreateStateMachine()
        {
            return new GameWorldStateMachineSimple();
        }

        /// <summary>
        /// ����ʵ�����״̬��
        /// </summary>
        protected GameWorldStateMachineSimple StateMachine { get { return (GameWorldStateMachineSimple)m_stateMachine; } }
    }
}
