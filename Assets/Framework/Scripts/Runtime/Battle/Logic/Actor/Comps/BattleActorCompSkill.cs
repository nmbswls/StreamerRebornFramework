using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    /// <summary>
    /// 技能相关
    /// </summary>
    public class BattleActorCompSkill : BattleActorCompBase
    {
        /// <summary>
        /// 当前的Skill列表
        /// </summary>
        public List<BattleActorSkill> SkillList = new List<BattleActorSkill>();
    }
}
