using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 战斗相关数据
    /// </summary>
    public class BattleDataBlock : DataBlock
    {
        public int CachedBattleId;
        public int CachedBattleParam1;
    }

    public interface IDataContainerBattle : IDataContainerBase
    {
        /// <summary>
        /// 创建战斗持久化
        /// </summary>
        void BattlePersistentInfoCreate(int battleId);

        /// <summary>
        /// 移除战斗持久化数据
        /// </summary>
        void BattleCtxPersistentInfoRemove();
    }

    /// <summary>
    /// 数据容器 - 战斗
    /// </summary>
    public class DataContainer4Battle : DataContainerBase, IDataContainerBattle
    {

        public override string Name
        {
            get
            {
                return nameof(DataContainer4Battle);
            }
        }

        /// <summary>
        /// 创建持久化shuju
        /// </summary>
        /// <param name="battleId"></param>
        public void BattlePersistentInfoCreate(int battleId)
        {
            m_dataBlock.CachedBattleId = battleId;
        }

        public void BattleCtxPersistentInfoRemove()
        {
            m_dataBlock.CachedBattleId = 0;
        }

        public BattleDataBlock m_dataBlock;
    }

    

}
