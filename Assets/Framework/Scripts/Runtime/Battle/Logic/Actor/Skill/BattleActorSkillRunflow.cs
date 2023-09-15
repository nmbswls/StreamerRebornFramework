using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Battle.Actor
{
    
    public class BattleActorSkillRunflow
    {
        public enum PhaseStep
        {
            Init,
            PreCast,
            Cast,
            PostCast,
            Finish,
        }

        public BattleActorHandlerSkill Owner;
        public BattleActorSkill InnerSkill;

        public PhaseStep Step;

        /// <summary>
        /// 技能结束
        /// </summary>
        public event Action EventOnSkillOver;

        /// <summary>
        /// 开始tick一次执行
        /// </summary>
        public void Start()
        {
            // 更改执行状态
            NextStep();
        }

        public void Tick()
        {
            switch (Step)
            {
                case PhaseStep.PreCast:
                case PhaseStep.Cast:
                case PhaseStep.PostCast:
                {
                    // 检查现场是否运行完毕
                    if (Owner.GetResolver().CurrContext == null)
                    {
                        NextStep();
                    }
                }
                    break;
            }
            
        }

        /// <summary>
        /// 进入下一步
        /// </summary>
        public void NextStep()
        {
            Step = (PhaseStep)(Step + 1);

            switch (Step)
            {
                case PhaseStep.Init:
                {
                    InnerSkill.OnSkillPreCast(Owner.GetResolver());
                }
                    break;
                case PhaseStep.PreCast:
                {
                    InnerSkill.OnSkillCast(Owner.GetResolver());
                }
                    break;
                case PhaseStep.Cast:
                {
                    InnerSkill.OnSkillPostCast(Owner.GetResolver());
                }
                    break;
                case PhaseStep.PostCast:
                {
                    EventOnSkillOver?.Invoke();
                }
                    break;
            }
        }
    }
}
