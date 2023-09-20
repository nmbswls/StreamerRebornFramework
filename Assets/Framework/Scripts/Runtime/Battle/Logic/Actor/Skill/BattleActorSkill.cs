using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Battle.Actor
{
    public class BattleActorSkill
    {
        /// <summary>
        /// 技能id
        /// </summary>
        public int SkillId;

        /// <summary>
        /// 技能状态
        /// </summary>
        public float CoolDown;

        /// <summary>
        /// 是否是被动技能
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPassive()
        {
            return false;
        }

        public virtual void OnSkillPreCast(IBattleLogicResolver resolver)
        {
            // 施加pre的各种效果
            var ctx = resolver.OpenResolveCtx(EnumTriggereSourceType.BeforeCast);
        }

        /// <summary>
        /// 释放开始
        /// </summary>
        public virtual void OnSkillCast(IBattleLogicResolver resolver)
        {
            var ctx = resolver.OpenResolveCtx(EnumTriggereSourceType.OnCast);

            // 显示特效
            ctx.AddEffectNode(new EffectNodeShow());
        }

        public virtual void OnSkillPostCast(IBattleLogicResolver resolver)
        {
            var ctx = resolver.OpenResolveCtx(EnumTriggereSourceType.PostCast);
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
