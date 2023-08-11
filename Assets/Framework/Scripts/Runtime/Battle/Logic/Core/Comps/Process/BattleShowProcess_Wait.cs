using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// 等待 
    /// </summary>
    public class BattleShowProcess_Wait : BattleShowProcess
    {
        /// <summary>
        /// 等待时间
        /// </summary>
        public float WaitTime;

        public BattleShowProcess_Wait(float waitTime) : base()
        {
            WaitTime = waitTime;
        }

        public override int Type
        {
            get { return ProcessTypes.Wait; }
        }
    }
}
