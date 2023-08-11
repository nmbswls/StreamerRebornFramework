using My.ConfigData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class CardInstanceInfo
    {
        public uint InstanceId { get; set; }

        public ConfigDataCardBattleInfo Config { get; set; }

        /// <summary>
        /// 制造出来的临时卡片
        /// </summary>
        public bool IsTemp;
    }
}

