using My.Framework.Battle;
using My.Framework.Runtime.Common;
using System.Collections;
using System.Collections.Generic;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Runtime.Logic
{
    public interface IGamePlayerLogicEnv
    {

        /// <summary>
        /// ��ȡ����Դ
        /// </summary>
        /// <returns></returns>
        IDataContainerHolder GetDataProvider();

        /// <summary>
        /// ��־�ӿ�
        /// </summary>
        /// <returns></returns>
        ILogicLogger LogicLoggerGet();
    }

    public interface IGamePlayerLogic : IGamePlayerCompBattle
    {
        
    }

    public partial class GamePlayerLogic : IGamePlayerLogic, IGamePlayerCompOwnerBase
    {
        public GamePlayerLogic(IGamePlayerLogicEnv env)
        {
            m_env = env;
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// ��ȡ��־
        /// </summary>
        /// <returns></returns>
        public ILogicLogger LogicLoggerGet()
        {
            return m_env.LogicLoggerGet();
        }

        /// <summary>
        /// ��ȡ����Դ
        /// </summary>
        /// <returns></returns>
        public IDataContainerHolder GetDataProvider()
        {
            return m_env.GetDataProvider();
        }

        #region IGamePlayerCompBattle

        /// <summary>
        /// ��ȡ��ǰ����ս��
        /// </summary>
        /// <returns></returns>
        public BattleLogic CurrBattleMainGet()
        {
            return m_compBattle.CurrBattleMainGet();
        }

        /// <summary>
        /// ս���ֳ�����
        /// </summary>
        public void BattleCtxOpen(BattleLaunchInfo battleLaunchInfo)
        {
            m_compBattle.BattleCtxOpen(battleLaunchInfo);
        }

        #endregion

        #region �ڲ�����


        protected GamePlayerCompBattle m_compBattle;

        /// <summary>
        /// ����б�
        /// </summary>
        protected List<GamePlayerCompBase> m_compList = new List<GamePlayerCompBase>();

        /// <summary>
        /// ���ݹ�����
        /// </summary>
        protected DataManagerBase m_dataManagerBase;
        public DataManagerBase DataManager
        {
            get { return m_dataManagerBase; }
        }

        #endregion

        /// <summary>
        /// ����
        /// </summary>
        protected IGamePlayerLogicEnv m_env;
    }
}

