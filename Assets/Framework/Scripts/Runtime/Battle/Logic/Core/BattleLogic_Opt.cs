using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{
    public partial class BattleLogic
    {

        public void OnEventControllerWait4Input()
        {

        }

        /// <summary>
        /// 推入指令
        /// </summary>
        public virtual bool InputOpt(BattleOpt opt)
        {
            var targetCtrl = GetBattleControllerById(opt.m_controllerId);
            if(targetCtrl == null)
            {
                Debug.LogError("opt source not found");
                return false;
            }
            
            if(!targetCtrl.CheckOptInput(opt))
            {
                Debug.LogError("opt CheckOptInput fail");
                return false;
            }

            // 执行指定的cmd
            if(!targetCtrl.ExecOptCmd(opt))
            {
                Debug.LogError("opt ExecOptCmd fail");
                return false;
            }
            EventOnOptCmdInput?.Invoke(opt);
            return true;
        }


        /// <summary>
        /// 推入命令
        /// 可能来自输入或重放组件
        /// </summary>
        public event Action<BattleOpt> EventOnOptCmdInput;

    }
}
