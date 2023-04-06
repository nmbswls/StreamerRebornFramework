using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 更新管线现场
    /// </summary>

    public partial class GameModeLoadPipeLineCtxBase
    {
        public virtual bool Start()
        {
            if (m_runing) return false;
            m_runing = true;
            return true;
        }

        public virtual void Clear()
        {
            m_runing = false;
            m_loadingStaticResCorutineCount = 0;
            m_loadingDynamicResCorutineCount = 0;
            m_updateViewAction = null;

            SceneInfoId = 0;
            FakeSceneName = "";
            RetLayer = null;
        }

        public bool IsRunning()
        {
            return m_runing;
        }

        /// <summary>
        /// 加载场景信息
        /// </summary>
        public int SceneInfoId;
        public string FakeSceneName;
        
        /// <summary>
        /// 管线加载结果
        /// </summary>
        public SceneLayerUnity RetLayer;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        protected bool m_runing;

        /// <summary>
        /// 和该管线现场相关的资源加载过程数量
        /// </summary>
        public int m_loadingStaticResCorutineCount;
        public int m_loadingDynamicResCorutineCount;

        /// <summary>
        /// 需要在updateView时执行的action
        /// </summary>
        public Action m_updateViewAction;
    }

    /// <summary>
    /// 游戏
    /// </summary>
    public class GameModeBase
    {
        /// <summary>
        /// 主scene
        /// </summary>
        public SceneLayerUnity MainSceneLayer;

        /// <summary>
        /// 其他scene 当包含多场景时使用
        /// </summary>
        public List<SceneLayerUnity> ManagedScenes = new List<SceneLayerUnity>();


        /// <summary>
        /// 对外接口 开始一次加载 TODO REMOVE FAKE
        /// </summary>
        public void TryStartLoad(string sceneName, Action onEnd = null)
        {
            var pipeline = AllocPipeLineCtx();
            pipeline.FakeSceneName = sceneName;
            pipeline.SceneInfoId = 1;
            pipeline.m_updateViewAction += onEnd;

            m_pipeLineWait2Start.Add(pipeline);
        }


        /// <summary>
        /// 加载场景进入内存
        /// </summary>
        protected bool StartUpdatePipeline(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            // 判断是否使用默认的管线现场
            StartPipeLineCtx(pipeCtx);

            List<string> dynamicResPathList = CollectAllDynamicResForLoad(pipeCtx);

            StartLoadScene(pipeCtx);
            StartLoadDynamicRes(pipeCtx, dynamicResPathList);
            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dTime"></param>
        public virtual void Tick(float dTime)
        {
            // 处理pipline队列
            {
                while (m_pipeLineWait2Start.Count != 0)
                {
                    var ctx = m_pipeLineWait2Start[0];
                    StartUpdatePipeline(ctx);
                    m_pipeLineWait2Start.Remove(ctx);
                }
            }
        }

        #region 供子类重写

        public virtual bool IsReserve { get { return false; } }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initialize(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (pipeCtx.m_updateViewAction != null)
            {
                pipeCtx.m_updateViewAction();
            }
        }

        /// <summary>
        /// 收集场景
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectScenes4Load(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return new List<string>() { MainSceneResPath };
        }


        /// <summary>
        /// 收集需要加载的动态资源的路径
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectAllDynamicResForLoad(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return null;
        }

        /// <summary>
        /// 创建PipeLineCtx实例
        /// </summary>
        /// <returns></returns>
        protected virtual GameModeLoadPipeLineCtxBase CreatePipeLineCtx()
        {
            return new GameModeLoadPipeLineCtxBase();
        }

        #endregion


        #region 管线处理

        /// <summary>
        /// 启动管线现场
        /// </summary>
        /// <param name="pipeCtx"></param>
        /// <returns></returns>
        protected virtual bool StartPipeLineCtx(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (pipeCtx == null || !pipeCtx.Start())
            {
                return false;
            }
            // 将运行中的管线现场记录下来
            m_runingPipeLineCtxList.Add(pipeCtx);

            return true;
        }

        /// <summary>
        /// 清理管线现场
        /// </summary>
        /// <param name="pipeCtx"></param>
        protected virtual void ReleasePipeLineCtx(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            pipeCtx.Clear();
            m_runingPipeLineCtxList.Remove(pipeCtx);
            if (pipeCtx != m_pipeCtxDefault)
            {
                m_unusingPipeLineCtxList.Add(pipeCtx);
            }
        }

        /// <summary>
        /// 分配一个管线现场
        /// </summary>
        /// <returns></returns>
        protected virtual GameModeLoadPipeLineCtxBase AllocPipeLineCtx()
        {
            GameModeLoadPipeLineCtxBase ret;
            if (m_unusingPipeLineCtxList.Count != 0)
            {
                ret = m_unusingPipeLineCtxList[m_unusingPipeLineCtxList.Count - 1];
                m_unusingPipeLineCtxList.RemoveAt(m_unusingPipeLineCtxList.Count - 1);
            }
            else
            {
                ret = CreatePipeLineCtx();
            }
            return ret;
        }



        /// <summary>
        /// 默认管线现场
        /// </summary>
        protected GameModeLoadPipeLineCtxBase m_pipeCtxDefault = new GameModeLoadPipeLineCtxBase();

        /// <summary>
        /// 正在运行的管线现场的列表
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_runingPipeLineCtxList = new List<GameModeLoadPipeLineCtxBase>();

        /// <summary>
        /// 没有使用的管线现场
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_unusingPipeLineCtxList = new List<GameModeLoadPipeLineCtxBase>();

        /// <summary>
        /// 等待启动的管线
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_pipeLineWait2Start = new List<GameModeLoadPipeLineCtxBase>();

        #endregion


        #region 资源加载

        /// <summary>
        /// 开始加载静态资源
        /// </summary>
        protected virtual void StartLoadScene(GameModeLoadPipeLineCtxBase pipeCtx)
        {

            List<string> scenes4Load = CollectScenes4Load(pipeCtx);

            if(scenes4Load == null || scenes4Load.Count == 0)
            {
                // 继续管线，静态资源加载完成
                OnLoadStaticResCompleted(pipeCtx);
                return;
            }
            foreach(var scenePath in scenes4Load)
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                pipeCtx.m_loadingStaticResCorutineCount++;
                // 加载scene
                SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUnity), sceneName, scenePath,
                            (layer) =>
                            {
                            // 加载失败
                            if (layer == null)
                                {
                                    Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(),
                                        "Battle"));
                                    return;
                                }


                                pipeCtx.RetLayer = (SceneLayerUnity)layer;
                                pipeCtx.RetLayer.SetReserve(true);
                            // 加载中计数--
                            pipeCtx.m_loadingStaticResCorutineCount--;
                            // 继续管线，静态资源加载完成
                            OnLoadStaticResCompleted(pipeCtx);
                            });
            }
            
        }

        /// <summary>
        /// 启动动态资源加载
        /// </summary>
        /// <param name="resPathSet"></param>
        /// <param name="pipeCtx"></param>
        protected virtual void StartLoadDynamicRes(GameModeLoadPipeLineCtxBase pipeCtx, List<string> resPathList)
        {
            // 过滤出真正需要加载的资源
            if (resPathList == null || resPathList.Count == 0)
            {
                OnLoadDynamicResCompleted(pipeCtx, null);
                return;
            }
            var resPathSet = new HashSet<string>(resPathList);

            // 启动加载
            Dictionary<string, UnityEngine.Object> resDict = new Dictionary<string, UnityEngine.Object>();
            pipeCtx.m_loadingDynamicResCorutineCount++;
            SimpleResourceManager.Instance.StartLoadAssetsCorutine(resPathSet, resDict,
                () =>
                {
                    pipeCtx.m_loadingDynamicResCorutineCount--;
                    OnLoadDynamicResCompleted(pipeCtx, resDict);
                }, loadAsync: true);
        }

        /// <summary>
        /// 当静态资源加载完成
        /// </summary>
        protected virtual void OnLoadStaticResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (IsLoadAllResCompleted(pipeCtx)) OnLoadAllResCompleted(pipeCtx);
        }

        /// <summary>
        /// 当动态资源加载完成
        /// </summary>
        protected virtual void OnLoadDynamicResCompleted(GameModeLoadPipeLineCtxBase pipeCtx, Dictionary<string, UnityEngine.Object> resDict)
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
            if (IsLoadAllResCompleted(pipeCtx)) OnLoadAllResCompleted(pipeCtx);
        }

        /// <summary>
        /// 是否所有的资源加载都完成了
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadAllResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return pipeCtx.m_loadingStaticResCorutineCount == 0 && pipeCtx.m_loadingDynamicResCorutineCount == 0;
        }

        /// <summary>
        /// 当所有资源加载完成
        /// </summary>
        protected virtual void OnLoadAllResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            // Push Layer
            if(pipeCtx.RetLayer != null)
            {
                // 加载成功记录layer
                ManagedScenes.Add(pipeCtx.RetLayer);
                if (MainSceneLayer == null)
                {
                    MainSceneLayer = pipeCtx.RetLayer;
                }
            }

            if (Stopped)
            {
                // 清理资源
                ClearAllContextAndRes();
                // 通知管线更新完成(失败)
                if (pipeCtx.m_updateViewAction != null)
                    pipeCtx.m_updateViewAction();
                return;
            }

            // 更新显示
            Initialize(pipeCtx);

            // 管线现场结束了
            ReleasePipeLineCtx(pipeCtx);
        }


        /// <summary>
        /// 清理所有现场
        /// </summary>
        protected virtual void ClearAllContextAndRes()
        {
            foreach(var scene in ManagedScenes)
            {
                SceneLayerManager.Instance.FreeLayer(scene);
            }
        }

        /// <summary>
        /// 资源场景
        /// </summary>
        public virtual string MainSceneResPath { get; set; }

        /// <summary>
        /// stop 停止
        /// </summary>
        public bool Stopped { get; set; }

        #endregion

    }
}
