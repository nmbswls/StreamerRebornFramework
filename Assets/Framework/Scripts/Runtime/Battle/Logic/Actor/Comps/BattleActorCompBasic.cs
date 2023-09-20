using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public class BattleActorCompBasic : BattleActorCompBase
    {
        public uint InstId;
        public int Level;
        public int ActorId;
        public int CampId;

        public bool IsDead;

        public BattleActorCompBasic(int actorId, uint instId)
        {
            ActorId = actorId;
            InstId = instId;
        }
    }
}
