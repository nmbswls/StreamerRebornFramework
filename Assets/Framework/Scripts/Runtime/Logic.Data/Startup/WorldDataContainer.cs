using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 世界相关数据
    /// </summary>
    public class WorldDataBlock : DataBlock
    {
        public int CachedBattleId;
        public int CachedBattleParam1;
    }

    /// <summary>
    /// 数据容器
    /// </summary>
    public interface IDataContainer4World : IDataContainerBase
    {

    }

    /// <summary>
    /// 数据容器 - 世界
    /// </summary>
    public class DataContainer4World : DataContainerBase, IDataContainer4World
    {

        public override string Name
        {
            get
            {
                return nameof(DataContainer4World);
            }
        }

        public WorldDataBlock m_dataBlock;
    }

    

}
