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

    #region 各基本状态

    /// <summary>
    /// 世界状态 - 空
    /// 非法状态，表示主菜单等游戏外状态
    /// </summary>
    public class GameWorldStateNone : GameWorldStateBase
    {
        /// <summary>
        /// 获取状态类型 由枚举生成
        /// </summary>
        public override int StateType { get { return GameWorldStateTypeDefineBase.None; } }

        /// <summary>
        /// Tick
        /// </summary>
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                m_env.ChangeState(GameWorldStateTypeDefineBase.SimpleHall);
            }
        }
    }

    /// <summary>
    /// 世界状态 - 大厅中
    /// </summary>
    public class GameWorldStateSimpleHall : GameWorldStateBase
    {
        /// <summary>
        /// 获取状态类型 由枚举生成
        /// </summary>
        public override int StateType { get { return GameWorldStateTypeDefineBase.SimpleHall; } }

        /// <summary>
        /// Tick
        /// </summary>
        public override void OnUpdate()
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                m_env.ChangeState(GameWorldStateTypeDefineBase.SimpleMap);
            }
        }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        protected override IEnumerator OnEnter()
        {

            var sceneName = GetMainSceneName();
            yield return SwitchSceneSimple(sceneName, onSwitchEnd: OnEnterHall);
        }

        /// <summary>
        /// 离开 清理资源
        /// </summary>
        protected override void OnExit()
        {
            // 暂停时 离开不清理资源
            if (m_isPaused)
            {
                return;
            }

            // 清理
            m_cameraManager?.UnInitialize();
            m_cameraManager = null;

            base.OnExit();
        }

        protected override void OnPause()
        {
            foreach(var sceneHandler in m_loadedSceneDict.Values)
            {
                sceneHandler.MainRootGameObject.SetActive(false);
            }
        }

        protected override void OnResume()
        {
            foreach (var sceneHandler in m_loadedSceneDict.Values)
            {
                sceneHandler.MainRootGameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 获取主场景句柄类
        /// </summary>
        /// <returns></returns>
        public SceneHandlerHall GetMainSceneHandler()
        {
            if(m_loadedSceneDict.Count == 0)
            {
                return null;
            }
            return (SceneHandlerHall)m_loadedSceneDict.First().Value;
        }


        /// <summary>
        /// 进入大厅后执行逻辑
        /// </summary>
        public void OnEnterHall(bool ret)
        {
            Debug.Log("进入大厅 " + ret);

            var main = GetMainSceneHandler();
            // do init
            var virtualRoot = main.MainRootGameObject.transform.Find("VirtualCameraRoot");
            var mainCamera = main.MainRootGameObject.transform.Find("MainCamera").GetComponent<Camera>();
            m_cameraManager = new WorldCameraManager();
            m_cameraManager.Initialize(virtualRoot, mainCamera);

            UIControllerLoading.StopLoadingUI();
        }

        /// <summary>
        /// 获取进入状态后需要加载的主场景
        /// </summary>
        /// <returns></returns>
        protected override string GetMainSceneName()
        {
            return m_defaultSceneName;
        }


        #region 内部变量

        /// <summary>
        /// 主场景名
        /// </summary>
        protected string m_defaultSceneName = "Assets/Scenes/Hall.unity";

        /// <summary>
        /// 相机管理器
        /// </summary>
        public WorldCameraManager CameraManager { get { return m_cameraManager; } }
        protected WorldCameraManager m_cameraManager;

        #endregion

    }

    /// <summary>
    /// 世界状态 - 简单的单场景
    /// </summary>
    public class GameWorldStateSimpleMap : GameWorldStateBase
    {
        /// <summary>
        /// 获取状态类型 - 由常量定义
        /// </summary>
        public override int StateType { get { return (int)GameWorldStateTypeDefineSample.SimpleMap; } }

        /// <summary>
        /// 进入状态回调
        /// </summary>
        protected override IEnumerator OnEnter()
        {

            yield return UIBeforeLoading();

            var sceneName = GetMainSceneName();

            // collect scenes
            yield return SwitchSceneSimple(sceneName, onSwitchEnd: OnEnterMap);

            yield return UIAfterLoading();
        }

        /// <summary>
        /// 离开 清理资源
        /// </summary>
        protected override void OnExit()
        {
            // 暂停时 离开不清理资源
            if (m_isPaused)
            {
                return;
            }
            ClearAllContextAndRes();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public override void OnUpdate()
        {
            CheckPreload();

            // 检查预加载结果
            for (int i=m_runingLoadingCtxList.Count - 1; i>=0;i--)
            {
                var ctx = m_runingLoadingCtxList[i];
                // 仍在加载 不处理
                if (ctx.IsRunning)
                {
                    continue;
                }
                m_runingLoadingCtxList.RemoveAt(i);
            }
        }

        /// <summary>
        /// 获取进入状态后需要加载的主场景
        /// </summary>
        /// <returns></returns>
        protected override string GetMainSceneName()
        {
            // 根据计算获取当前场景
            return "Assets/Scenes/LevelRoot.unity";
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchScene(string sceneName)
        {
            m_env.StartCorutine(SwitchSceneSimple(sceneName, onSwitchEnd: OnEnterMap));
        }


        /// <summary>
        /// 准备场景加载管线
        /// </summary>
        /// <param name="newScene"></param>
        protected override void LaunchLoadingCtx(string newScene)
        {
            base.LaunchLoadingCtx(newScene);

            string[] sceneList = new string[] { };
            foreach(var sceneName in sceneList)
            {
                // 跳过已加载
                if (IsSceneLoading(newScene))
                {
                    continue;
                }

                // 加载资源
                // 包括静态资源 动态资源
                var newCtx = CreateLoadingCtx();
                newCtx.MainSceneName = newScene;
                newCtx.m_onLoadingEnd += delegate () {
                    var retScene = newCtx.SceneLoaded;
                    OnSceneLoaded(newCtx.MainSceneName, retScene);
                };

                m_runingLoadingCtxList.Add(newCtx);
                newCtx.Start();
            }
        }


        /// <summary>
        /// 是否所有加载管线都已完成
        /// </summary>
        /// <returns></returns>
        protected override bool IsSceneLoadingCtxAllFinish()
        {
            return base.IsSceneLoadingCtxAllFinish();
        }

        /// <summary>
        /// 是否已正在加载
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        protected bool IsSceneLoading(string sceneName)
        {
            foreach(var ctx in m_runingLoadingCtxList)
            {
                if(ctx.MainSceneName == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 进入大厅后执行逻辑
        /// </summary>
        public void OnEnterMap(bool ret)
        {
            Debug.Log("OnEnterMap " + ret);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void CheckPreload()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                if(IsSceneLoading("Assets/Scenes/Map02.unity"))
                {
                    return;
                }
                // 加载资源
                // 包括静态资源 动态资源
                var ctx = CreateLoadingCtx();
                ctx.MainSceneName = "Assets/Scenes/Map02.unity";
                ctx.m_onLoadingEnd += delegate () {
                    var retScene = ctx.SceneLoaded;
                    OnSceneLoaded(ctx.MainSceneName, retScene);
                };

                ctx.Start();
                m_runingLoadingCtxList.Add(ctx);
            }
        }

        /// <summary>
        /// 正在运行的管线现场的列表
        /// </summary>
        protected List<GameWorldSceneLoadingCtxBase> m_runingLoadingCtxList = new List<GameWorldSceneLoadingCtxBase>();

        #region 内部方法


        #endregion

        #region 私有变量


        #endregion
    }

    #endregion

}
