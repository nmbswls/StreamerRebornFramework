using My.Framework.Runtime.Saving;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StreamerReborn.Saving
{
    /// <summary>
    /// 主存档 数据
    /// </summary>
    public class SavingDataMain : SavingData
    {
        public int Level;
        public int Day;
        public int MainName;
    }

    /// <summary>
    /// 主存档 实例
    /// </summary>
    public class SavingUnitMain : SavingUnitBase<SavingDataMain>
    {
        #region override

        public override string SavingUnitName => "Main";

        /// <summary>
        /// 初始化
        /// </summary>
        public override void InitEmpty()
        {
            var data = new SavingDataSummary();
            data.m_savingName = "Unname";
            m_savingData = data;
        }

        /// <summary>
        /// 触发回调
        /// </summary>
        protected override void OnReconstruct()
        {
            Debug.LogError($"Load finish data: dat {SavingData.Day} name {SavingData.MainName}");
        }

        #endregion
    }

}

