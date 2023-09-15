using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using UnityEditor.PackageManager;

namespace My.Framework.Battle.Logic
{
    public interface IBattleLogicCompMain
    {
        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        BattleController GetBattleControllerById(int controllerId);
    }

    public class BattleLogicCompMain : BattleLogicCompBase, IBattleLogicCompMain
    {
        public BattleLogicCompMain(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName
        {
            get { return GamePlayerCompNames.Main; }
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!base.Initialize())
            {
                return false;
            }

            m_compActorContainer = m_owner.CompGet<BattleLogicCompActorContainer>(GamePlayerCompNames.ProcessManager);
            m_compProcessManager = m_owner.CompGet<BattleLogicCompProcessManager>(GamePlayerCompNames.ProcessManager);
            m_compRuler = m_owner.CompGet<BattleLogicCompRuler>(GamePlayerCompNames.Ruler);
            m_compResolver = m_owner.CompGet<BattleLogicCompResolver>(GamePlayerCompNames.Resolver);

            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            if (!base.PostInitialize())
            {
                return false;
            }

            //创建controller
            InitControllers();

            // 装配状态切换回调
            m_stateEnterCbDict.Clear();
            m_stateEnterCbDict[BattleMainState.Starting] = OnStateEnter_Starting;
            m_stateEnterCbDict[BattleMainState.Running] = OnStateEnter_Running;
            m_stateEnterCbDict[BattleMainState.Ending] = OnStateEnter_Ending;
            m_stateEnterCbDict[BattleMainState.Closed] = OnStateEnter_Closed;

            StateGoto(BattleMainState.Init);

            return true;
        }

        #region Overrides of BattleLogicCompBase

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            TickState(dt);
        }

        #endregion

        #endregion

        #region 对外方法

        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        public BattleController GetBattleControllerById(int controllerId)
        {
            return m_controllers[controllerId];
        }

        /// <summary>
        /// 召唤actor
        /// </summary>
        /// <param name="actorConfigId"></param>
        /// <param name="actorType"></param>
        /// <param name="campId"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool SummonActor(int actorConfigId, int actorType, int campId, out BattleActor actor)
        {
            actor = m_compActorContainer.CreateActor(actorType, actorConfigId, BattleActorSourceType.Summon, campId);
            if (actor == null)
            {
                return false;
            }
            
            //actor.Init4EnterBattle();
            return true;
        }

        /// <summary>
        /// 启动战斗开始状态改变
        /// </summary>
        public override void BattleStart()
        {
            // 切换到开始状态
            StateGoto(BattleMainState.Starting);
        }

        #endregion

        #region 战斗状态

        /// <summary>
        /// 状态
        /// </summary>
        public enum BattleMainState
        {
            Init = 0, // 无效状态 初始化

            Starting,

            Running,

            Ending,

            Closed,
        }


        /// <summary>
        /// Tick推进逻辑进行
        /// </summary>
        protected virtual void TickState(float dt)
        {
            switch (m_currState)
            {
                case BattleMainState.Init:
                    break;
                case BattleMainState.Running:
                {
                    // 如果已经结束 进入下个状态
                    if (m_compRuler.IsFinishRuleMatch())
                    {
                        StateGoto(BattleMainState.Ending);
                        break;
                    }
                }
                    break;
                case BattleMainState.Ending:
                    // 当没有process时 进入下个阶段
                    if (m_compProcessManager.IsPlayingProcess())
                    {
                        break;
                    }
                    StateGoto(BattleMainState.Closed);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <param name="enterState"></param>
        protected void StateGoto(BattleMainState enterState)
        {
            // 1. 设置当前状态
            m_currState = enterState;

            // 2. 调用state的enter流程
            StateEnter(m_currState);
        }

        /// <summary>
        /// 状态进入
        /// </summary>
        /// <param name="enterState"></param>
        protected void StateEnter(BattleMainState enterState)
        {
            // 触发回调
            if (m_stateEnterCbDict.ContainsKey(enterState))
            {
                m_stateEnterCbDict[enterState]?.Invoke();
            }
        }

        #region 状态切换函数

        /// <summary>
        /// 切换状态 starting
        /// </summary>
        protected virtual void OnStateEnter_Starting()
        {
            // 推入process
            m_compProcessManager.PushProcessToCache(new BattleShowProcess_Print("Now Battle Start Yeah."));

            // 触发效果
            EventOnBattleStateStarting?.Invoke();

            // 显示层表现
            m_compProcessManager.FlushAndRaiseEvent();
        }

        /// <summary>
        /// 切换状态 running
        /// </summary>
        protected virtual void OnStateEnter_Running()
        {
            // handle process
            // 
            EventOnBattleStateRunning?.Invoke();
        }

        protected virtual void OnStateEnter_Ending()
        {
            // handle process

            // 
            EventOnBattleStateEnding?.Invoke();

            // 显示层表现
            m_compProcessManager.FlushAndRaiseEvent();
        }

        protected virtual void OnStateEnter_Closed()
        {
            // 抛出事件
            EventOnBattleStateClosed?.Invoke();
        }

        #endregion

        /// <summary>
        /// 当前的状态
        /// </summary>
        protected BattleMainState m_currState;
        /// <summary>
        /// 状态切换字典
        /// </summary>
        protected Dictionary<BattleMainState, Action> m_stateEnterCbDict =
            new Dictionary<BattleMainState, Action>();
        // 状态切换函数
        public event Action EventOnBattleStateStarting;
        public event Action EventOnBattleStateRunning;
        public event Action EventOnBattleStateEnding;
        public event Action EventOnBattleStateClosed;

        #endregion

        #region 内部方法

        /// <summary>
        /// 初始化阵营 控制器
        /// </summary>
        protected void InitControllers()
        {
            var initInfo = m_owner.BattleInitInfoGet();
            m_controllers.Add(new BattleControllerPlayer());
            m_controllers.Add(new BattleControllerEnemy());
        }

        #endregion

        #region 组件

        /// <summary>
        /// actor
        /// </summary>
        protected BattleLogicCompActorContainer m_compActorContainer;

        /// <summary>
        /// 表现组件
        /// </summary>
        protected BattleLogicCompProcessManager m_compProcessManager;

        /// <summary>
        /// 判定结束
        /// </summary>
        protected BattleLogicCompRuler m_compRuler;

        /// <summary>
        /// 效果结算
        /// </summary>
        protected BattleLogicCompResolver m_compResolver;

        /// <summary>
        /// 战斗控制器列表
        /// </summary>
        public List<BattleController> m_controllers = new List<BattleController>();

        #endregion
    }
}
