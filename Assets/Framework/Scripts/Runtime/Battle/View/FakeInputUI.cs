using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Battle.View
{
    public class FakeInputUI
    {
        public BattleLogic BattleLogic;
        public void OnEventWaitOptCmdInput()
        {
            // showui
        }

        public void OnInputUI()
        {
            BattleLogic.InputOpt(new BattleOpt());
        }
    }
}
