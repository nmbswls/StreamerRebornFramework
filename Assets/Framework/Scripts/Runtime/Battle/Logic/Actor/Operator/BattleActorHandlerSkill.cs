using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Rendering;

namespace My.Framework.Battle.Actor
{

    public interface IBattleActorHandlerSkill
    {
        /// <summary>
        /// 是否能使用技能
        /// </summary>
        /// <returns></returns>
        bool CanUseSkill(int skillId);

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <returns></returns>
        bool UseSkill(int skillId);
    }


    public class BattleActorHandlerSkill : BattleActorHandlerBase, IBattleActorHandlerSkill
    {
        public BattleActorHandlerSkill(IBattleActorOperatorEnv env) : base(env)
        {
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            m_compSkill = m_env.BattleActorCompSkillGet();

            m_handlerBuff = m_env.BattleActorHandlerBuffGet();
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            foreach (var skill in m_compSkill.SkillList)
            {
                if (skill.IsPassive())
                {
                    AddPassiveSkill((BattleActorSkillPassive)skill);
                }
            }
            return true;
        }


        #endregion

        #region 对外方法

        /// <summary>
        /// 是否能使用技能
        /// </summary>
        /// <returns></returns>
        public bool CanUseSkill(int skillId)
        {
            return true;
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <returns></returns>
        public bool UseSkill(int skillId)
        {
            return true;
        }

        #endregion


        #region 内部方法

        /// <summary>
        /// 增加被动技能
        /// </summary>
        /// <param name="skill"></param>
        protected void AddPassiveSkill(BattleActorSkillPassive skill)
        {
            int buff = skill.GetPassiveBuff();
            m_handlerBuff.AddBuff(buff, 1);
        }

        #endregion

        #region 事件

        

        #endregion

        #region 引用component

        protected BattleActorCompSkill m_compSkill;

        #endregion

        #region 引用Handler

        protected BattleActorHandlerBuff m_handlerBuff;

        #endregion

        
    }
}
