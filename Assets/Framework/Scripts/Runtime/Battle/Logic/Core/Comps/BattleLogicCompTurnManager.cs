using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 对外接口
    /// </summary>
    public interface IBattleLogicCompTurnManager
    {
        /// <summary>
        /// 获取当前行动中的controller
        /// </summary>
        /// <returns></returns>
        BattleController CurrTurnActionController();
    }


    public class BattleLogicCompTurnManager : BattleLogicCompBase, IBattleLogicCompTurnManager
    {

        public BattleLogicCompTurnManager(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName
        {
            get { return GamePlayerCompNames.TurnManager; }
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

            m_compActorContainer = m_owner.CompGet<BattleLogicCompActorContainer>(GamePlayerCompNames.ActorManager);
            m_compFsm = m_owner.CompGet<BattleLogicCompFsm>(GamePlayerCompNames.FSM);

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

            //m_compFsm.EventOnBattleStateStarting += OnBattleStateStarting;

            // 初始化controller
            foreach (var ctrl in m_compMain.m_controllers)
            {
                ctrl.EventOnExitTurnStart += OnControllerTurnEnd;
            }

            return true;
        }

        #endregion

        public int TurnNumber = 0;
        protected TurnContext m_turnContext = new TurnContext();
        public class TurnContext
        {
            /// <summary>
            /// 行动过的ctrl列表
            /// </summary>
            public List<BattleController> m_actedControllers = new List<BattleController>();

            /// <summary>
            /// 当前行动对象
            /// </summary>
            public BattleController m_currActCtrl = null;
            public void Clear()
            {
                m_actedControllers.Clear();
                m_currActCtrl = null;
            }
        }

        

        /// <summary>
        /// 控制器回合结束事件
        /// </summary>
        /// <param name="ctrl"></param>
        public void OnControllerTurnEnd(BattleController ctrl)
        {
            if (ctrl != m_turnContext.m_currActCtrl)
            {
                Debug.LogError("OnControllerTurnEnd not current action ctrl.");
                return;
            }

            // 行动结束
            m_turnContext.m_actedControllers.Add(m_turnContext.m_currActCtrl);

            if (IsAllControllerFinished())
            {
                EnterNextTurn();
            }
            // 选择下个行动对象
            var nextController = GetNextController();
            if (nextController == null)
            {
                Debug.LogError("No Valid Controller");
                return;
            }
            m_turnContext.m_currActCtrl = nextController;
            m_turnContext.m_currActCtrl.DoTurnStart();
        }

        /// <summary>
        /// 获取当前回合的actor
        /// </summary>
        /// <returns></returns>
        public BattleController CurrTurnActionController()
        {
            if (m_turnContext == null || m_turnContext.m_currActCtrl == null)
            {
                return null;
            }

            if (m_turnContext.m_currActCtrl.CurrState == BattleController.TurnActionState.Idle)
            {
                return null;
            }

            return m_turnContext.m_currActCtrl;
        }

        #region 流程方法

        /// <summary>
        /// 是否所有controller行动过
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsAllControllerFinished()
        {
            return m_compMain.m_controllers.Count == m_turnContext.m_actedControllers.Count;
        }

        /// <summary>
        /// 获取下一个行动的controller
        /// </summary>
        /// <returns></returns>
        protected BattleController GetNextController()
        {
            BattleController nextCtrl = null;
            
            foreach (var ctrl in m_compMain.m_controllers)
            {
                if (!m_turnContext.m_actedControllers.Contains(ctrl))
                {
                    nextCtrl = ctrl;
                    break;
                }
            }
            return nextCtrl;
        }

        /// <summary>
        /// 进入下个回合
        /// </summary>
        protected void EnterNextTurn()
        {
            m_turnContext.Clear();
            TurnNumber += 1;
        }

        #endregion

        #region 内部组件

        /// <summary>
        /// actor
        /// </summary>
        protected BattleLogicCompActorContainer m_compActorContainer;

        /// <summary>
        /// Main
        /// </summary>
        protected BattleLogicCompMain m_compMain;

        /// <summary>
        /// 状态机
        /// </summary>
        protected BattleLogicCompFsm m_compFsm;

        #endregion

    }
}
