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
    /// �浵
    /// </summary>
    public class SavingManagerRB : SavingManagerBase
    {

        /// <summary>
        /// ע��洢��Ԫ
        /// </summary>
        protected override void RegisterSavingUnit()
        {
            base.RegisterSavingUnit();
            m_savingUnitRegEntryDict["Main"] = new SavingUnitRegEntry() { UnitName = "Main", UnitType = typeof(SavingUnitSummary), IsOpen = true };
        }

        #region ��Ϣ



        #endregion
    }

}

