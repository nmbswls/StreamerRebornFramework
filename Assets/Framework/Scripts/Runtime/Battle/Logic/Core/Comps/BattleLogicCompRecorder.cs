using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 战斗记录组件
    /// </summary>
    public class BattleLogicCompRecorder : BattleLogicCompBase
    {
        public BattleLogicCompRecorder(IBattleLogicCompOwnerBase owner) :base(owner)
        {

        }

        public override string CompName { get { return GamePlayerCompNames.Recorder; } }

        public void OnOptInput(BattleOpt opt)
        {
            m_battleOptRecords.Add(opt);
        }

        public List<BattleOpt> m_battleOptRecords = new List<BattleOpt>();
    }
}
