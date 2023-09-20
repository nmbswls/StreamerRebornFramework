using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public interface IBattleControllerAI
    {

    }
    public class BattleControllerAI : IBattleControllerAI
    {
        public BattleController OwnerController;

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="currTime"></param>
        public virtual void Tick()
        {
            // 1. 判断当前的状态是否等待输入
            if (!CanAIInput())
            {
                return;
            }

            AIActionExec();
        }

        /// <summary>
        /// AI是否能输入
        /// </summary>
        /// <returns></returns>
        public bool CanAIInput()
        {
            // 1. 判断当前的状态是否等待输入
            if (OwnerController.CurrState != BattleController.TurnActionState.WaitOptCmdInput)
            {
                return false;
            }

            // 2. 不允许AI
            if (!IsAIEnable())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ai是否生效
        /// Monster会重写这个方法
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsAIEnable()
        {
            if(OwnerController.FlagAuto)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 默认AI行为执
        /// </summary>
        protected virtual void AIActionExec()
        {
            BattleOpt battleOptCmd;
        }
    }
}
