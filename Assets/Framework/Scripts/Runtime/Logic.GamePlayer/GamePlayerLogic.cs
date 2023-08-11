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
        /// 获取数据源
        /// </summary>
        /// <returns></returns>
        IDataContainerHolder GetDataProvider();

        /// <summary>
        /// 日志接口
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
        /// 初始化
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// 获取日志
        /// </summary>
        /// <returns></returns>
        public ILogicLogger LogicLoggerGet()
        {
            return m_env.LogicLoggerGet();
        }

        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <returns></returns>
        public IDataContainerHolder GetDataProvider()
        {
            return m_env.GetDataProvider();
        }

        #region IGamePlayerCompBattle

        /// <summary>
        /// 获取当前进行战斗
        /// </summary>
        /// <returns></returns>
        public BattleLogic CurrBattleMainGet()
        {
            return m_compBattle.CurrBattleMainGet();
        }

        /// <summary>
        /// 战斗现场开启
        /// </summary>
        public void BattleCtxOpen(BattleLaunchInfo battleLaunchInfo)
        {
            m_compBattle.BattleCtxOpen(battleLaunchInfo);
        }

        #endregion

        #region 内部变量


        protected GamePlayerCompBattle m_compBattle;

        /// <summary>
        /// 组件列表
        /// </summary>
        protected List<GamePlayerCompBase> m_compList = new List<GamePlayerCompBase>();

        /// <summary>
        /// 数据管理器
        /// </summary>
        protected DataManagerBase m_dataManagerBase;
        public DataManagerBase DataManager
        {
            get { return m_dataManagerBase; }
        }

        #endregion

        /// <summary>
        /// 环境
        /// </summary>
        protected IGamePlayerLogicEnv m_env;
    }
}

