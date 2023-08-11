using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameWorldStateEnvBase
    {
        /// <summary>
        /// 执行协程
        /// </summary>
        /// <param name="corutine"></param>
        void StartCorutine(IEnumerator corutine);

        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="isStopPre"></param>
        void ChangeState(int newState, bool isStopPre = true);
    }

    /// <summary>
    /// scene 管理器
    /// </summary>
    public class SceneHandlerHall : SceneHandlerBase
    {
        public Transform m_dynamicRoot;
    }
    /// <summary>
    /// 游戏状态机 状态类
    /// </summary>
    public abstract class GameWorldStateBase
    {
        /// <summary>
        /// 获取状态类型 由枚举生成
        /// </summary>
        public abstract int StateType { get; }

        #region 生命周期

        public void Init(IGameWorldStateEnvBase env)
        {
            m_env = env;
        }

        public void SetEndAction(Action endAction)
        {
            m_onEndAction = endAction;
        }


        public void UnInit()
        {

        }

        public void Enter()
        {
            m_isPaused = false;
            m_isExit = false;
            m_env.StartCorutine(OnEnter());
        }


        public void Resume()
        {
            m_isPaused = false;
            m_isExit = false;
            OnResume();
        }

        public void Pause()
        {
            m_isPaused = true;
            m_isExit = false;
            OnPause();
        }

        public void Exit()
        {
            m_isPaused = false;
            m_isExit = true;
            OnExit();
        }


        /// <summary>
        /// 获取主相机
        /// </summary>
        public virtual void GetMainCameraControl()
        {

        }

        #endregion

        #region 状态切换

        public Action onEnterEvent;
        public Action onUpdateEvent;
        public Action onExitEvent;

        protected virtual IEnumerator OnEnter()
        {
            yield break;
        }

        protected virtual void OnExit()
        {
            m_onEndAction = null;
            ClearAllContextAndRes();
        }

        protected virtual void OnPause()
        {

        }
        protected virtual void OnResume()
        {
        }

        #endregion

        #region 回调

        public virtual void OnInitialize()
        {

        }
        public virtual void OnUpdate()
        {
        }

        #endregion

        public void InvokeEndAction()
        {
            if (m_onEndAction == null) return;

            m_onEndAction.Invoke();
        }

        #region 参数


        #endregion

        #region 场景加载控制

        /// <summary>
        /// 最简单的切换场景 - 显示loading 将新场景加载 将旧场景缓存
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SwitchSceneSimple(string newScene, Action<bool> onSwitchEnd = null)
        {
            // 没有场景变化 直接返回
            if (m_currScene == newScene)
            {
                onSwitchEnd?.Invoke(true);
                yield break;
            }

            
            SceneHandlerBase sceneHandler;
            // 场景未加载过 需要先加载 直接刷新即可
            if (!m_loadedSceneDict.TryGetValue(newScene, out sceneHandler) || sceneHandler == null)
            {
                // 启动场景加载管线
                LaunchLoadingCtx(newScene);

                while(!IsSceneLoadingCtxAllFinish())
                {
                    yield return null;
                }

                // 如果已经退出 则不处理
                if (m_isExit)
                {
                    // 清理资源
                    ClearAllContextAndRes();
                    yield break;
                }
            }

            // 如果执行完流程仍无场景 说明出错 进行错误处理
            // 这里加载场景还卡着 考虑直接崩到开始画面
            if (sceneHandler == null && !m_loadedSceneDict.TryGetValue(newScene, out sceneHandler))
            {
                Debug.LogError("Error occur when SwitchSceneSimple ");
                onSwitchEnd?.Invoke(false);
                yield break;
            }

            // 完成切换场景后 执行方法
            InitializeOnSwitch(sceneHandler);

            // 挂起上一个场景
            if(!string.IsNullOrEmpty(m_currScene))
            {
                if(m_loadedSceneDict.TryGetValue(m_currScene, out var oldSceneHandler))
                {
                    oldSceneHandler.Suspend();
                }
            }


            m_currScene = newScene;
            onSwitchEnd?.Invoke(true);
        }

        /// <summary>
        /// 准备场景加载管线
        /// </summary>
        /// <param name="newScene"></param>
        protected virtual void LaunchLoadingCtx(string newScene)
        {
            // 加载资源
            // 包括静态资源 动态资源
            var newCtx = CreateLoadingCtx();
            newCtx.MainSceneName = newScene;
            newCtx.m_onLoadingEnd += delegate () {
                var retScene = newCtx.SceneLoaded;
                OnSceneLoaded(newCtx.MainSceneName, retScene);
            };

            newCtx.Start();
            m_mainLoadingCtx = newCtx;
        }

        /// <summary>
        /// 创建PipeLineCtx实例
        /// </summary>
        /// <returns></returns>
        protected virtual GameWorldSceneLoadingCtxBase CreateLoadingCtx()
        {
            return new GameWorldSceneLoadingCtxBase();
        }

        /// <summary>
        /// 获取进入状态后需要加载的主场景
        /// </summary>
        /// <returns></returns>
        protected virtual string GetMainSceneName()
        {
            return string.Empty;
        }

        /// <summary>
        /// 清理资源
        /// 在退出状态 或 在退出状态下完成加载时 触发调用
        /// </summary>
        protected virtual void ClearAllContextAndRes()
        {
            // 卸载所有场景
            foreach (var scene in m_loadedSceneDict.Values)
            {
                SceneManager.UnloadSceneAsync(scene.UnityScene);
            }
            m_loadedSceneDict.Clear();
            m_currScene = string.Empty;

            // 清理加载现场
            m_mainLoadingCtx = null;
        }

        /// <summary>
        /// 在场景下载完毕后执行逻辑
        /// </summary>
        protected virtual void InitializeOnSwitch(SceneHandlerBase sceneHandler)
        {
            sceneHandler.SetActive();
        }

        /// <summary>
        /// 判断所有加载现场都已完毕
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsSceneLoadingCtxAllFinish()
        {
            if (m_mainLoadingCtx == null)
            {
                return false;
            }

            return !m_mainLoadingCtx.IsRunning;
        }

        /// <summary>
        /// 加载前的ui操作
        /// 例如显示loading界面等
        /// </summary>
        /// <returns></returns>
        protected IEnumerator UIBeforeLoading()
        {
            var loadingCtrl = UIManager.Instance.FindUIControllerByName("Loading") as UIControllerLoading;
            if (loadingCtrl == null || loadingCtrl.State != UIControllerBase.UIState.Running)
            {
                loadingCtrl = UIControllerLoading.ShowLoadingUI(0, "loading tips");
                while (!loadingCtrl.IsOpen)
                {
                    yield return 0;
                }
            }
        }

        /// <summary>
        /// 加载后的ui操作
        /// </summary>
        /// <returns></returns>
        protected IEnumerator UIAfterLoading()
        {
            UIControllerLoading.StopLoadingUI();
            yield return null;
        }

        /// <summary>
        /// 场景加载成功后回调
        /// </summary>
        protected void OnSceneLoaded(string sceneName, Scene scene)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            if (rootGameObjects == null || rootGameObjects.Length == 0)
            {
                return;
            }
            var rootGo = rootGameObjects[0];
            var handler = rootGo.AddComponent<SceneHandlerHall>();
            m_loadedSceneDict.TryAdd(sceneName, handler);
            handler.UnityScene = scene;
            handler.MainRootGameObject = rootGo;
            handler.Initialize();
        }

        /// <summary>
        /// 当前场景
        /// </summary>
        protected string m_currScene = string.Empty;

        /// <summary>
        /// 当前正在运行的loading现场
        /// </summary>
        protected GameWorldSceneLoadingCtxBase m_mainLoadingCtx;

        /// <summary>
        /// 已加载场景表
        /// </summary>
        protected Dictionary<string, SceneHandlerBase> m_loadedSceneDict = new Dictionary<string, SceneHandlerBase>();

        #endregion

        #region 成员

        /// <summary>
        /// 状态机 幻境
        /// </summary>
        protected IGameWorldStateEnvBase m_env;

        /// <summary>
        /// 结束回调
        /// </summary>
        public Action m_onEndAction;

        /// <summary>
        /// 暂停标记位
        /// </summary>
        protected bool m_isPaused;
        public bool IsPaused { get { return m_isPaused; } }

        /// <summary>
        /// 停止标记位
        /// </summary>
        protected bool m_isExit;
        public bool IsExit { get { return m_isExit; } }

        #endregion
    }

    /// <summary>
    /// 状态定义
    /// </summary>
    public class GameWorldStateTypeDefineBase
    {
        public const int None = 0;
        public const int SimpleHall = 1;
        public const int SimpleMap = 2;
    }

}
