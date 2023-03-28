
using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Saving;
using My.Framework.Runtime.UI;
using StreamerReborn.Config;
using StreamerReborn.Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public static class GameStatic
    {

        public static UIManager UIManager
        {
            get { return (GameManager.Instance.SimpleUIManager); }
        }

        public static ConfigDataLoader ConfigDataLoader
        {
            get { return (GameManager.Instance.ConfigDataLoader) as ConfigDataLoader; }
        }

        public static SimpleResourceManager ResourceManager
        {
            get { return GameManager.Instance.ResourceManager; }
        }

        public static GamePlayer GamePlayer
        {
            get { return (GameManager.Instance.GamePlayer) as GamePlayer; }
        }

        public static MyGameManager MyGameManager
        {
            get { return (GameManager.Instance) as MyGameManager; }
        }
    }


    public class MyGameManager : GameManager
    {

        /// <summary>
        /// tick
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            m_processManager.Tick(Time.deltaTime);
        }


        /// <summary>
        /// 创建存档管理器
        /// </summary>
        /// <returns></returns>
        protected override SavingManagerBase CreateSavingManager()
        {
            return new SavingManager();
        }

        /// <summary>
        /// 创建ConfigDataLoader
        /// </summary>
        /// <returns></returns>
        protected override ConfigDataLoaderBase CreateConfigDataLoader()
        {
            return new ConfigDataLoader();
        }


        protected override GamePlayerBase CreateGamePlayer()
        {
            return new GamePlayer();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>

        public override bool Initlize()
        {
            if(!base.Initlize())
            {
                return false;
            }
            

            m_processManager = new GameProcessManager();
            return true;
        }

        /// <summary>
        /// 场景切换
        /// </summary>
        public GameProcessManager ProcessManager { get { return m_processManager; } }
        protected GameProcessManager m_processManager;

        #region 游戏进程控制

        public void EnterBattle()
        {
            StartCoroutine(EnterBattleImpl());
        }

        protected IEnumerator EnterBattleImpl()
        {
            bool? ret = null;

            yield return ProcessManager.EnterBattleCoroutine(
                () => {
                    BattleManager.Instance.BattleStart();
                }
            );
        }

        #endregion
    }
}

