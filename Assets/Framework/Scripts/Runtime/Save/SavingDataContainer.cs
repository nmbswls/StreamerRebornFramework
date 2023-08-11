//using Newtonsoft.Json;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using My.Framework.Runtime.Logic;
//using UnityEngine;

//namespace My.Framework.Runtime.Saving
//{

//    /// <summary>
//    /// 存储单元
//    /// </summary>
//    public abstract class SavingDataContainer : IDataContainerBase
//    {
//        /// <summary>
//        /// 初始化
//        /// </summary>
//        /// <param name="savingData"></param>
//        /// <returns></returns>
//        public bool Initialize(SavingData savingData)
//        {
//            m_savingData = savingData;
//            return true;
//        }

//        /// <summary>
//        /// 获取data bytes
//        /// </summary>
//        /// <returns></returns>
//        public SavingData GetSavingData()
//        {
//            return m_savingData;
//        }

//        /// <summary>
//        /// 持有数据
//        /// </summary>
//        protected SavingData m_savingData;
//    }

//}

