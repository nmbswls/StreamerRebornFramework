using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime.Resource
{
    /// <summary>
    /// 简单资源管理器 
    /// </summary>
    public partial class SimpleResourceManager
    {
        private bool Inited = false;

        #region 单例模式
        protected SimpleResourceManager() {; }

        /// <summary>
        /// 创建资源管理器
        /// </summary>
        /// <returns></returns>

        public static SimpleResourceManager CreateResourceManager()
        {
            if (m_instance == null)
            {
                m_instance = new SimpleResourceManager();
            }
            return m_instance;
        }
        /// <summary>
        /// 单例访问器
        /// </summary>
        public static SimpleResourceManager Instance { get { return m_instance; } }
        protected static SimpleResourceManager m_instance;

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>

        public bool Initlize(string initLocalization)
        {
            m_currLocalization = initLocalization;
            return true;
        }


        /// <summary>
        /// 释放
        /// </summary>

        public void Uninitlize()
        {
            m_corutineHelper.CancelAll();
            m_assetsCacheDict.Clear();
            m_bundleCacheDict.Clear();
        }

        /// <summary>
        /// tick
        /// </summary>
        public void Tick()
        {
            // tick corutine
            m_corutineHelper.Tick();

            // 进行等待中的卸载行为
            TickWaitingUnload();

            //TickBundle();
        }

        


        #region 对外接口

        /// <summary>
        /// LoadAsset Sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadAssetSync<T>(string path, bool mayMiss = false) where T : UnityEngine.Object
        {
            // 首先尝试从缓存获取
            T asset = GetAssetFromCache(path) as T;
            if (asset != null)
            {
                return asset;
            }

            // 先解析路径
            string subAssetName, assetPath;
            int atIndexInPath = path.IndexOf('@');
            if (atIndexInPath == -1)
            {
                assetPath = path;
                subAssetName = null;
            }
            else
            {
                assetPath = path.Substring(0, atIndexInPath);
                subAssetName = path.Substring(atIndexInPath + 1);
            }
            bool hasSubAssetName = !string.IsNullOrEmpty(subAssetName);

            // 是否需要加载所有的子资源
            bool isNeedLoadAllRes = IsNeedLoadAllForAssetPath(assetPath);

            if (hasSubAssetName && !isNeedLoadAllRes)
            {
                throw new ApplicationException(string.Format("[{0}] hasSubAssetName but not in fbx or png", path));
            }

            // 由于goto的原因需要在这里定义局部变量
            BundleCacheItem bundleCacheItem = null;

            // 用来保存loadall的结果，包括所有的subasset
            UnityEngine.Object[] allAsset = null;

            // 在Ready之前只能使用ResourcesLoad
            if (!Inited)
            {
                LoadAssetByResourcesLoad<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);

                // 直接goto不用走后面的各种加载方式
                goto LB_LOADEND;
            }

            // 是否允许在editor中加载bundle
            if (!Application.isEditor || m_loadAssetFromBundleInEditor)
            {
                // 从bundle加载资源
                var iter = LoadAssetFromBundle(assetPath,
                    (lPath, lAllAsset, lBundleCacheItem) => { allAsset = lAllAsset; bundleCacheItem = lBundleCacheItem; }, false, mayMiss
                    );
                while (iter.MoveNext())
                {
                }
                // 只有asset加载成功的情况才需要，关联assetcache和bundlecache
                bundleCacheItem = (allAsset != null ? bundleCacheItem : null);
            }
            else if (Application.isEditor)
            {
                // 从AssetDatabase加载资源
                LoadAssetByAssetDatabase<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);
            }

            // 最后fallback到resources.load
            if (allAsset == null || allAsset.Length == 0)
            {
                // 使用resources.load加载
                LoadAssetByResourcesLoad<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);
            }

            LB_LOADEND:

            if (allAsset == null || allAsset.Length == 0)
            {
                return asset;
            }

            // 当成功加载资源
            UnityEngine.Object mainAsset;

            // 从allAsset中取得，调用者需要的asset
            if (!hasSubAssetName)
            {
                asset = allAsset[0] as T;
            }
            else
            {
                // 当加载 AssetName.png的时候
                // 当通过AssetDataBase加载sprite时，mainAsset 为 Texture2D, subAsset为sprite,但是有概率顺序不对
                if (allAsset.Length == 2 &&
                    (allAsset[1] is Texture2D || allAsset[0] is Sprite))
                {
                    var temp = allAsset[0];
                    allAsset[0] = allAsset[1];
                    allAsset[1] = temp;
                }

                mainAsset = allAsset[0];
                foreach (var item in allAsset)
                {
                    if (item != null && item != mainAsset && item.name == subAssetName)
                    {
                        asset = item as T;
                        break;
                    }
                }

                // 当加载 AssetName.png的时候
                // 当通过AssetBundle加载sprite时，mainAsset就是sprite,没有subAsset,所用当asset为null时，直接使用mainAsset
                asset = asset != null ? asset : mainAsset as T;
            }

            // 如果这个时候找不到asset，说明加载到的资源集合中不存在指定asset
            if (asset == null)
            {
                Debug.Log(string.Format("LoadAsset fail for {0}", path));
            }

            // 处理PushAssetToCache
            for (int i = 0; i < allAsset.Length; i++)
            {
                var lasset = allAsset[i];
                if (lasset == null)
                    continue;

                if (lasset is Sprite)
                {
                    PushAssetToCache(string.Format("{0}@{1}", assetPath, lasset.name), lasset, bundleCacheItem);
                }
                else if (i == 0)
                {
                    // 对主asset直接使用path缓存
                    PushAssetToCache(assetPath, lasset, bundleCacheItem);
                }
                else
                {
                    // 对subasset需要拼接完整路径保存
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (lasset != null)
                    {
                        PushAssetToCache(string.Format("{0}@{1}", assetPath, lasset.name), lasset, bundleCacheItem);
                    }

                }
            }
            return asset;
        }

        /// <summary>
        /// 启动资源加载
        /// </summary>
        /// <param name="path">带后缀的资源路径,如果是resources目录之下的请使用相对于resources的路径，可以有后缀或者没有</param>
        /// <param name="onCompleted"></param>
        /// <param name="noErrlog"></param>
        /// <param name="loadAsync"></param>
        /// <returns></returns>
        public IEnumerator LoadAsset<T>(string path, Action<string, T> onCompleted, bool loadAsync = false)
            where T : UnityEngine.Object
        {

            m_loadingOpCount++;

            // 首先尝试从缓存获取
            var asset = GetAssetFromCache(path) as T;
            if (asset != null)
            {
                m_loadingOpCount--;
                onCompleted(path, asset);
                if (EventOnAssetLoaded != null) EventOnAssetLoaded();
                yield break;
            }

            // 先解析路径
            string subAssetName, assetPath;
            int atIndexInPath = path.IndexOf('@');
            if (atIndexInPath == -1)
            {
                assetPath = path;
                subAssetName = null;
            }
            else
            {
                assetPath = path.Substring(0, atIndexInPath);
                subAssetName = path.Substring(atIndexInPath + 1);
            }
            bool hasSubAssetName = !string.IsNullOrEmpty(subAssetName);

            // 是否需要加载所有的子资源
            bool isNeedLoadAllRes = IsNeedLoadAllForAssetPath(assetPath);

            if (hasSubAssetName && !isNeedLoadAllRes)
            {
                m_loadingOpCount--;
                throw new ApplicationException(string.Format("[{0}] hasSubAssetName but not in fbx", path));
            }
            // 由于goto的原因需要在这里定义局部变量
            BundleCacheItem bundleCacheItem = null;
            // 用来保存loadall的结果，包括所有的subasset
            UnityEngine.Object[] allAsset = null;

            // 在Ready之前只能使用ResourcesLoad
            if (!Inited)
            {
                LoadAssetByResourcesLoad<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);

                // 直接goto不用走后面的各种加载方式
                goto LB_LOADEND;
            }

            // 是否允许在editor中加载bundle
            if (!Application.isEditor || m_loadAssetFromBundleInEditor)
            {
                // 从bundle加载资源
                var iter = LoadAssetFromBundle(assetPath,
                    (lPath, lAllAsset, lBundleCacheItem) => { allAsset = lAllAsset; bundleCacheItem = lBundleCacheItem; },
                    loadAsync);
                while (iter.MoveNext())
                {
                    yield return null;
                }
                // 只有asset加载成功的情况才需要，关联assetcache和bundlecache
                bundleCacheItem = (allAsset != null ? bundleCacheItem : null);
            }

            // 在editor环境下可以fallback到AssetDatabase.load
            if (Application.isEditor &&
                (allAsset == null || allAsset.Length == 0))
            {
                // 从AssetDatabase加载资源
                LoadAssetByAssetDatabase<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);
            }

            // 最后fallback到resources.load
            if (allAsset == null || allAsset.Length == 0)
            {
                // 使用resources.load加载
                LoadAssetByResourcesLoad<T>(assetPath, (lPath, lAllAsset) => { allAsset = lAllAsset; }, isNeedLoadAllRes);
            }

            LB_LOADEND:

            // 当成功加载资源
            UnityEngine.Object mainAsset;
            if (allAsset != null && allAsset.Length != 0)
            {
                // 从allAsset中取得，调用者需要的asset
                if (!hasSubAssetName)
                {
                    asset = allAsset[0] as T;
                }
                else
                {
                    // 当加载 AssetName.png的时候
                    // 当通过AssetDataBase加载sprite时，mainAsset 为 Texture2D, subAsset为sprite,但是有概率顺序不对
                    if (allAsset.Length == 2 &&
                        (allAsset[1] is Texture2D || allAsset[0] is Sprite))
                    {
                        var temp = allAsset[0];
                        allAsset[0] = allAsset[1];
                        allAsset[1] = temp;
                    }

                    mainAsset = allAsset[0];
                    foreach (var item in allAsset)
                    {
                        if (item != null && item != mainAsset && item.name == subAssetName)
                        {
                            asset = item as T;
                            break;
                        }
                    }

                    // 当加载 AssetName.png的时候
                    // 当通过AssetBundle加载sprite时，mainAsset就是sprite,没有subAsset,所用当asset为null时，直接使用mainAsset
                    asset = asset != null ? asset : mainAsset as T;
                }

                // 如果这个时候找不到asset，说明加载到的资源集合中不存在指定asset
                if (asset == null)
                {
                    Debug.LogError(string.Format("LoadAsset fail for {0}", path));
                }

                // 处理PushAssetToCache
                for (int i = 0; i < allAsset.Length; i++)
                {
                    var lasset = allAsset[i];
                    if (lasset == null)
                        continue;

                    if (lasset is Sprite)
                    {
                        PushAssetToCache(string.Format("{0}@{1}", assetPath, lasset.name), lasset, bundleCacheItem);
                    }
                    else if (i == 0)
                    {
                        // 对主asset直接使用path缓存
                        PushAssetToCache(assetPath, lasset, bundleCacheItem);
                    }
                    else
                    {
                        // 对subasset需要拼接完整路径保存
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (lasset != null)
                        {
                            PushAssetToCache(string.Format("{0}@{1}", assetPath, lasset.name), lasset, bundleCacheItem);
                        }
                    }
                }


            }


            m_loadingOpCount--;


            // 通知调用者完成
            onCompleted(path, asset);
            if (EventOnAssetLoaded != null) EventOnAssetLoaded();
        }

        /// <summary>
        /// 启动资源加载协程
        /// </summary>
        /// <param name="pathList"></param>
        /// <param name="assetDict"></param>
        /// <param name="onComplete"></param>
        /// <param name="loadAsync"></param>
        public void StartLoadAssetsCorutine(HashSet<string> pathList, IDictionary<string, UnityEngine.Object> assetDict, Action onComplete, bool loadAsync = false)
        {
            // 当需要加载资源比较多的情况分两个协程加载
            if (pathList.Count >= 10)
            {
                int endCout = 0;
                m_corutineHelper.StartCoroutine(LoadAssetsCorutine(pathList, 0, assetDict, () => { endCout++; if (endCout == 2) { onComplete(); } }, loadAsync));
                m_corutineHelper.StartCoroutine(LoadAssetsCorutine(pathList, 1, assetDict, () => { endCout++; if (endCout == 2) { onComplete(); } }, loadAsync));
            }
            else
            {
                m_corutineHelper.StartCoroutine(LoadAssetsCorutine(pathList, int.MaxValue, assetDict, onComplete, loadAsync));
            }
        }

        #endregion



        

        #region 内部加载方式

        /// <summary>
        /// 使用Resources.Load加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onCompleted"></param>
        /// <param name="noErrlog">有些时候从这个函数加载不到资源不一定是error</param>
        /// <param name="isNeedLoadAllRes"></param>
        protected void LoadAssetByResourcesLoad<T>(string path, Action<string, UnityEngine.Object[]> onCompleted, bool isNeedLoadAllRes = false)
            where T : UnityEngine.Object
        {
            string rPath = ConvertFullPath2ResourcesRelativePath(path);

            UnityEngine.Object[] allAsset;

            // 只有fbx使用 loadall
            if (isNeedLoadAllRes)
            {
                allAsset = Resources.LoadAll(rPath);
            }
            else
            {
                allAsset = new UnityEngine.Object[1];
                allAsset[0] = Resources.Load<T>(rPath);
            }

            if (allAsset == null || allAsset.Length == 0)
            {
                Debug.Log(string.Format("LoadAssetByResourcesLoad fail {0}", rPath));
            }
            else
            {
                Debug.Log(string.Format("LoadAssetByResourcesLoad ok {0}", rPath));
            }
            onCompleted(rPath, allAsset);
        }


        /// <summary>
        /// 使用AssetDatabase加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="onCompleted"></param>
        /// <param name="noErrlog"></param>
        /// <param name="isNeedLoadAllRes"></param>
        protected void LoadAssetByAssetDatabase<T>(string path, Action<string, UnityEngine.Object[]> onCompleted, bool isNeedLoadAllRes = false)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            UnityEngine.Object[] allAsset = null;

            // 是否加载子资源
            if (isNeedLoadAllRes)
            {
                allAsset = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            }
            else
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    allAsset = new UnityEngine.Object[1];
                    allAsset[0] = asset;
                }
            }

            onCompleted(path, allAsset);
#endif
        }

        /// <summary>
        /// 批量资源加载协程
        /// </summary>
        /// <param name="pathList"></param>
        /// <param name="corutineId"></param>
        /// <param name="assetDict"></param>
        /// <param name="onComplete"></param>
        /// <param name="loadAsync"></param>
        /// <returns></returns>
        protected IEnumerator LoadAssetsCorutine(HashSet<string> pathList, int corutineId, IDictionary<string, UnityEngine.Object> assetDict, Action onComplete, bool loadAsync)
        {
            int i = -1;
            DateTime startTime = DateTime.Now;
            TimeSpan timeSpan;

            m_loadingOpCount++;

            foreach (string path in pathList)
            {
                // 让不同id的corutine加载不同资源
                i++;
                if (corutineId != int.MaxValue &&
                    i % 2 != corutineId)
                {
                    continue;
                }
                // 加载资源
                UnityEngine.Object asset = null;
                var iter = LoadAsset<UnityEngine.Object>(path, (lpath, lasset) => { asset = lasset; }, loadAsync: loadAsync);
                while (iter.MoveNext())
                {
                    yield return null;
                }

                if (assetDict != null)
                {
                    assetDict[path] = asset;
                }


                // 统计本次资源加载循环耗时，超过0.33s则启动Corutine暂停循环
                timeSpan = DateTime.Now - startTime;
                if (timeSpan.TotalSeconds > 0.33f)
                {
                    startTime = DateTime.Now;
                    yield return null;
                }
            }

            onComplete();

            m_loadingOpCount--;
        }



        #endregion



        #region 内部方法


        /// <summary>
        /// 定时
        /// </summary>
        protected void TickWaitingUnload()
        {
            // 是否有等待中的资源回收操作需要进行
            if (m_waitingUnloadUnusedAssets)
            {
                if (m_loadingOpCount == 0)
                {
                    m_waitingUnloadUnusedAssets = false;
                    UnloadUnusedAssets();
                }
            }

            // 是否存在未完成的UnloadUnusedAssetsOperation
            if (m_currUnloadUnusedAssetsOperation != null)
            {
                if (m_currUnloadUnusedAssetsOperation.isDone)
                {
                    m_currUnloadUnusedAssetsOperation = null;

                    // 先执行一次gc,清除对asset的引用
                    GC.Collect();

                    // 在UnloadUnusedAssets完成之后清理一下assetsCache
                    CleanUnusedAssetsCache();

                    // 检查是否需要卸载bundle
                    CheckUnusedBundles();
                }
            }
        }


        #region 缓存控制

        /// <summary>
        /// 清理无用的assetcache
        /// </summary>
        protected void CleanUnusedAssetsCache()
        {
            var caches = m_assetsCacheDict.Values;
            foreach (var item in caches)
            {
                if (!item.m_weakRefrence.IsAlive)
                {
                    m_assetsCacheToRemove.Add(item);
                }
            }
            foreach (var item in m_assetsCacheToRemove)
            {
                m_assetsCacheDict.Remove(item.m_cacheKey);
            }

            m_assetsCacheToRemove.Clear();
        }

        /// <summary>
        /// 需要删除的资源缓存
        /// </summary>
        protected List<AssetCacheItem> m_assetsCacheToRemove = new List<AssetCacheItem>();

        /// <summary>
        /// 从cache获取资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected UnityEngine.Object GetAssetFromCache(string key)
        {
            AssetCacheItem cacheItem;
            bool ret = m_assetsCacheDict.TryGetValue(key, out cacheItem);
            if (ret)
            {
                WeakReference wref = cacheItem.m_weakRefrence;
                if (wref.IsAlive)
                {
                    return wref.Target as UnityEngine.Object;
                }
                else
                {
                    m_assetsCacheDict.Remove(key);
                }
            }
            return null;
        }

        /// <summary>
        /// 将资源缓存到cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asset"></param>
        /// <param name="bundleCacheItem"></param>
        protected void PushAssetToCache(string key, UnityEngine.Object asset, BundleCacheItem bundleCacheItem = null)
        {
            AssetCacheItem assetCacheItem;

            // 检查是否已经缓存过了
            bool ret = m_assetsCacheDict.TryGetValue(key, out assetCacheItem);
            if (ret)
            {
                // 如果缓存的资源相同直接返回
                if (ReferenceEquals(assetCacheItem.m_weakRefrence.Target, asset))
                {
                    return;
                }
            }

            // 缓存新的asset
            assetCacheItem = new AssetCacheItem();
            assetCacheItem.m_weakRefrence = new WeakReference(asset);
            assetCacheItem.m_cacheKey = key;

            m_assetsCacheDict[key] = assetCacheItem;

            // 由于资源加载导致bundleCache命中
            if (bundleCacheItem != null)
            {
                OnBundleCacheHit(bundleCacheItem);
            }
        }

        #endregion


        /// <summary>
        /// 是否需要加载子资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        protected bool IsNeedLoadAllForAssetPath(string assetPath)
        {
            return assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
        }

        #endregion


        #region 工具

        /// <summary>
        /// 将以asset开头的路径转化为resources目录下相对路径（无扩展名）
        /// </summary>
        /// <returns></returns>
        public static string ConvertFullPath2ResourcesRelativePath(string fullPath)
        {
            // 处理后缀
            int postFixIndex = fullPath.LastIndexOf(".", StringComparison.Ordinal);
            string ret = postFixIndex == -1 ? fullPath : fullPath.Substring(0, postFixIndex);
            // 处理前缀
            int preFixIndex = ret.IndexOf("Resources/", StringComparison.Ordinal);
            ret = preFixIndex == -1 ? ret : ret.Substring(preFixIndex + "Resources/".Length);
            return ret;
        }

        #endregion

        /// <summary>
        /// 正在进行的加载行为数量
        /// </summary>
        protected int m_loadingOpCount;

        /// <summary>
        /// 是否正在等待卸载资源
        /// 当读取途中接收到请求时，置该标记位并延后处理
        /// </summary>
        protected bool m_waitingUnloadUnusedAssets = false;

        /// <summary>
        /// asset缓存
        /// </summary>
        protected Dictionary<string, AssetCacheItem> m_assetsCacheDict = new Dictionary<string, AssetCacheItem>();
        protected class AssetCacheItem
        {
            public string m_cacheKey;
            public WeakReference m_weakRefrence;
        }

        /// <summary>
        /// TinyCorutineHelper
        /// </summary>
        protected SimpleCoroutineWrapper m_corutineHelper = new SimpleCoroutineWrapper();

        /// <summary>
        /// 编辑器中测试assetbundle
        /// </summary>
        protected bool m_loadAssetFromBundleInEditor = false;

        /// <summary>
        /// 当资源加载完成
        /// </summary>
        public event Action EventOnAssetLoaded;

        /// <summary>
        /// 当前多语言类型
        /// </summary>
        private string m_currLocalization;

        /// <summary>
        /// 当前多语言类型
        /// </summary>
        public string CurrLocalization
        {
            get { return m_currLocalization; }
            set 
            { 
                m_currLocalization = value;
                OnLocalizationChange();
            }
        }

        /// <summary>
        /// 当Localization 改变
        /// </summary>
        public void OnLocalizationChange()
        {

        }
    }

    
}
