using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Battle.View
{
    public class BattleLoader
    {
        private SimpleCoroutineWrapper m_coroutine = new SimpleCoroutineWrapper();
        private Action m_actionOnEnd;
        private bool m_isComplete;
        protected int m_battleInfo;

        public bool isComplete
        {
            get { return m_isComplete; }
        }

        public void Load(int battleInfo, Action actionOnEnd)
        {
            if (m_isComplete)
            {
                LoadEnd();
                return;
            }
            m_actionOnEnd = actionOnEnd;
            Action action = () =>
            {
                m_coroutine.StartCoroutine(LoadStart());
            };

            //普通战斗定制loading
            UIControllerLoading.ShowLoadingUI(999, "begin", action);
        }

        public void Tick()
        {
            m_coroutine.Tick();
        }

        protected virtual IEnumerator LoadStart()
        {
            yield return UnloadPreScene();
            yield return SimpleResourceManager.Instance.UnloadUnusedAssetDirect("BattleLoader.LoadStart");
            yield return InitBattle();
            yield return LoadScene();
            yield return LoadUI();
            yield return LoadBattleActorResource(null);
            LoadEnd();
        }

        private IEnumerator InitBattle()
        {
            yield return null;
        }

        protected virtual IEnumerator UnloadPreScene()
        {
            yield return null;
            //UIManager.Instance.StopUITaskByGroup((int)UIGroup.GameLogicOutofBattle);
        }

        protected virtual IEnumerator LoadScene()
        {
            bool isSceneManagerInitOver = false;
            // TODO 真实初始化数据
            int sceneId = m_battleInfo;
            var loadingCtx = new XXXSceneLoadingCtxBase();
            loadingCtx.MainSceneName = "test/battle.unity";
            loadingCtx.Start();
            while (loadingCtx.IsRunning)
            {
                yield return true;
            }
            if (loadingCtx.SceneLoaded == null)
            {
                Debug.LogError($"Load Scene fail {loadingCtx.MainSceneName}");
                yield break;
            }
            // 初始化场景
            BattleManager.Instance.SceneManager.Initialize(loadingCtx.SceneLoaded.Value);
        }

        protected IEnumerator LoadUI()
        {

            UIControllerBattlePerform.StartUITask(null);

            UIControllerBattleStartup.StartUITask(null);
            while (!UIControllerBattleStartup.Instance.IsLayerInStack())
            {
                yield return null;
            }
        }


        protected void LoadEnd()
        {
            m_isComplete = true;

            UIControllerLoading.StopLoadingUI(m_actionOnEnd);
        }

        #region 记载各种资源

        /// <summary>
        /// 加载actor所需资源
        /// </summary>
        /// <param name="onCompleted"></param>
        protected virtual IEnumerator LoadBattleActorResource(Action onCompleted)
        {
            //BattleManager.Instance.SceneManager..ClearDeadList();
            //BattleSceneActorManager.Instance.RemoveAllSceneActor();

            int playerRes = 100;
            int mcsCountMax = 20;

            //foreach (var actor in m_battleState.battleSceneConfig.CharActorSetups)
            //{
            //    ConfigDataCharacterInfo characterInfo = ConfigDataLoader.Instance.GetConfigDataCharacterInfo(actor.CharacterId);
            //    if (characterInfo != null)
            //    {
            //        LoaderCombatCharacter loader = new LoaderCombatCharacter(actor.CharacterId, 0, characterInfo.CharacterShowId);
            //        LoaderManager.Instance.AddLoader(loader);
            //    }
            //}

            //collect all need
            {
                HashSet<string> msResNameList = new HashSet<string>();
                Dictionary<string, UnityEngine.Object> mResGameObjects = new Dictionary<string, UnityEngine.Object>();
                
                Dictionary<int, HashSet<string>> tempResMap = new Dictionary<int, HashSet<string>>(10);

                int miLoadCount = 0;
                int miLoadTick = 0;

                int i = 0;
                foreach (string resName in msResNameList)
                {
                    int idx = i / mcsCountMax;
                    if (!tempResMap.ContainsKey(idx))
                    {
                        tempResMap.Add(idx, new HashSet<string>());
                    }
                    tempResMap[idx].Add(resName);
                    i++;
                }

                foreach (HashSet<string> hashSet in tempResMap.Values)
                {
                    SimpleResourceManager.Instance.StartLoadAssetsCorutine(hashSet, mResGameObjects,
                        () => { miLoadTick++;}, true);
                }

                while (miLoadTick < miLoadCount)
                {
                    yield return null;
                }

                BattleManager.Instance.SceneManager.ActorManager.Initialize(BattleManager.Instance.BattleLogic);
                onCompleted?.Invoke();
            }
        }

        #endregion

    }


    #region MyRegion

    /// <summary>
    /// 场景加载现场
    /// 希望用于任意场景的加载
    /// </summary>
    public partial class XXXSceneLoadingCtxBase
    {

        public XXXSceneLoadingCtxBase()
        {

        }

        /// <summary>
        /// 开始加载
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            if (m_runing) return false;
            m_runing = true;

            // 加载主场景
            StartLoadMainScene();

            // 加载所需动态资源
            List<string> dynamicResPathList = DynamicResourceCollect();
            if (dynamicResPathList != null)
            {
                StartLoadDynamicRes(dynamicResPathList);
            }

            return true;
        }

        /// <summary>
        /// 加载场景信息
        /// </summary>
        public string MainSceneName;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get { return m_runing; } }
        protected bool m_runing;

        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool IsErrorOccur { get { return m_isErrorOccur; } }
        protected bool m_isErrorOccur;

        /// <summary>
        /// 和该管线现场相关的资源加载过程数量
        /// </summary>
        public int m_loadingStaticResCorutineCount;
        public int m_loadingDynamicResCorutineCount;

        #region 加载结果

        /// <summary>
        /// 主场景
        /// </summary>
        public Scene? SceneLoaded;

        #endregion

        /// <summary>
        /// 需要在updateView时执行的action
        /// </summary>
        public Action m_onLoadingEnd;

        #region 资源加载控制

        /// <summary>
        /// 开始加载静态资源
        /// </summary>
        protected virtual void StartLoadMainScene()
        {
            List<string> scenes4Load = MainSceneCollect();

            if (scenes4Load == null || scenes4Load.Count == 0)
            {
                // 继续管线，静态资源加载完成
                OnLoadStaticResCompleted();
                return;
            }

            foreach (var scenePath in scenes4Load)
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    // 直接continue可能导致死等
                    continue;
                }
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                m_loadingStaticResCorutineCount++;
                // 加载scene
                SimpleResourceManager.Instance.StartLoadSceneCorutine(scenePath,
                    (scenePath, scene) =>
                    {
                        // 加载失败
                        if (scene == null)
                        {
                            Debug.LogError(string.Format("Load unity scene fail task={0} layer={1}", ToString(),
                                scenePath));
                            m_isErrorOccur = true;
                            m_runing = false;
                            return;
                        }

                        SceneLoaded = scene.Value;

                        // 加载中计数--
                        m_loadingStaticResCorutineCount--;
                        // 继续管线，静态资源加载完成
                        OnLoadStaticResCompleted();
                    });
            }
        }

        /// <summary>
        /// 启动动态资源加载
        /// </summary>
        /// <param name="resPathSet"></param>
        /// <param name="pipeCtx"></param>
        protected virtual void StartLoadDynamicRes(List<string> resPathList)
        {
            // 过滤出真正需要加载的资源
            if (resPathList == null || resPathList.Count == 0)
            {
                OnLoadDynamicResCompleted(null);
                return;
            }
            var resPathSet = new HashSet<string>(resPathList);

            // 启动加载
            Dictionary<string, UnityEngine.Object> resDict = new Dictionary<string, UnityEngine.Object>();
            m_loadingDynamicResCorutineCount++;
            SimpleResourceManager.Instance.StartLoadAssetsCorutine(resPathSet, resDict,
                () =>
                {
                    m_loadingDynamicResCorutineCount--;
                    OnLoadDynamicResCompleted(resDict);
                }, loadAsync: true);
        }

        /// <summary>
        /// 当静态资源加载完成
        /// </summary>
        public virtual void OnLoadStaticResCompleted()
        {
            if (IsLoadAllResCompleted()) OnLoadAllResCompleted();
        }

        /// <summary>
        /// 当动态资源加载完成
        /// </summary>
        public virtual void OnLoadDynamicResCompleted(Dictionary<string, UnityEngine.Object> resDict)
        {
            if (resDict != null && resDict.Count != 0)
            {
                // 将刚加载的资源缓存到m_dynamicResCacheDict
                foreach (var item in resDict)
                {
                    // 只处理成功记载的情况
                    if (item.Value != null)
                    {

                    }
                }
            }
            if (IsLoadAllResCompleted()) OnLoadAllResCompleted();
        }

        /// <summary>
        /// 是否所有的资源加载都完成了
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadAllResCompleted()
        {
            return m_loadingStaticResCorutineCount == 0 && m_loadingDynamicResCorutineCount == 0;
        }

        /// <summary>
        /// 当所有资源加载完成
        /// </summary>
        protected virtual void OnLoadAllResCompleted()
        {
            m_runing = false;
            // 通知管线更新完成(失败)
            if (m_onLoadingEnd != null)
                m_onLoadingEnd();
        }

        #endregion

        #region 供子类重写

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> MainSceneCollect()
        {
            return new List<string>() { MainSceneName };
        }

        /// <summary>
        /// 收集资源
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> DynamicResourceCollect()
        {
            return null;
        }

        #endregion
    }

    #endregion

}
