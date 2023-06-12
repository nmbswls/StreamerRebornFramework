using My.Framework.Runtime.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 最基本的场景加载现场
    /// 不含有逻辑，仅通过scene路径加载场景本身
    /// 子类自行转换为具体的数据结构
    /// </summary>
    public partial class GameWorldSceneLoadingCtxBase
    {

        public GameWorldSceneLoadingCtxBase()
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
        public Scene SceneLoaded;

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
}
