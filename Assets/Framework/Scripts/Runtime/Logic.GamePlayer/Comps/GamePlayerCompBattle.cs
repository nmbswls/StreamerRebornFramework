using My.Framework.Battle;
using My.Framework.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 对外接口
    /// </summary>
    public interface IGamePlayerCompBattle
    {
        /// <summary>
        /// 获取当前进行战斗
        /// </summary>
        /// <returns></returns>
        BattleLogic CurrBattleMainGet();

        /// <summary>
        /// 战斗现场开启
        /// </summary>
        void BattleCtxOpen(BattleLaunchInfo battleLaunchInfo);
    }

    public  class GamePlayerCompBattle : GamePlayerCompBase, IBattleMainEnv, IBattleMainEventListener
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="owner"></param>
        protected GamePlayerCompBattle(IGamePlayerCompOwnerBase owner) : base(owner)
        {
        }


        /// <summary>
        /// 获取当前进行战斗
        /// </summary>
        /// <returns></returns>
        public BattleLogic CurrBattleMainGet()
        {
            if (m_battleCtx == null) return null;
            return m_battleCtx.MBattleLogic;
        }

        /// <summary>
        /// 战斗现场
        /// </summary>
        public class BattleCtx
        {
            /// <summary>
            /// 战斗来源
            /// </summary>
            public int m_launchReason;

            /// <summary>
            /// 战斗来源参数
            /// </summary>
            public int m_launchReasonParam;

            /// <summary>
            /// 当前的战斗组对象 对于服务器端，是为了方便调试
            /// </summary>
            public BattleLogic MBattleLogic;

            /// <summary>
            /// 默认构造函数
            /// </summary>
            public BattleCtx()
            {

            }
        }

        /// <summary>
        /// 战斗现场开启
        /// </summary>
        public virtual void BattleCtxOpen(BattleLaunchInfo battleLaunchInfo)
        {
            // 1. 构建battleInitInfo
            var battleInitInfo = BattleInitInfoBuild(battleLaunchInfo);

            // 2. 构建BattleGroup，初始化BattleGroup
            var battleMain = new BattleLogic(this, battleInitInfo);

            // 3. 初始化battleGroup
            if (!battleMain.Initialize())
            {
                m_battleCtx = null;
                return;
            }

            // 4. 注册事件
            battleMain.EventListenerAdd(this);

            // 5. 构建BattleCtx
            m_battleCtx = new BattleCtx();
            m_battleCtx.MBattleLogic = battleMain;
            m_battleCtx.m_launchReason = battleLaunchInfo.m_launchReason;
            m_battleCtx.m_launchReasonParam = battleLaunchInfo.m_reasonParam;

            // 6. 持久化数据
            m_owner.GetDataProvider().DataContainerBattleGet().BattlePersistentInfoCreate(battleLaunchInfo.m_launchReason);

            // 7. 抛出战斗现场打开事件
            EventOnBattleCtxOpen?.Invoke(m_battleCtx);
        }


        /// <summary>
        /// 构建初始化信息
        /// </summary>
        public virtual BattleInitInfoBase BattleInitInfoBuild(BattleLaunchInfo battleLaunchInfo)
        {
            return new BattleInitInfoBase();
        }

        /// <summary>
        /// 日志
        /// </summary>
        /// <returns></returns>
        public ILogicLogger LogicLoggerGet()
        {
            return m_owner.LogicLoggerGet();
        }

        /// <summary>
        /// 流程-关闭现场
        /// </summary>
        protected void BattleCtxClose()
        {
            // 1. 清理dc数据
            m_owner.GetDataProvider().DataContainerBattleGet().BattleCtxPersistentInfoRemove();
            // 2. 清理BattleLaunchPersistentInfo
            //BattleLaunchPersistentInfoRemove();

            // 3. 清理现场
            m_battleCtx = null;
        }

        #region 事件

        public event Action<BattleCtx> EventOnBattleCtxOpen;

        #endregion

        #region 内部变量

        /// <summary>
        /// 战斗现场
        /// </summary>
        protected BattleCtx m_battleCtx;

        #endregion

        #region 监听下层事件

        public void OnBattleEnd(string resultInfo)
        {
            throw new NotImplementedException();
        }

        public void OnFlushProcess(List<BattleShowProcess> processList)
        {
            throw new NotImplementedException();
        }


        #endregion

    }
}
