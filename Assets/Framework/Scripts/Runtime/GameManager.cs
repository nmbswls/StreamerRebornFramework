
using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Saving;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime
{
    

    /// <summary>
    /// 最小GameManager
    /// </summary>
    public abstract partial class GameManager
    {

        #region 单例
        public static GameManager Instance { get { return m_instance; } }
        private static GameManager m_instance;

        public GameManager()
        {
        }

        /// <summary>
        /// 创建游戏管理器
        /// </summary>
        /// <returns></returns>

        public static T CreateAndInitGameManager<T>()
            where T : GameManager, new()
        {
            if (m_instance != null)
            {
                return m_instance as T;
            }
            m_instance = new T();
            // 初始化
            if (!m_instance.Initlize())
            {
                UnityEngine.Debug.LogError("CreateAndInitGameManager fail");
                return null;
            }
            return (T)m_instance;
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>

        public virtual bool Initlize()
        {
            UnityEngine.Debug.Log("GameManager.Initlize start");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;


            // 初始化帧率
            //Application.targetFrameRate = 90;

            // Disable screen dimming  
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 初始化本地化信息
            InitLocalizationInfo();

            // 初始化ResourceManager
            m_resourceManager = SimpleResourceManager.CreateResourceManager();
            if (!m_resourceManager.Initlize(m_currLocalization))
            {
                Debug.LogError("GameManager.Initlize fail for m_resourceManager.Initlize()");
                return false;
            }

            // 初始化UIManager
            m_uiManager = UIManager.CreateUIManager();
            if (!m_uiManager.Initialize())
            {
                Debug.LogError("GameManager.Initlize fail for m_uiManager.Initlize()");
                return false;
            }

            // 创建config
            m_savingManager = CreateSavingManager();
            m_savingManager.Initialize();

            // 配置管理器
            m_configDataLoader = CreateConfigDataLoader();

            // 创建玩家类
            m_gamePlayer = CreateGamePlayer();

            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        public virtual void Tick()
        {
            m_corutineWrapper.Tick();

            m_resourceManager.Tick();
            m_uiManager.Tick(Time.deltaTime);
            m_savingManager.Tick();
        }

        /// <summary>
        /// 启动携程
        /// </summary>
        /// <param name="corutine"></param>
        public void StartCoroutine(IEnumerator corutine)
        {
            m_corutineWrapper.StartCoroutine(corutine);
        }

        protected void InitLocalizationInfo()
        {
            m_currLocalization = "CN";
            PlayerPrefs.SetString("Localization", m_currLocalization);
            PlayerPrefs.Save();
        }

        #region 装配组件
        /// <summary>
        /// 创建存档管理器
        /// </summary>
        /// <returns></returns>
        protected virtual SavingManagerBase CreateSavingManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建ConfigDataLoader
        /// </summary>
        /// <returns></returns>
        protected virtual ConfigDataLoaderBase CreateConfigDataLoader()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建GamePlayer
        /// </summary>
        /// <returns></returns>
        protected virtual GamePlayerBase CreateGamePlayer()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Exception Debug提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.LogError(string.Format("OnUnhandledException  {0} {1}", sender, e));
        }

        #region 内部变量

        /// <summary>
        /// 资源管理器
        /// </summary>
        public SimpleResourceManager ResourceManager { get { return m_resourceManager; } }
        protected SimpleResourceManager m_resourceManager;

        /// <summary>
        /// 配置数据加载器
        /// </summary>
        public ConfigDataLoaderBase ConfigDataLoader { get { return m_configDataLoader; } }
        protected ConfigDataLoaderBase m_configDataLoader;

        /// <summary>
        /// UI管理器
        /// </summary>
        public UIManager SimpleUIManager { get { return m_uiManager; } }
        protected UIManager m_uiManager;

        /// <summary>
        /// 数据类
        /// </summary>
        public SavingManagerBase SavingManager { get { return m_savingManager; } }
        protected SavingManagerBase m_savingManager;

        /// <summary>
        /// 玩家功能类
        /// </summary>
        public GamePlayerBase GamePlayer { get { return m_gamePlayer; } }
        protected GamePlayerBase m_gamePlayer;

        /// <summary>
        /// corutine管理
        /// </summary>
        protected SimpleCoroutineWrapper m_corutineWrapper = new SimpleCoroutineWrapper();

        #endregion

        /// <summary>
        /// 设置本地化语言
        /// </summary>
        /// <param name="localization"></param>
        public void SetLocalization(string localization)
        {
            if(localization == m_currLocalization)
            {
                return;
            }

        }

        /// <summary>
        /// 多语言
        /// </summary>
        public string m_currLocalization;

    }
    
}

