using My.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle.View
{
    /// <summary>
    /// 战斗管理器
    /// </summary>
    public  class BattleManagerTorture : BattleManagerBase
    {

        public static BattleManagerTorture Instance
        {
            get
            {
                return (BattleManagerTorture)GameManager.Instance.BattleManager;
            }
        }

        #region 供子类重写

        /// <summary>
        /// 创建监听组件
        /// </summary>
        /// <returns></returns>
        protected override BattleSceneManagerBase CreateBattleSceneManager()
        {
            return new BattleSceneManagerTorture();
        }


        /// <summary>
        /// 注册事件
        /// </summary>
        protected override void RegisterListener()
        {
            base.RegisterListener();
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        protected virtual void UnregisterListener()
        {
            base.UnregisterListener();
        }


        #endregion

    }
}
