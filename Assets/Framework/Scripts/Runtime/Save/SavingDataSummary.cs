using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{
    /// <summary>
    /// �洢����ʵ��
    /// </summary>
    public class SavingDataSummary : SavingData
    {
        public string m_savingName;
    }

    /// <summary>
    /// �洢��Ԫʵ��
    /// </summary>
    public class SavingUnitSummary : SavingUnitBase<SavingDataSummary>
    {
        #region override

        public override string SavingUnitName => "Summary";

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public override void InitEmpty()
        {
            var data = new SavingDataSummary();
            data.m_savingName = "Unname";
            m_savingData = data;
        }

        #endregion

        #region Data

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <returns></returns>
        public string GetBasicInfoName()
        {
            return SavingData.m_savingName;
        }

        #endregion
    }
}

