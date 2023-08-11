using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;

namespace My.Framework.Battle
{
    public interface IBattleControllerEnv4Player : IBattleControllerEnv
    {

    }
    public partial class BattleControllerPlayer : BattleController
    {
        public BattleControllerPlayer(IBattleControllerEnv4Player env) :base(env)
        {
            
        }

        #region 执行指令


        

        #endregion

        /// <summary>
        /// 玩法相关 特殊actor
        /// </summary>
        public BattleActor MainPlayerActor;
    }
}
