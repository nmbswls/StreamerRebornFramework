//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace My.Framework.Runtime.Saving
//{
//    /// <summary>
//    /// 存储数据实例
//    /// </summary>
//    public class SavingDataSummary : SavingData
//    {

//        public string m_savingName;

//        public override string SavingDataName
//        {
//            get { return "Summary"; }
//        }
//        public override void InitEmpty()
//        {
//            m_savingName = "New New";
//        }
//    }

//    /// <summary>
//    /// 存储单元实例
//    /// </summary>
//    public class SavingDataContainerSummary : SavingDataContainer
//    {
//        public SavingDataSummary SavingData
//        {
//            get { return (SavingDataSummary)m_savingData; }
//        }
//    }
//}

