using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.Saving;
using Newtonsoft.Json;

namespace My.Framework.Runtime.Logic
{

    /// <summary>
    /// 数据管理
    /// </summary>
    public class DataManagerBase : IDataContainerHolder
    {
        /// <summary>
        /// 从文件数据中重建
        /// </summary>
        /// <param name="dataSource"></param>
        public virtual void RestructFromSaving(IDataSourceBase dataSource)
        {
            m_dcBattle = new DataContainer4Battle();
            m_dcBattle.InitData(dataSource);

            m_dcWorld = new DataContainer4World();
            m_dcWorld.InitData(dataSource);
        }

        #region 提供数据
        public virtual IDataContainerBattle DataContainerBattleGet()
        {
            return m_dcBattle;
        }

        public virtual IDataContainer4World DataContainerWorldGet()
        {
            return m_dcWorld;
        }

        #endregion


        #region dc

        /// <summary>
        /// 
        /// </summary>
        protected DataContainer4Battle m_dcBattle;

        /// <summary>
        /// 
        /// </summary>
        protected DataContainer4World m_dcWorld;


        #endregion

    }
}
