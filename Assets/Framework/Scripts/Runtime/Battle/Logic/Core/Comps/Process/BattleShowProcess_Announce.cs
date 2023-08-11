using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// 显示宣告 
    /// </summary>
    public class BattleShowProcess_Announce : BattleShowProcess
    {
        /// <summary>
        /// 目标actor id
        /// </summary>
        protected uint m_actorId;
        protected float m_waitTime;

        public BattleShowProcess_Announce(uint actorId, float waitTime) : base()
        {
            m_actorId = actorId;
            m_waitTime = waitTime;
        }

        public override int Type
        {
            get { return ProcessTypes.Announce; }
        }
    }
}
