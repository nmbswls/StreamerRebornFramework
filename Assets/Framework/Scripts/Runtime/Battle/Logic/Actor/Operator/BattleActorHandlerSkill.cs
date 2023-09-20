using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;
using Unity.VisualScripting;
using UnityEngine;
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

        /// <summary>
        /// 是否正在使用技能
        /// </summary>
        /// <returns></returns>
        bool IsAnySkillflowRunninging();
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
            // todo 添加测试技能
            m_compSkill.SkillList.Add(new BattleActorSkill());

            foreach (var skill in m_compSkill.SkillList)
            {
                if (skill.IsPassive())
                {
                    AddPassiveSkill((BattleActorSkillPassive)skill);
                }
            }
            return true;
        }

        #region Overrides of BattleActorHandlerBase

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            foreach (var runflow in m_runningSkillRunflowList)
            {
                runflow.Tick();
            }

            // 清理执行完毕的现场
            for (int i = m_runningSkillRunflowList.Count - 1; i >= 0; i--)
            {
                m_runningSkillRunflowList[i].Tick();
                if (m_runningSkillRunflowList[i].Step == BattleActorSkillRunflow.PhaseStep.Finish)
                {
                    m_runningSkillRunflowList.RemoveAt(i);
                }
            }
        }

        #endregion

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
            Debug.Log($"UseSkill {skillId}");
            
            // 二次校验是否可以使用
            if (!CanUseSkill(skillId))
            {
                return false;
            }
            
            // 获取skill
            var runflow = new BattleActorSkillRunflow(m_compSkill.SkillList[skillId], this);

            runflow.Start();
            m_runningSkillRunflowList.Add(runflow);

            return true;
        }

        /// <summary>
        /// 是否正在使用技能
        /// </summary>
        /// <returns></returns>
        public bool IsAnySkillflowRunninging()
        {
            return m_runningSkillRunflowList.Count != 0;
        }

        /// <summary>
        /// 获取resolver
        /// </summary>
        /// <returns></returns>
        public IBattleLogicResolver GetResolver()
        {
            return m_env.GetResolver();
        }

        /// <summary>
        /// 获取拥有者
        /// </summary>
        /// <returns></returns>
        public IBattleActor GetActor()
        {
            return m_env as IBattleActor;
        }

        #endregion


        #region 内部方法

        /// <summary>
        /// 增加被动技能
        /// </summary>
        /// <param name="skill"></param>
        protected void AddPassiveSkill(BattleActorSkillPassive skill)
        {
            int buffId = skill.GetPassiveBuff();
            m_handlerBuff.AddBuff( buffId, 1);
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

        #region 内部变量

        /// <summary>
        /// 正在进行的技能执行流
        /// 回合制应当只有一个
        /// </summary>
        protected List<BattleActorSkillRunflow> m_runningSkillRunflowList = new List<BattleActorSkillRunflow>();

        #endregion

    }
}
