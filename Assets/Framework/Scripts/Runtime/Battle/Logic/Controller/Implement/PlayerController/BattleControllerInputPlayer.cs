using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
{
    
    public interface IBattleControllerInputPlayer : IBattleControllerInput
    {
    }

    public class BattleControllerInputPlayer : BattleControllerInput, IBattleControllerInputPlayer
    {

        public BattleControllerPlayer OwnerPlayer
        { get { return (BattleControllerPlayer)Owner; } }


        #region 指令执行具体类

        /// <summary>
        /// 执行 - 释放技能
        /// </summary>
        /// <param name="opt"></param>
        protected override bool ExecSkillCast(BattleOpt opt)
        {
            var mainActor = OwnerPlayer.MainPlayerActor;
            mainActor.UseSkill(0);
            return true;
        }

        
        #endregion

        /// <summary>
        /// 是否可以输入角色操作指令
        /// </summary>
        protected override bool CanInputCmd()
        {
            if(!base.CanInputCmd())
            {
                return false;
            }
            return true;
        }
    }
}
