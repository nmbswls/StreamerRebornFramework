using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{
    /// <summary>
    /// 存储数据实例
    /// </summary>
    public class SavingDataSummary : SavingData
    {
        public string m_savingName;
    }

    /// <summary>
    /// 存储单元实例
    /// </summary>
    public class SavingUnitSummary : SavingUnitBase<SavingDataSummary>
    {
        #region override

        public override string SavingUnitName => "Summary";

        /// <summary>
        /// 初始化
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
        /// 获取基本类型
        /// </summary>
        /// <returns></returns>
        public string GetBasicInfoName()
        {
            return SavingData.m_savingName;
        }

        #endregion
    }
}

