using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{

    public class BattleAudienceAttribute
    {
        private BattleAudience m_owner;
        /// <summary>
        /// �����
        /// </summary>
        public int Satisfaction;

        /// <summary>
        /// �������� ���˾�����
        /// </summary>
        public int MaxSatisfaction;

        /// <summary>
        /// ʣ��غ���
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

