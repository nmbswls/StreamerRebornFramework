using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public class BattleActorCompBasic : BattleActorCompBase
    {
        public uint ActorId;
        public int ActorType;
        public int Level;
        public int ConfigId;
        public int CampId;

        public bool IsDead;
    }
}
