using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 基础数据源
    /// </summary>
    public interface IDataSourceSimple : IDataSourceBase
    {
        BattleDataBlock GetBattleDataBlock();
    }
    
}
