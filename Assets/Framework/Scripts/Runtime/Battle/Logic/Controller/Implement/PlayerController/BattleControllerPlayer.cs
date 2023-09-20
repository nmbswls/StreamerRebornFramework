using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;

namespace My.Framework.Battle.Logic
{
    public interface IBattleControllerEnv4Player
    {

    }
    public partial class BattleControllerPlayer : BattleController
    {
        public BattleControllerPlayer(BattleLogic battleLogic) :base(battleLogic)
        {

        }

        /// <summary>
        /// 创建输入模块
        /// </summary>
        /// <returns></returns>
        protected override BattleControllerInput CreateInputModule()
        {
            return new BattleControllerInputPlayer(this);
        }

        #region 执行指令



        #endregion

        public override int ControllerId
        {
            get { return s_PlayerControllerId; }
        }

        public static int s_PlayerControllerId = 1;

        /// <summary>
        /// 玩法相关 特殊actor
        /// </summary>
        public BattleActor MainPlayerActor
        {
            get
            {
                return BattleLogic.GetFirstActorByType(BattleActorTypeStartup.Player);
            }
        }
    }
}
