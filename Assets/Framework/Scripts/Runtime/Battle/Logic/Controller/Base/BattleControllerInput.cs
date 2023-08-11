using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
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
        public BattleController Owner;

        /// <summary>
        /// Push操作指令
        /// </summary>
        /// <param name="cmd"></param>
        public bool CheckOptInput(BattleOpt opt)
        {
            // 检查是否
            if (!CanInputCmd())
            {
                return false;
            }

            // 分配操作数据
            opt.m_seq = Owner.m_env.OptSeqAlloc();

            Owner.m_env.OptPush(opt);
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
            Owner.DoTurnEnd();
            return true;
        }

        #endregion

        /// <summary>
        /// 是否可以输入角色操作指令
        /// </summary>
        protected virtual bool CanInputCmd()
        {
            //var currCtrl = Owner.Owner.CurrTurnActionController();
            //if (currCtrl == null || currCtrl != Owner)
            //{
            //    return false;
            //}
            return true;
        }
    }
}
