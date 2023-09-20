using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// 显示特效 
    /// </summary>
    public class BattleShowProcess_Show : BattleShowProcess
    {

        public BattleShowProcess_Show(string showInfo, float waitTime) : base()
        {
        }

        public override int Type
        {
            get { return ProcessTypes.Show; }
        }
    }
}
