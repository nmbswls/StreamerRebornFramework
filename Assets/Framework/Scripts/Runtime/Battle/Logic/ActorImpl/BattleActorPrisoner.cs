using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public class BattleActorPrisoner : BattleActor
    {
        public BattleActorPrisoner(IBattleActorEnvBase env) : base(env)
        {
        }
        public override int ActorType
        {
            get { return BattleActorTypeStartup.Enemy; }
        }
    }
}
