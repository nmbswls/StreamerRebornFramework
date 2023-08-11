using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{

    public class BattleActorCompBuff : BattleActorCompBase
    {
        /// <summary>
        /// 当前的buf列表
        /// </summary>
        public List<BattleActorBuff> BuffList = new List<BattleActorBuff>();
    }
}
