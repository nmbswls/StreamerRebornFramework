using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    
    public interface IBattleControllerInputPlayer : IBattleControllerInput
    {
    }

    public class BattleControllerInputPlayer : BattleControllerInput, IBattleControllerInputPlayer
    {

        public BattleControllerInputPlayer(BattleController battleController):base(battleController)
        {
        }


        public BattleControllerPlayer ControllerPlayer
        { get { return (BattleControllerPlayer)m_battleController; } }


        #region 指令执行具体类

        

        /// <summary>
        /// 执行 - 释放技能
        /// </summary>
        /// <param name="opt"></param>
        protected override bool ExecSkillCast(BattleOpt opt)
        {
            var mainActor = ControllerPlayer.MainPlayerActor;
            mainActor.UseSkill(0);
            return true;
        }

        #endregion

        /// <summary>
        /// 是否可以输入角色操作指令
        /// </summary>
        protected override bool CanInputCmd(BattleOpt opt)
        {
            if(!base.CanInputCmd(opt))
            {
                return false;
            }

            // 正在使用技能时 无法操作
            var playerActor = ControllerPlayer.MainPlayerActor;
            if (playerActor.IsAnySkillflowRunninging())
            {
                return false;
            }

            switch (opt.m_type)
            {
                case BattleOptType.SkillCast:
                {
                    // 检查是否有阻止施法的状态
                    //if(playerActor.)
                    break;
                }
            }

            return true;
        }
    }
}
