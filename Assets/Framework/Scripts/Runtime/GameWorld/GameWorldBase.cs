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
    /// ��Ϸ����
    /// </summary>
    public class GameWorldBase
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public void Init()
        {
            // ����״̬��
            m_stateMachine = CreateStateMachine();
            m_stateMachine.Init();
        }

        /// <summary>
        /// ��ͣ
        /// </summary>
        public void Pause()
        {
            m_stateMachine.Pause();
        }

        /// <summary>
        /// �ָ�
        /// </summary>
        public void Resume()
        {
            m_stateMachine.Resume();
        }

        /// <summary>
        /// ����
        /// </summary>
        public void Tick()
        {
            m_stateMachine?.Tick();
        }

        #region ҵ�����

        /// <summary>
        /// �ص���ʼ״̬
        /// </summary>
        public void ResetToOrigin()
        {
            m_stateMachine.ChangeState(GameWorldStateTypeDefineBase.None);
        }

        /// <summary>
        /// �������
        /// </summary>
        public void EnterHall()
        {
            m_stateMachine.ChangeState(GameWorldStateTypeDefineBase.SimpleHall);
        }
        
        #endregion

        #region ����̳�

        /// <summary>
        /// ��ʵ���ഴ�����Ե�״̬��
        /// </summary>
        /// <returns></returns>
        protected virtual GameWorldStateMachineBase CreateStateMachine()
        {
            return new GameWorldStateMachineBase();
        }

        #endregion

        /// <summary>
        /// ״̬��
        /// </summary>
        protected GameWorldStateMachineBase m_stateMachine;
    }
}
