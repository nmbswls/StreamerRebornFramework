using My.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle.View
{
    /// <summary>
    /// 单例战斗管理器
    /// </summary>
    public abstract class BattleManagerBase
    {

        #region 单例相关


        protected BattleManagerBase()
        {
        }

        public static BattleManagerBase Instance
        {
            get
            {
                return GameManager.Instance.BattleManager;
            }
        }

        public static bool HasInstance
        {
            get { return GameManager.Instance.BattleManager != null; }
        }

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            m_battleLoader = CreateBattleLoader();
            m_sceneManager = CreateBattleSceneManager();
            RegisterListener();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void UnInit()
        {
            UnregisterListener();
        }

        public void OnTick(float deltaTime)
        {
            if (!m_battleLoader.isComplete)
            {
                m_battleLoader.Tick();
            }

            if (!m_isRunning || m_isErrorOccured)
            {
                return;
            }

            //battle里面会堆积事件
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    BattleLogic.Tick(0);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    m_isErrorOccured = true;
                }
            }

            m_showProcessHandler.Tick(deltaTime);
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="battleId"></param>
        public void StartBattle(int battleId)
        {
            if (battleId == 0)
            {
                // 断线重连时，可能会出现sceneId=0的情况
                Debug.Log("StartBattle failed, battleSceneId=0");
                return;
            }
            if (m_isBlockStartBattle)
            {
                return;
            }
            m_isBlockStartBattle = true;

            GameManager.Instance.GamePlayer.GamePlayerLogic.BattleCtxOpen(new BattleLaunchInfo());
            BattleLogic = GameManager.Instance.GamePlayer.GamePlayerLogic.CurrBattleMainGet();

            PostOpenBattle(BattleLogic.BattleStart);
        }

        #region 供子类重写

        /// <summary>
        /// 创建监听组件
        /// </summary>
        /// <returns></returns>
        protected virtual ViewBattleEventDispatcher CreateEventListener()
        {
            return new ViewBattleEventDispatcher();
        }

        /// <summary>
        /// 创建监听组件
        /// </summary>
        /// <returns></returns>
        protected abstract BattleSceneManagerBase CreateBattleSceneManager();

        /// <summary>
        /// 创建加载器
        /// </summary>
        /// <returns></returns>
        protected virtual BattleLoader CreateBattleLoader()
        {
            return new BattleLoader();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegisterListener()
        {
            m_battleEventDispatcher.RegisterListener<string>(BattleEventIds.BattleEnd, OnBattleFinalize);
            m_battleEventDispatcher.RegisterListener<List<BattleShowProcess>>(BattleEventIds.FlushProcess, OnFlushProcess);
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        protected virtual void UnregisterListener()
        {
        }


        #endregion

        #region 事件


        #region 内部方法

        /// <summary>
        /// 逻辑层初始化完毕，显示层初始化
        /// </summary>
        /// <param name="actionOnEnd"></param>
        protected void PostOpenBattle(Action actionOnEnd)
        {
            m_battleEventDispatcher.Init(BattleLogic);

            if (!m_battleLoader.isComplete)
            {
                m_battleLoader.Load(1, () =>
                {
                    m_isBlockStartBattle = false;
                    m_isRunning = true;

                    actionOnEnd();
                });
            }
            else
            {
                m_isBlockStartBattle = false;
                m_isRunning = true;
                BattleManagerBase.Instance.SceneManager.RestartBattle();
                actionOnEnd();
                //EventManager.Instance.BroadCast(EventID.BattleMessage_BattleStarted, m_mainBattle.GetConfBattleSceneInfo().ID, m_replayBattle);
            }
        }

        /// <summary>
        /// 战斗结束
        /// </summary>
        protected void OnBattleFinalize(string result)
        {
            // do 表现层

            {
                m_isRunning = false;
                // build battle result and pop up
                //m_battleResult.StartResult(finalRecord, m_collection, needShowBattleResult);
                //PlayerContext.SetGameSessionOperateProxy(null);
                //CollectBattleEndStepParam();
                //LoadingUITask.StartLoadingUITask(loadingType, OnLoadingEndAction);
                EventOnBattleEnd?.Invoke();
            }
        }


        /// <summary>
        /// process表现层
        /// </summary>
        /// <param name="processList"></param>
        protected void OnFlushProcess(List<BattleShowProcess> processList)
        {
            foreach (var process in processList)
            {
                m_showProcessHandler.InputProcess(process);
            }
        }

        #endregion



        #endregion

        //状态
        private bool m_isBlockStartBattle;
        private bool m_isRunning;
        private bool m_isErrorOccured;

        /// <summary>
        /// 加载器
        /// </summary>
        protected BattleLoader m_battleLoader = new BattleLoader();

        /// <summary>
        /// 显示层事件分发器 - 分发逻辑事件
        /// </summary>
        protected ViewBattleEventDispatcher m_battleEventDispatcher = new ViewBattleEventDispatcher();
        public ViewBattleEventDispatcher BattleEventDispatcher
        {
            get { return m_battleEventDispatcher; }
        }

        /// <summary>
        /// 场景控制器
        /// </summary>
        public BattleSceneManagerBase SceneManager 
        {
            get
            {
                return m_sceneManager;
            }
        }
        protected BattleSceneManagerBase m_sceneManager;

        /// <summary>
        /// process处理器
        /// </summary>
        public ShowProcessHandler m_showProcessHandler = new ShowProcessHandler();

        /// <summary>
        /// 持有逻辑类
        /// </summary>
        public BattleLogic BattleLogic;

        /// <summary>
        /// 战斗结束
        /// </summary>
        public event Action EventOnBattleEnd;
    }
}
