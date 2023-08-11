using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
{
    /// <summary>
    /// 战斗指令类型
    /// </summary>
    public enum BattleOptType
    {
        /// <summary>
        /// 无效指令
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 技能使用
        /// m_intParam: 使用的技能id
        /// </summary>
        SkillCast = 1,

        /// <summary>
        /// 结束回合
        /// </summary>
        EndTurn = 2
    }

    /// <summary>
    /// 战斗操作指令
    /// </summary>
    [Serializable]
    public class BattleOpt
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public int m_seq;

        /// <summary>
        /// 战斗操作指令类型
        /// </summary>
        public BattleOptType m_type;

        /// <summary>
        /// 操作发起者
        /// </summary>
        public int m_controllerId;

        /// <summary>
        /// 额外信息
        /// </summary>
        public BattleOptExtraInfo m_optExtraInfo;
    }

    /// <summary>
    /// 战斗操作指令
    /// </summary>
    [Serializable]
    public class BattleOptExtraInfo
    {
        public virtual bool IsValid()
        {
            return true;
        }
    }
}
