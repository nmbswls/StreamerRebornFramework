
using My.ConfigData;
using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Saving;
using My.Framework.Runtime.Storytelling;
using My.Framework.Runtime.UI;
using StreamerReborn.Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using My.Framework.Battle.View;
using My.Framework.Runtime.Logic;
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

            // tick
            m_gameWorld.Tick();
        }


        /// <summary>
        /// 创建存档管理器
        /// </summary>
        /// <returns></returns>
        protected override SavingManager CreateSavingManager()
        {
            return new SavingManagerRB();
        }

        /// <summary>
        /// 创建ConfigDataLoader
        /// </summary>
        /// <returns></returns>
        protected override ConfigDataLoaderBase CreateConfigDataLoader()
        {
            return new ConfigDataLoader();
        }


        protected override GamePlayer CreateGamePlayer()
        {
            return new GamePlayer();
        }

        /// <summary>
        /// 创建存档管理器
        /// </summary>
        /// <returns></returns>
        protected override StorytellingSystemBase CreateStorytellingSystem()
        {
            return new MyStorytellingSystem();
        }

        /// <summary>
        /// 根据战斗信息创建对应类型的battlemanager
        /// </summary>
        /// <param name="battleInfo"></param>
        /// <returns></returns>
        protected override BattleManagerBase CreateBattleManager(int battleInfo)
        {
            return new BattleManagerTorture();
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

            m_gameWorld = new GameWorldBase();
            m_gameWorld.Init();
            return true;
        }

        

        #region 游戏进程控制

        public void EnterBattle()
        {
            StartCoroutine(EnterBattleImpl());
        }

        protected IEnumerator EnterBattleImpl()
        {
            bool? ret = null;

            //yield return ProcessManager.EnterBattleCoroutine(
            //    () => {
            //        BattleManager.Instance.BattleStart();
            //    }
            //);
            yield break;
        }

        #endregion
    }
}

