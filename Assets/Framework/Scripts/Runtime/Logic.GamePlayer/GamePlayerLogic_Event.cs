using System;
using My.Framework.Battle;
using My.Framework.Runtime.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Logic
{
    
    public partial class GamePlayerLogic
    {
        public void InitEvents()
        {
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="action"></param>
        public void RegisterEventOnBattleCtxOpen(Action<GamePlayerCompBattle.BattleCtx> action)
        {
            m_compBattle.EventOnBattleCtxOpen += action;
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterEventOnBattleCtxOpen(Action<GamePlayerCompBattle.BattleCtx> action)
        {
            m_compBattle.EventOnBattleCtxOpen -= action;
        }

        #region 事件列表



        #endregion

    }
}

