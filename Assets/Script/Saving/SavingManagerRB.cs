using My.Framework.Runtime.Saving;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StreamerReborn.Saving
{
    /// <summary>
    /// 存档
    /// </summary>
    public class SavingManagerRB : SavingManagerBase
    {

        /// <summary>
        /// 注册存储单元
        /// </summary>
        protected override void RegisterSavingUnit()
        {
            base.RegisterSavingUnit();
            m_savingUnitRegEntryDict["Main"] = new SavingUnitRegEntry() { UnitName = "Main", UnitType = typeof(SavingUnitSummary), IsOpen = true };
        }

        #region 信息



        #endregion
    }

}

