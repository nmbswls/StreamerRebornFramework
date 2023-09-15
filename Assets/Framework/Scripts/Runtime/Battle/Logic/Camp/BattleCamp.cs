using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 阵营类型
    /// </summary>
    public enum BattleCampType
    {
        Invalid,
        Env, // 环境类
    }

    public class BattleCamp
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public BattleCamp(int id, BattleCampType type)
        {
            m_id = id;
            m_teamType = type;
            m_teamMembers = new List<BattleActor>();
        }

        /// <summary>
        /// 小队id
        /// </summary>
        protected int m_id;

        /// <summary>
        /// 小队类型
        /// </summary>
        protected BattleCampType m_teamType;

        /// <summary>
        /// 阵营actor列表
        /// </summary>
        protected List<BattleActor> m_teamMembers;
    }
}
