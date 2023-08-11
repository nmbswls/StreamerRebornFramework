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
        /// ע���¼�
        /// </summary>
        /// <param name="action"></param>
        public void RegisterEventOnBattleCtxOpen(Action<GamePlayerCompBattle.BattleCtx> action)
        {
            m_compBattle.EventOnBattleCtxOpen += action;
        }

        /// <summary>
        /// ��ע���¼�
        /// </summary>
        /// <param name="action"></param>
        public void UnRegisterEventOnBattleCtxOpen(Action<GamePlayerCompBattle.BattleCtx> action)
        {
            m_compBattle.EventOnBattleCtxOpen -= action;
        }

        #region �¼��б�



        #endregion

    }
}

