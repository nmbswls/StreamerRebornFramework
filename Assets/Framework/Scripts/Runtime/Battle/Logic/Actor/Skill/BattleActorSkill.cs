using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public class BattleActorSkill
    {
        /// <summary>
        /// 是否是被动技能
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPassive()
        {
            return false;
        }
        
    }

    public class BattleActorSkillPassive : BattleActorSkill
    {
        /// <summary>
        /// 是否是被动技能
        /// </summary>
        /// <returns></returns>
        public override bool IsPassive()
        {
            return true;
        }

        /// <summary>
        /// 获取被动核心Buff
        /// </summary>
        /// <returns></returns>
        public int GetPassiveBuff()
        {
            return 100;
        }
    }
}
