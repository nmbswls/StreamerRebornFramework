using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{

    public class BattleAudienceAttribute
    {
        private BattleAudience m_owner;
        /// <summary>
        /// 满意度
        /// </summary>
        public int Satisfaction;

        /// <summary>
        /// 最大满意度 满了就死亡
        /// </summary>
        public int MaxSatisfaction;

        /// <summary>
        /// 剩余回合数
        /// </summary>
        public int LefeTurn;

        public void Initialize(BattleAudience Owner)
        {
            Satisfaction = 0;
            MaxSatisfaction = 14;
            LefeTurn = 3;
        }
    }
}

