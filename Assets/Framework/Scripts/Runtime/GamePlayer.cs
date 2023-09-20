using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Common;
using My.Framework.Runtime.Logic;
using My.Framework.Runtime.Saving;

namespace My.Framework.Runtime
{
    public class GamePlayer : IGamePlayerLogicEnv
    {

        public void OnLoadGame(SavingData savingData)
        {
            m_dataManager.RestructFromSaving(savingData);

            if (m_gamePlayerLogic == null)
            {
                m_gamePlayerLogic = CreateGamePlayerLogic();
            }

            m_gamePlayerLogic.Initialize();
        }

        public void Tick(float dt)
        {
            m_gamePlayerLogic?.Tick(dt);
        }

        protected virtual GamePlayerLogic CreateGamePlayerLogic()
        {
            return new GamePlayerLogic(this);
        }


        public IDataContainerHolder GetDataProvider()
        {
            return m_dataManager;
        }

        public ILogicLogger LogicLoggerGet()
        {
            return GameManager.Instance.LogicLoggerGet();
        }


        /// <summary>
        /// 数据管理器
        /// </summary>
        public DataManagerBase DataManager
        {
            get { return m_dataManager; }
        }

        public DataManagerBase m_dataManager = new DataManagerBase();

        /// <summary>
        /// 玩家逻辑对象
        /// </summary>
        public GamePlayerLogic GamePlayerLogic
        {
            get { return m_gamePlayerLogic; }
        }

        /// <summary>
        /// 玩家逻辑对象
        /// </summary>
        private GamePlayerLogic m_gamePlayerLogic;

        
    }
}
