//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace My.Framework.Runtime.Saving
//{
//    /// <summary>
//    /// �洢����ʵ��
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
//    /// �洢��Ԫʵ��
//    /// </summary>
//    public class SavingDataContainerSummary : SavingDataContainer
//    {
//        public SavingDataSummary SavingData
//        {
//            get { return (SavingDataSummary)m_savingData; }
//        }
//    }
//}

