using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
{
    /// <summary>
    /// 战斗操作指令
    /// </summary>
    [Serializable]
    public class BattleOptExtraInfo_Skill : BattleOptExtraInfo
    {
        /// <summary>
        /// 技能id
        /// </summary>
        public int m_skillId;

        /// <summary>
        /// 目标信息
        /// </summary>
        public int m_targetInfo;
    }
}
