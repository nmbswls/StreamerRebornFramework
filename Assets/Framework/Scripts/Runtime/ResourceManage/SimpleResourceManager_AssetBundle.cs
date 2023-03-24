using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace My.Framework.Runtime.Resource
{
    public class BundlesMetaHelper
    {
        public BundlesMetaHelper(BundlesMetaInfo internalMeta)
        {
            m_internalBundleMeta = internalMeta;

            m_variantBundleInfoDict.Clear();
            m_internalBundleMeta.m_bundleInfoList.ForEach((elem) => {
                string bundleKey = elem.m_bundleName;
                if (!string.IsNullOrEmpty(elem.m_variant))
                {
                    bundleKey = $"{elem.m_bundleName}.{elem.m_variant}";
                }
                m_bundleDataDict.Add(bundleKey, elem);
            });

            Dictionary<string, SingleBundleInfo> variantBundleInfoDict;
            m_internalBundleMeta.m_bundleInfoList.ForEach((elem) =>
            {
                elem.m_assetList.ForEach((assetPath) =>
                {
                    m_assetPath2BundleInfoDict.Add(assetPath, elem);
                });

                if (string.IsNullOrEmpty(elem.m_variant))
                {
                    return; // 不是变体bundle
                }

                var bundleNameWithoutExt = Path.GetFileNameWithoutExtension(elem.m_bundleName);

                if (!m_variantBundleInfoDict.TryGetValue(bundleNameWithoutExt, out variantBundleInfoDict))
                {
                    // bundleName去除后缀作为Key
                    variantBundleInfoDict = new Dictionary<string, SingleBundleInfo>();
                    m_variantBundleInfoDict[bundleNameWithoutExt] = variantBundleInfoDict;
                }
                // 缓存每个多语言对应的BundleData
                variantBundleInfoDict[elem.m_variant.ToLower()] = elem;
            });
        }

        /// <summary>
        /// 通过bundlename获取BundleData
        /// </summary>
        /// <param name="name"></param>
        /// <param name="replaceByLocalization">是否替换为当前设置的多语言的BundleData</param>
        /// <returns></returns>
        public SingleBundleInfo GetBundleInfoByName(string bundleName, bool replaceByLocalization = true)
        {
            if (!m_bundleDataDict.TryGetValue(bundleName, out var bundleInfo))
            {
                return null;
            }

            if (replaceByLocalization)
            {
                bundleInfo = ReplaceLocalizationBundleData(bundleInfo);
            }

            return bundleInfo;
        }

        /// <summary>
        /// 通过资源路径获取bundledata
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public SingleBundleInfo GetBundleInfoByAssetPath(string assetPath, bool replaceByLocalization = true)
        {
            if (!m_assetPath2BundleInfoDict.TryGetValue(assetPath, out var bundleInfo))
            {
                return null;
            }

            if (replaceByLocalization)
            {
                bundleInfo = ReplaceLocalizationBundleData(bundleInfo);
            }
            return bundleInfo;
        }

        /// <summary>
        /// 获得当前多语言相关 bundleData
        /// </summary>
        /// <param name="bundleData"></param>
        /// <returns></returns>
        private SingleBundleInfo ReplaceLocalizationBundleData(SingleBundleInfo bundleInfo)
        {
            if (string.IsNullOrWhiteSpace(bundleInfo.m_variant))
            {
                return bundleInfo;
            }

            var bundleNameWithoutExt = Path.GetFileNameWithoutExtension(bundleInfo.m_bundleName);
            Dictionary<string, SingleBundleInfo> variantBundleInfoDict;
            SingleBundleInfo locBundleInfo;

            if (!m_variantBundleInfoDict.TryGetValue(bundleNameWithoutExt, out variantBundleInfoDict)
                || !variantBundleInfoDict.TryGetValue(SimpleResourceManager.Instance.CurrLocalization, out locBundleInfo))
            {
                locBundleInfo = bundleInfo;
                Debug.LogError(string.Format("Can't find variant bundleData: {0}.{1}", bundleNameWithoutExt,
                    SimpleResourceManager.Instance.CurrLocalization));
            }

            return locBundleInfo;
        }


        private BundlesMetaInfo m_internalBundleMeta;


        /// <summary>
        /// 名称 - bundleInfo
        /// </summary>
        private Dictionary<string, SingleBundleInfo> m_bundleDataDict = new Dictionary<string, SingleBundleInfo>();

        /// <summary>
        /// 资源路径 - bundleInfo
        /// </summary>
        private Dictionary<string, SingleBundleInfo> m_assetPath2BundleInfoDict = new Dictionary<string, SingleBundleInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// BundleName和变体BundleInfo对应表
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, SingleBundleInfo>> m_variantBundleInfoDict =
            new Dictionary<string, Dictionary<string, SingleBundleInfo>>();
    }


    public static class PathHelper
    {
        /// <summary>
        /// 整理路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReformPathString(string path)
        {
            path = path.Replace("\\", "/");
            path = path.Replace("//", "/");
            path = path.Replace(@"\\", "/");

            while (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);
            return path;
        }

        /// <summary>
        /// 规范化路径字符串
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static List<string> ReformPathStrings(IEnumerable paths)
        {
            List<string> resPaths = new List<string>();
            foreach (string path in paths)
            {
                resPaths.Add(ReformPathString(path));
            }

            return resPaths;
        }
    }

    public partial class SimpleResourceManager
    {
        public class BundleCacheItem
        {
            public void AddRefrence()
            {
                m_refCount++;
            }

            public void RemoveRefrence()
            {
                if (m_refCount > 0)
                    m_refCount--;
                else
                {
                    Debug.LogError("[ResourceManager]RefCount Error: " + m_bundleName);
                }
            }

            public int RefCount
            {
                get { return m_refCount; }
            }

            public string m_bundleName;
            public DateTime m_lastTouchTime;     // 上一次的访问时间
            public DateTime m_timeOutTime;       // 过期时间
            public int m_hitCount;                      // 命中次数
            public int m_refCount;                      // 被依赖引用的次数
            public AssetBundle m_bundle;                // 被缓存的bundle
            public SingleBundleInfo m_bundleInfo;
            public bool m_bRootLoad = false;
            /// <summary>
            /// 依赖项
            /// </summary>
            public List<BundleCacheItem> m_dependBundleCacheList;
        }

        public class BundleLoadingCtx
        {
            public string m_bundleName;
            public AssetBundle m_bundle;
            public bool m_isEnd;
            public int m_ref;
        }

        /// <summary>
        /// 开始处理StreamingAssetsFiles,将必要的bundle加载到cache
        /// </summary>
        public IEnumerator InitializeAssetBundle(string versionName, Action<bool> onEnd)
        {
            Debug.Log("InitializeAssetBundle Load ");

            // 加载bundle
            AssetBundle mainManifestBundle = null;
            var lbIter = LoadBundleFromWWWOrStreamingImpl(versionName, false, (lbundle) => { mainManifestBundle = lbundle; });
            yield return lbIter;

            // 没有主资源包 不行
            if (mainManifestBundle == null)
            {
                yield break;
            }

            // 加载AssetBundleManifest
            var assets = mainManifestBundle.LoadAllAssets<AssetBundleManifest>();
            if (assets.Length <= 0)
            {
                Debug.LogError("BundleDataLoadingWorker can not find AssetBundleManifest in bundle ");
                yield break;
            }

            m_assetBundleManifest = assets[0];

            // 释放bundle
            mainManifestBundle.Unload(false);

            m_bundleMeta = Resources.Load(AssetBundleMetaNameInResources) as BundlesMetaInfo;
            BundlesHelper = new BundlesMetaHelper(m_bundleMeta);

            Inited = true;

            //LoadResidentAssetBundle();
        }

        /// <summary>
        /// 加载常驻内存的AB
        /// </summary>
        public void LoadResidentAssetBundle(bool warmup = true)
        {
            if(BundlesHelper == null)
            {
                return;
            }

            foreach (var singleBundleInfo in m_bundleMeta.m_bundleInfoList)
            {
                if (singleBundleInfo != null && singleBundleInfo.m_isResident)
                {

                    var iter = LoadAssetBundle(singleBundleInfo, false, null);
                    while (iter.MoveNext())
                    {

                    }
                }
            }
        }


        /// <summary>
        /// 从bundle加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        protected IEnumerator LoadAssetFromBundle(string path, Action<string, UnityEngine.Object[], BundleCacheItem> onCompleted, bool loadAync = false, bool mayMiss = false)
        {
            // 首先获取资源对应的bundle名称
            var assetBundleInfo = BundlesHelper.GetBundleInfoByAssetPath(path);

            // 如果获取不到bundleData，直接失败
            if (assetBundleInfo == null)
            {
                if (!mayMiss)
                {
                    //Debug.LogError(string.Format("LoadAssetFromBundle bundleData=null {0}", path));
                }
                onCompleted(path, null, null);
                yield break;
            }

            // 查找bundlecache
            AssetBundle bundle = null;
            var bundleCacheItem = GetAssetBundleFromCache(assetBundleInfo.m_bundleName);

            // 如果cache没有命中
            if (bundleCacheItem == null)
            {
                // 启动bundle加载
                var iter  = LoadAssetBundle(assetBundleInfo, loadAync, null);
                while (iter.MoveNext())
                {
                    yield return null;
                }

                if (bundle == null)
                {
                    Debug.LogError(string.Format("[BundleQueue]LoadAssetFromBundle LoadBundle fail {0}", assetBundleInfo.m_bundleName));

                    onCompleted(path, null, null);
                    yield break;
                }
            }
            else
            {
                bundle = bundleCacheItem.m_bundle;
            }

            // 获取asset名称
            //string assetName = GetAssetNameByPath(path);
            string assetName = Path.GetFileName(path);
            Debug.LogError(string.Format("Load Asset  Name : " + assetName));
            // 从bundle加载资源
            UnityEngine.Object[] allAssets;
            if (loadAync)
            {
                var assetReqIter = bundle.LoadAssetWithSubAssetsAsync(assetName);
                while (!assetReqIter.isDone)
                {
                    yield return null;
                }
                allAssets = assetReqIter.allAssets;

            }
            else
            {
                allAssets = bundle.LoadAssetWithSubAssets(assetName);
            }

            if (allAssets == null || allAssets.Length <= 0)
            {
                Debug.LogError(string.Format("LoadAssetFromBundle bundle.LoadAsset fail {0} {1}", assetName, assetBundleInfo.m_bundleName));

                onCompleted(path, null, null);
                yield break;
            }

            onCompleted(path, allAssets, bundleCacheItem);
        }

        /// <summary>
        /// LoadBundleEx
        /// </summary>
        /// <param name="assetBundleInfo"></param>
        /// <param name="loadAync"></param>
        /// <param name="onComplete"></param>
        /// <param name="bRoot">true: load dependency false:dont't load dependency</param>
        /// <returns></returns>
        protected IEnumerator LoadAssetBundle(SingleBundleInfo assetBundleInfo, bool loadAync, Action<AssetBundle, BundleCacheItem> onComplete, bool bRoot = true)
        {
            // 如果bundle已经加载，直接使用
            var bundleCatchItem = GetAssetBundleFromCache(assetBundleInfo.m_bundleName);
            if (bundleCatchItem != null)
            {
                if (onComplete != null)
                    onComplete(bundleCatchItem.m_bundle, bundleCatchItem);
                yield break;
            }

            // 注册一个加载现场
            BundleLoadingCtx bundleLoadingCtx;
            bool alreadyInLoading = RegBundleLoadingCtx(assetBundleInfo.m_bundleName, out bundleLoadingCtx);

            // 如果已经有一个加载现场处于加载中，等待加载完成
            if (alreadyInLoading)
            {
                var waitItor = WaitForBundleLoadComplete(assetBundleInfo, bundleLoadingCtx, loadAync, onComplete);
                while (waitItor.MoveNext())
                {
                    yield return null;
                }
                yield break;
            }

            float startWaitTime = Time.realtimeSinceStartup;

            // 加载bundle
            AssetBundle loadedBundle = null;
            var lbIter = LoadBundleFromWWWOrStreamingImpl(assetBundleInfo.m_bundleName, loadAync, (lbundle) => { loadedBundle = lbundle; });
            while (lbIter.MoveNext())
            {
                yield return null;
            }

            if (loadedBundle == null)
            {
                bundleLoadingCtx.m_isEnd = true;
                if (onComplete != null)
                    onComplete(null, null);
                // 注销加载现场
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }

            List<BundleCacheItem> dependBundleCacheList = null;
            if (bRoot)
            {
                // 获取所有的直接依赖
                var dependenceList = m_assetBundleManifest.GetAllDependencies(assetBundleInfo.m_bundleName);

                // 加载依赖的bundle
                if (dependenceList != null && dependenceList.Length != 0)
                {
                    foreach (string dependence in dependenceList)
                    {
#if UNITY_EDITOR && !RELEASE
                        if (dependenceList.Length >= 5)
                        {
                            Debug.LogWarning(string.Format("[ResourceManager]LoadBundle load dependence bundles {0} for {1}", dependence, assetBundleInfo.m_bundleName));
                        }
#endif
                        AssetBundle dependBundle = null;
                        BundleCacheItem dependCacheItem = null;
                        var iter = LoadBundleDependence(dependence, loadAync,
                            (lbundle, lbundleCache) => { dependBundle = lbundle; dependCacheItem = lbundleCache; });
                        while (iter.MoveNext())
                        {
                            yield return null;
                        }
                        if (dependBundle == null)
                        {
                            Debug.LogError(string.Format("LoadBundle fail by load dependence fail {0} ", assetBundleInfo.m_bundleName));
                        }
                        else
                        {
                            if (dependBundleCacheList == null)
                            {
                                dependBundleCacheList = new List<BundleCacheItem>();
                            }
                            dependCacheItem.AddRefrence();
                            dependBundleCacheList.Add(dependCacheItem);
                        }
                    }
                }
            }

            // bundle加载成功
            bundleLoadingCtx.m_isEnd = true;
            bundleLoadingCtx.m_bundle = loadedBundle;

            // 注册bundleCache
            bundleCatchItem = PushAssetBundleToCache(assetBundleInfo, loadedBundle);
            if (bRoot)
            {
                bundleCatchItem.m_dependBundleCacheList = dependBundleCacheList;
                bundleCatchItem.AddRefrence();
                bundleCatchItem.m_bRootLoad = true;
            }

            // 注销加载现场
            UnregBundleLoadingCtx(bundleLoadingCtx);

            // 自己和依赖都已经完成加载，通知调用者
            if (onComplete != null)
                onComplete(loadedBundle, bundleCatchItem);
        }

        /// <summary>
        /// 为了scene加载bundle
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="noErrlog"></param>
        /// <param name="loadAync"></param>
        /// <returns></returns>
        protected IEnumerator LoadBundle4UnityScene(string scenePath, Action<string, BundleCacheItem> onComplete, bool noErrlog, bool loadAync)
        {
            // 首先获取资源对应的bundle名称
            SingleBundleInfo bundleInfo = BundlesHelper.GetBundleInfoByAssetPath(scenePath);

            // 如果获取不到bundleData，直接失败
            if (bundleInfo == null)
            {
                Debug.LogError(string.Format("LoadBundle4UnityScene bundleData=null {0}", scenePath));
                onComplete(scenePath, null);
                yield break;
            }

            
            // 查找bundlecache
            AssetBundle bundle = null;
            var bundleCacheItem = GetAssetBundleFromCache(bundleInfo.m_bundleName);

            // 如果cache没有命中
            if (bundleCacheItem == null)
            {
                // 启动bundle加载
                var iter = LoadAssetBundle(bundleInfo, loadAync, (lbundle, lbundleCacheItem) => { bundle = lbundle; bundleCacheItem = lbundleCacheItem; });
                while (iter.MoveNext())
                {
                    yield return null;
                }
                if (bundle == null)
                {
                    if (!noErrlog) { Debug.LogError(string.Format("LoadBundle4UnityScene LoadBundle fail {0}", bundleInfo.m_bundleName)); }
                    else { Debug.Log(string.Format("LoadBundle4UnityScene LoadBundle fail {0}", bundleInfo.m_bundleName)); }

                    onComplete(scenePath, null);
                    yield break;
                }
            }

            //Debug.LogWarning(string.Format("LoadAssetFromBundle OK {0}", path));

            onComplete(scenePath, bundleCacheItem);
        }


        /// <summary>
        /// 等待assetbundle加载完成
        /// </summary>
        /// <param name="bundleData"></param>
        /// <param name="bundleLoadingCtx"></param>
        /// <param name="loadAync"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        protected IEnumerator WaitForBundleLoadComplete(SingleBundleInfo bundleInfo, BundleLoadingCtx bundleLoadingCtx,
            bool loadAync, Action<AssetBundle, BundleCacheItem> onComplete)
        {
            Debug.Log(string.Format("LoadBundle start wait for LoadingCtx {0}", bundleInfo.m_bundleName));
            while (!bundleLoadingCtx.m_isEnd)
            {
                if (!loadAync)
                {
                    AssetBundle assetBundle = null;
                    bool hasDone = false;

                    if (hasDone)
                    {
                        if (assetBundle == null)
                        {
                            if (onComplete != null)
                                onComplete(null, null);
                            // 注销加载现场
                            UnregBundleLoadingCtx(bundleLoadingCtx);
                            yield break;
                        }

                        bundleLoadingCtx.m_bundle = assetBundle;
                        if (onComplete != null)
                        {
                            BundleCacheItem bcItem = PushAssetBundleToCache(bundleInfo, assetBundle);
                            onComplete(bundleLoadingCtx.m_bundle, bcItem);
                        }

                        // 注销加载现场
                        UnregBundleLoadingCtx(bundleLoadingCtx);
                        yield break;
                    }
                }
                yield return null;
            }
            if (onComplete != null)
                onComplete(bundleLoadingCtx.m_bundle, GetAssetBundleFromCache(bundleInfo.m_bundleName));
            // 注销加载现场
            UnregBundleLoadingCtx(bundleLoadingCtx);
        }

        
        /// <summary>
        /// LoadBundle Dependence. It will not find the dependencies.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="loadAync"></param>
        /// <param name="onComplete"></param>
        /// <param name="noErrlog"></param>
        /// <returns></returns>
        protected IEnumerator LoadBundleDependence(string bundleName, bool loadAync, Action<AssetBundle, BundleCacheItem> onComplete)
        {
            var bundleData = BundlesHelper.GetBundleInfoByName(bundleName);
            if (bundleData == null)
            {
                Debug.LogError(string.Format("LoadBundle fail {0}", bundleName));
                onComplete(null, null);
                yield break;
            }
            //dependency load shouldn't load dependency, may will cause circle load
            var iter = LoadAssetBundle(bundleData, loadAync, onComplete, false);
            while (iter.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// 使用www或者LoadFromFileAsync加载bundle
        /// </summary>
        /// <param name="bundleData"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        protected IEnumerator LoadBundleFromWWWOrStreamingImpl(string bundleName, bool loadAync, Action<AssetBundle> onComplete)
        {
            AssetBundle loadedBundle = null;
            string assetBundlePath = $"{AssetBundleRootRuntime}/{bundleName}";
            float startWaitTime = Time.realtimeSinceStartup;
            // 异步加载
            if (loadAync)
            {
                //var req = AssetBundle.LoadFromFileAsync(path);
                var req = AssetBundle.LoadFromFileAsync(assetBundlePath, 0, 0);
                while (!req.isDone)
                {
                    yield return null;
                    if (Time.realtimeSinceStartup > startWaitTime + 20)
                    {
                        Debug.LogError(string.Format("LoadBundle Waiting LoadingCtx {0} time out! loadAync = {1}", bundleName, loadAync));
                        yield break;
                    }
                }
                if (req.assetBundle == null)
                {
                    Debug.LogError($"LoadBundleFromWWWOrStreamingImpl LoadFromFileAsync  fail {assetBundlePath}");
                    onComplete(null);
                    yield break;
                }
                loadedBundle = req.assetBundle;
            }
            else
            {
                // 同步加载
                loadedBundle = AssetBundle.LoadFromFile(assetBundlePath, 0, 0);
            }

            if (loadedBundle != null)
            {
                onComplete(loadedBundle);
                yield break;
            }

            onComplete(loadedBundle);
        }

        /// <summary>
        /// 进行一次包括asset的和bundle的卸载
        /// </summary>
        /// <param name="onComplete"></param>
        /// <param name="keepReserve"></param>
        public bool UnloadUnusedResourceAll(Action onComplete = null, HashSet<string> keepReserve = null)
        {
            // 如果还存在进行中的加载,不能释放资源
            if (m_loadingOpCount != 0)
            {
                Debug.LogWarning("UnloadUnusedResourceAll fail, m_loadingOpCount != 0" + m_loadingOpCount.ToString());
                m_waitingUnloadUnusedAssets = true;
                if (onComplete != null)
                {
                    onComplete();
                }
                return false;
            }

            UnloadUnusedAssets();
            m_waitingUnloadAllUnusedBundles = true;

            if (onComplete != null)
            {
                m_eventOnUnloadUnusedResourceAllCompleted += onComplete;
            }

            return true;
        }

        /// <summary>
        /// 是否正在等待资源卸载
        /// </summary>
        /// <returns></returns>
        public bool WaitingUnloading()
        {
            if (m_currUnloadUnusedAssetsOperation != null)
                return true;
            if (m_waitingUnloadUnusedAssets)
                return true;
            return false;
        }


        #region bundle 缓存

        /// <summary>
        /// 从cache获取bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected BundleCacheItem GetAssetBundleFromCache(string bundleName)
        {
            BundleCacheItem item;
            bool ret = m_bundleCacheDict.TryGetValue(bundleName, out item);
            return item;
        }

        /// <summary>
        /// 将bundle缓存到cache中
        /// </summary>
        /// <param name="singleData"></param>
        /// <param name="bundle"></param>
        protected BundleCacheItem PushAssetBundleToCache(SingleBundleInfo bundleInfo, AssetBundle bundle)
        {
            BundleCacheItem item;

            bool ret = m_bundleCacheDict.TryGetValue(bundleInfo.m_bundleName, out item);
            if (!ret)
            {
                item = new BundleCacheItem();
                item.m_bundleName = bundleInfo.m_bundleName;
                item.m_bundle = bundle;
                item.m_hitCount = 0;
                item.m_lastTouchTime = System.DateTime.Now;
                item.m_bundleInfo = bundleInfo;
                m_bundleCacheDict[bundleInfo.m_bundleName] = item;
            }
            return item;
        }

        /// <summary>
        /// 当缓存命中
        /// </summary>
        /// <param name="item"></param>
        protected void OnBundleCacheHit(BundleCacheItem item)
        {
            
        }

        #endregion

        #region 加载ctx

        /// <summary>
        /// 注册一个bundle加载现场
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="pipeCtx"></param>
        /// <returns></returns>
        protected bool RegBundleLoadingCtx(string bundleName, out BundleLoadingCtx ctx)
        {
            bool ret = m_bundleLoadingCtxDict.TryGetValue(bundleName, out ctx);
            if (!ret)
            {
                ctx = new BundleLoadingCtx();
                ctx.m_bundleName = bundleName;
                ctx.m_ref = 1;
                m_bundleLoadingCtxDict[bundleName] = ctx;
                return false;
            }
            else
            {
                ctx.m_ref++;
                return true;
            }
        }

        /// <summary>
        /// 注销加载现场
        /// </summary>
        /// <param name="bundleLoadingCtx"></param>
        protected void UnregBundleLoadingCtx(BundleLoadingCtx bundleLoadingCtx)
        {
            bundleLoadingCtx.m_ref--;
            if (bundleLoadingCtx.m_ref <= 0)
            {
                m_bundleLoadingCtxDict.Remove(bundleLoadingCtx.m_bundleName);
            }
        }

        /// <summary>
        /// bundle加载现场缓存
        /// </summary>
        protected Dictionary<string, BundleLoadingCtx> m_bundleLoadingCtxDict = new Dictionary<string, BundleLoadingCtx>();



        #endregion

        #region a

        /// <summary>
        /// bundle 的定时清理
        /// </summary>
        protected void TickBundle()
        {
            if (m_nextTickBundleTime > Time.realtimeSinceStartup)
            {
                return;
            }

            m_nextTickBundleTime = m_nextTickBundleTime + 15;

            UnloadAllUnusedBundles();
        }

        /// <summary>
        /// 检查卸载未使用bundle
        /// </summary>
        protected void CheckUnusedBundles()
        {
            // 如果需要卸载bundle
            if (m_waitingUnloadAllUnusedBundles)
            {
                // 卸载bundle
                if (UnloadAllUnusedBundles())
                {
                    m_waitingUnloadAllUnusedBundles = false;
                }

                // 发起卸载完成回调
                if (m_eventOnUnloadUnusedResourceAllCompleted != null)
                {
                    m_eventOnUnloadUnusedResourceAllCompleted();
                    m_eventOnUnloadUnusedResourceAllCompleted = null;
                }
            }
        }

        /// <summary>
        /// 清理无用资源
        /// </summary>
        protected bool UnloadUnusedAssets()
        {
            if (m_currUnloadUnusedAssetsOperation == null)
            {
                m_currUnloadUnusedAssetsOperation = Resources.UnloadUnusedAssets();
            }
            return true;
        }

        /// <summary>
        /// 卸载所有的bundle
        /// </summary>
        protected bool UnloadAllUnusedBundles()
        {
            // 如果还存在进行中的加载,不能释放资源
            if (m_loadingOpCount != 0)
            {
                Debug.LogWarning("UnloadAllUnusedBundles fail,  m_loadingOpCount != 0");
                return false;
            }

            m_bundleCachetoUnload.Clear();
            // 收集需要卸载的
            foreach (var pair in m_bundleCacheDict)
            {
                BundleCacheItem item = pair.Value;

                // 当ref不为0，以及 dontUnload， 不释放
                if (item.m_refCount > 0 || item.m_bundleInfo.m_isResident)
                {
                    continue;
                }

                m_bundleCachetoUnload.Add(item);
            }

            // 卸载所有需要卸载的bundle
            foreach (var item in m_bundleCachetoUnload)
            {
                // 为依赖的bundle解引用
                if (item.m_dependBundleCacheList != null)
                {
                    foreach (var dependBundleCacheItem in item.m_dependBundleCacheList)
                    {
                        dependBundleCacheItem.RemoveRefrence();
                    }
                }

                item.m_bundle.Unload(false);

                m_bundleCacheDict.Remove(item.m_bundleName);
            }

            m_bundleCachetoUnload.Clear();

            return true;
        }

        /// <summary>
        /// 正在进行的UnloadUnusedAssets
        /// </summary>
        protected AsyncOperation m_currUnloadUnusedAssetsOperation;

        
        #endregion

        /// <summary>
        /// 下一次tick bundle的时间
        /// </summary>
        protected float m_nextTickBundleTime = 0;

        /// <summary>
        /// bundle的缓存
        /// </summary>
        protected Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();

        /// <summary>
        /// 需要卸载的列表
        /// </summary>
        protected List<BundleCacheItem> m_bundleCachetoUnload = new List<BundleCacheItem>();


        /// <summary>
        /// 是否正在等待卸载Bundle
        /// 当读取途中接收到请求时，置该标记位并延后处理
        /// </summary>
        protected bool m_waitingUnloadAllUnusedBundles = false;

        /// <summary>
        /// 回调
        /// </summary>
        protected event Action m_eventOnUnloadUnusedResourceAllCompleted;

        #region 默认路径

        /// <summary>
        /// 运行时ab包路径
        /// </summary>
        public static string AssetBundleRootRuntime { get { return $"{Application.streamingAssetsPath}/AssetBundles"; } }

        /// <summary>
        /// resource下 meta
        /// </summary>
        public static string AssetBundleMetaNameInResources = $"BundleMetaInfo/BundleMetaDefault";

        #endregion

        #region Meta信息

        protected AssetBundleManifest m_assetBundleManifest;
        protected BundlesMetaInfo m_bundleMeta; 
        public BundlesMetaHelper BundlesHelper;


        #endregion
    }
}

