using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataContainerHolder
    {
        /// <summary>
        /// 获取战斗dc
        /// </summary>
        /// <returns></returns>
        IDataContainerBattle DataContainerBattleGet();

        /// <summary>
        /// 获取战斗dc
        /// </summary>
        /// <returns></returns>
        IDataContainer4World DataContainerWorldGet();
    }
}
