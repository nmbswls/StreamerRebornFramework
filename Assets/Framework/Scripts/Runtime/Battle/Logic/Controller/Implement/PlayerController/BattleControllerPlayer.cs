using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;

namespace My.Framework.Battle
{
    public interface IBattleControllerEnv4Player
    {

    }
    public partial class BattleControllerPlayer : BattleController
    {
        public BattleControllerPlayer() :base()
        {
            
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
        public BattleActor MainPlayerActor;
    }
}
