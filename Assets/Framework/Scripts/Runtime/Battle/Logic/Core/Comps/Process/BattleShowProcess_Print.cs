using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// 打印日志 
    /// </summary>
    public class BattleShowProcess_Print : BattleShowProcess
    {
        /// <summary>
        /// 目标actor id
        /// </summary>
        protected string m_printContent;

        public BattleShowProcess_Print(string printContent) : base()
        {
            m_printContent = printContent;
        }

        public override int Type
        {
            get { return ProcessTypes.Print; }
        }
    }
}
