using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    
    public interface IBattleControllerInput
    {
        /// <summary>
        /// Push操作指令
        /// </summary>
        /// <param name="cmd"></param>
        bool CheckOptInput(BattleOpt opt);

        /// <summary>
        /// 执行操作指令
        /// </summary>
        /// <param name="cmd"></param>
        bool ExecOptCmd(BattleOpt opt);
    }

    public class BattleControllerInput : IBattleControllerInput
    {
        public BattleControllerInput(BattleController battleController)
        {
            m_battleController = battleController;
        }

        /// <summary>
        /// 检查操作指令
        /// </summary>
        /// <param name="cmd"></param>
        public bool CheckOptInput(BattleOpt opt)
        {
            // 检查是否
            if (!CanInputCmd(opt))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 执行操作指令
        /// </summary>
        /// <param name="cmd"></param>
        public virtual bool ExecOptCmd(BattleOpt opt)
        {
            // 1. 执行各种指令
            switch (opt.m_type)
            {
                case BattleOptType.SkillCast:
                    return ExecSkillCast(opt);
                case BattleOptType.EndTurn:
                    return ExecEndTurn(opt);
                default:
                    return false;
            }
        }

        #region 指令执行具体类

        /// <summary>
        /// 执行 - 释放技能
        /// </summary>
        /// <param name="opt"></param>
        protected virtual bool ExecSkillCast(BattleOpt opt)
        {
            return true;
        }

        /// <summary>
        /// 执行 - 结束回合
        /// </summary>
        /// <param name="opt"></param>
        protected virtual bool ExecEndTurn(BattleOpt opt)
        {
            m_battleController.DoTurnEnd();
            return true;
        }

        #endregion

        /// <summary>
        /// 是否可以输入角色操作指令
        /// </summary>
        protected virtual bool CanInputCmd(BattleOpt opt)
        {
            var currCtrl = m_battleController.BattleLogic.CurrTurnActionController();
            if (currCtrl == null || currCtrl != m_battleController)
            {
                return false;
            }

            // 只有等待阶段才可输入
            if (currCtrl.CurrState != BattleController.TurnActionState.WaitOptCmdInput)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 控制器基类
        /// </summary>
        protected BattleController m_battleController;
    }
}
