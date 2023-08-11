using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Logic;

namespace My.Framework.Runtime.Saving
{

    /// <summary>
    /// 存档数据 聚合体
    /// 底层存储可以是单文件也可以多文件无所谓
    /// </summary>
    public class SavingData : IDataSourceBase
    {
        public string BattleData;
    }
}
