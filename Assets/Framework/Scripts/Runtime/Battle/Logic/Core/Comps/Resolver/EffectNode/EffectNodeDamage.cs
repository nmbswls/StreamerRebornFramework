using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public class EffectNodeDamage : EffectNode
    {
        public override BattleEffectType EffectType { get { return BattleEffectType.Damage; } }

        public EffectNodeDamage() : base()
        {

        }

        /// <summary>
        /// 施加者actorId
        /// </summary>
        public uint SourceActorId = 0;

        /// <summary>
        /// 
        /// </summary>
        public uint TargetActorId = 0;
    }

}
