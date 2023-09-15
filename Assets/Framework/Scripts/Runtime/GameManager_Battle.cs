using My.Framework.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.View;
using My.Framework.Runtime.UI;

namespace My.Framework.Runtime
{
    public partial class GameManager
    {

        /// <summary>
        /// 开始战斗 准备battle manager
        /// </summary>
        public void LaunchBattle()
        {
            BattleManager.CreateInstance();
            BattleManager.Instance.StartBattle(1);
            BattleManager.Instance.EventOnBattleEnd += OnBattleEnd;

            m_isInBattle = true;
        }

        /// <summary>
        /// 处理战斗结束
        /// </summary>
        public void OnBattleEnd()
        {
            BattleManager.DestroyInstance();
            //HandleReturn();
            UIControllerLoading.ShowLoadingUI(1, "nmsl", () => {
                GameManager.Instance.GameWorld.EnterHall();
            });
            m_isInBattle = false;
        }

        /// <summary>
        /// 战斗标记位
        /// </summary>
        protected bool m_isInBattle;
    }
}
