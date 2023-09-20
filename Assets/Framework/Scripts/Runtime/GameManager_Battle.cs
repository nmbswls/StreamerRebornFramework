using My.Framework.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.View;
using My.Framework.Runtime.UI;
using Unity.VisualScripting;

namespace My.Framework.Runtime
{
    public partial class GameManager
    {

        public BattleManagerBase BattleManager;

        /// <summary>
        /// 开始战斗 准备battle manager
        /// </summary>
        public void LaunchBattle(int battleInfo)
        {
            m_gameWorld.Pause();

            BattleManager = CreateBattleManager(battleInfo);
            BattleManager.Init();
            BattleManager.StartBattle(1);
            BattleManager.EventOnBattleEnd += OnBattleEnd;

            m_isInBattle = true;
        }

        /// <summary>
        /// 处理战斗结束
        /// </summary>
        public void OnBattleEnd()
        {
            BattleManager.UnInit();
            //HandleReturn();
            UIControllerLoading.ShowLoadingUI(1, "nmsl", () => {
                GameManager.Instance.GameWorld.EnterHall();
            });

            m_gameWorld.Resume();
            BattleManager = null;
            m_isInBattle = false;
        }

        /// <summary>
        /// 根据战斗信息创建对应类型的battlemanager
        /// </summary>
        /// <param name="battleInfo"></param>
        /// <returns></returns>
        protected virtual BattleManagerBase CreateBattleManager(int battleInfo)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 战斗标记位
        /// </summary>
        protected bool m_isInBattle;
    }
}
