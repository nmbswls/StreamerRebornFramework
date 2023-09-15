using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public class EffectNodeChooseCard : EffectNode
    {
        public override BattleEffectType EffectType { get { return BattleEffectType.ChooseCard; } }

        public EffectNodeChooseCard() : base()
        {

        }

        

        public List<int> ChooseSet;
        public int ChoosedIdx;
    }

}
