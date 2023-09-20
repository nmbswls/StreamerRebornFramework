using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public class EffectNodeShow : EffectNode
    {
        public override BattleEffectType EffectType { get { return BattleEffectType.Show; } }

        public EffectNodeShow() : base()
        {

        }

        /// <summary>
        /// 施加者actorId
        /// </summary>
        public uint SourceActorId = 0;

        /// <summary>
        /// show info
        /// </summary>
        public string ShowInfo;
    }

}
