
using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Saving;
using My.Framework.Runtime.Scene;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime
{
    

    /// <summary>
    /// ��СGameManager
    /// </summary>
    public abstract partial class GameManager
    {

        #region ����
        public static GameManager Instance { get { return m_instance; } }
        private static GameManager m_instance;

        public GameManager()
        {
        }

        /// <summary>
        /// ������Ϸ������
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
            // ��ʼ��
            if (!m_instance.Initlize())
            {
                UnityEngine.Debug.LogError("CreateAndInitGameManager fail");
                return null;
            }
            return (T)m_instance;
        }
        #endregion

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <returns></returns>

        public virtual bool Initlize()
        {
            UnityEngine.Debug.Log("GameManager.Initlize start");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;


            // ��ʼ��֡��
            //Application.targetFrameRate = 90;

            // Disable screen dimming  
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // ��ʼ�����ػ���Ϣ
            InitLocalizationInfo();

            // ��ʼ��ResourceManager
            m_resourceManager = SimpleResourceManager.CreateResourceManager();
            if (!m_resourceManager.Initlize(m_currLocalization))
            {
                Debug.LogError("GameManager.Initlize fail for m_resourceManager.Initlize()");
                return false;
            }

            // ��ʼ��SceneManager
            m_sceneLayerManager = SceneLayerManager.CreateSceneManager();
            if (!m_sceneLayerManager.Initialize())
            {
                Debug.LogError("GameManager.Initlize fail for m_sceneLayerManager.Initlize()");
                return false;
            }

            // ��ʼ��UIManager
            m_uiManager = UIManager.CreateUIManager();
            if (!m_uiManager.Initialize())
            {
                Debug.LogError("GameManager.Initlize fail for m_uiManager.Initlize()");
                return false;
            }

            // ����config
            m_savingManager = CreateSavingManager();
            m_savingManager.Initialize();

            // ���ù�����
            m_configDataLoader = CreateConfigDataLoader();

            // ���������
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
            m_sceneLayerManager.Tick();
            m_uiManager.Tick(Time.deltaTime);


        }

        /// <summary>
        /// ����Я��
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


        #region װ�����
        /// <summary>
        /// �����浵������
        /// </summary>
        /// <returns></returns>
        protected virtual SavingManagerBase CreateSavingManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ����ConfigDataLoader
        /// </summary>
        /// <returns></returns>
        protected virtual ConfigDataLoaderBase CreateConfigDataLoader()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ����GamePlayer
        /// </summary>
        /// <returns></returns>
        protected virtual GamePlayerBase CreateGamePlayer()
        {
            throw new NotImplementedException();
        }

        #endregion




        /// <summary>
        /// Exception Debug��ʾ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.LogError(string.Format("OnUnhandledException  {0} {1}", sender, e));
        }

        #region �ڲ�����

        /// <summary>
        /// ��Դ������
        /// </summary>
        public SimpleResourceManager ResourceManager { get { return m_resourceManager; } }
        protected SimpleResourceManager m_resourceManager;

        /// <summary>
        /// �������ݼ�����
        /// </summary>
        public ConfigDataLoaderBase ConfigDataLoader { get { return m_configDataLoader; } }
        protected ConfigDataLoaderBase m_configDataLoader;

        /// <summary>
        /// ����������
        /// </summary>
        public SceneLayerManager SceneLayerManager { get { return m_sceneLayerManager; } }
        protected SceneLayerManager m_sceneLayerManager;

        /// <summary>
        /// UI������
        /// </summary>
        public UIManager SimpleUIManager { get { return m_uiManager; } }
        protected UIManager m_uiManager;

        /// <summary>
        /// ������
        /// </summary>
        public SavingManagerBase SavingManager { get { return m_savingManager; } }
        protected SavingManagerBase m_savingManager;

        /// <summary>
        /// ��ҹ�����
        /// </summary>
        public GamePlayerBase GamePlayer { get { return m_gamePlayer; } }
        protected GamePlayerBase m_gamePlayer;

        /// <summary>
        /// corutine����
        /// </summary>
        protected SimpleCoroutineWrapper m_corutineWrapper = new SimpleCoroutineWrapper();

        #endregion

        /// <summary>
        /// ���ñ��ػ�����
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
        /// ������
        /// </summary>
        public string m_currLocalization;

    }
    
}
