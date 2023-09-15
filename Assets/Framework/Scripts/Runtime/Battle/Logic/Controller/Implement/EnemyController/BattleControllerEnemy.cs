using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;

namespace My.Framework.Battle
{

    public partial class BattleControllerEnemy : BattleController
    {
        public BattleControllerEnemy() :base()
        {
            
        }

        #region 执行指令


        #endregion

        public override int ControllerId
        {
            get { return s_EnemyControllerId; }
        }

        public static int s_EnemyControllerId = 2;

        /// <summary>
        /// 玩法相关 特殊actor
        /// </summary>
        public BattleActor MainEnemyActor;
    }
}
