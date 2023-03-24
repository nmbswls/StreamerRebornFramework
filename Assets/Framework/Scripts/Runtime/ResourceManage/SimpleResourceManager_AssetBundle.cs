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
                    return; // ���Ǳ���bundle
                }

                var bundleNameWithoutExt = Path.GetFileNameWithoutExtension(elem.m_bundleName);

                if (!m_variantBundleInfoDict.TryGetValue(bundleNameWithoutExt, out variantBundleInfoDict))
                {
                    // bundleNameȥ����׺��ΪKey
                    variantBundleInfoDict = new Dictionary<string, SingleBundleInfo>();
                    m_variantBundleInfoDict[bundleNameWithoutExt] = variantBundleInfoDict;
                }
                // ����ÿ�������Զ�Ӧ��BundleData
                variantBundleInfoDict[elem.m_variant.ToLower()] = elem;
            });
        }

        /// <summary>
        /// ͨ��bundlename��ȡBundleData
        /// </summary>
        /// <param name="name"></param>
        /// <param name="replaceByLocalization">�Ƿ��滻Ϊ��ǰ���õĶ����Ե�BundleData</param>
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
        /// ͨ����Դ·����ȡbundledata
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
        /// ��õ�ǰ��������� bundleData
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
        /// ���� - bundleInfo
        /// </summary>
        private Dictionary<string, SingleBundleInfo> m_bundleDataDict = new Dictionary<string, SingleBundleInfo>();

        /// <summary>
        /// ��Դ·�� - bundleInfo
        /// </summary>
        private Dictionary<string, SingleBundleInfo> m_assetPath2BundleInfoDict = new Dictionary<string, SingleBundleInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// BundleName�ͱ���BundleInfo��Ӧ��
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, SingleBundleInfo>> m_variantBundleInfoDict =
            new Dictionary<string, Dictionary<string, SingleBundleInfo>>();
    }


    public static class PathHelper
    {
        /// <summary>
        /// ����·��
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
        /// �淶��·���ַ���
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
            public DateTime m_lastTouchTime;     // ��һ�εķ���ʱ��
            public DateTime m_timeOutTime;       // ����ʱ��
            public int m_hitCount;                      // ���д���
            public int m_refCount;                      // ���������õĴ���
            public AssetBundle m_bundle;                // �������bundle
            public SingleBundleInfo m_bundleInfo;
            public bool m_bRootLoad = false;
            /// <summary>
            /// ������
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
        /// ��ʼ����StreamingAssetsFiles,����Ҫ��bundle���ص�cache
        /// </summary>
        public IEnumerator InitializeAssetBundle(string versionName, Action<bool> onEnd)
        {
            Debug.Log("InitializeAssetBundle Load ");

            // ����bundle
            AssetBundle mainManifestBundle = null;
            var lbIter = LoadBundleFromWWWOrStreamingImpl(versionName, false, (lbundle) => { mainManifestBundle = lbundle; });
            yield return lbIter;

            // û������Դ�� ����
            if (mainManifestBundle == null)
            {
                yield break;
            }

            // ����AssetBundleManifest
            var assets = mainManifestBundle.LoadAllAssets<AssetBundleManifest>();
            if (assets.Length <= 0)
            {
                Debug.LogError("BundleDataLoadingWorker can not find AssetBundleManifest in bundle ");
                yield break;
            }

            m_assetBundleManifest = assets[0];

            // �ͷ�bundle
            mainManifestBundle.Unload(false);

            m_bundleMeta = Resources.Load(AssetBundleMetaNameInResources) as BundlesMetaInfo;
            BundlesHelper = new BundlesMetaHelper(m_bundleMeta);

            Inited = true;

            //LoadResidentAssetBundle();
        }

        /// <summary>
        /// ���س�פ�ڴ��AB
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
        /// ��bundle������Դ
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        protected IEnumerator LoadAssetFromBundle(string path, Action<string, UnityEngine.Object[], BundleCacheItem> onCompleted, bool loadAync = false, bool mayMiss = false)
        {
            // ���Ȼ�ȡ��Դ��Ӧ��bundle����
            var assetBundleInfo = BundlesHelper.GetBundleInfoByAssetPath(path);

            // �����ȡ����bundleData��ֱ��ʧ��
            if (assetBundleInfo == null)
            {
                if (!mayMiss)
                {
                    //Debug.LogError(string.Format("LoadAssetFromBundle bundleData=null {0}", path));
                }
                onCompleted(path, null, null);
                yield break;
            }

            // ����bundlecache
            AssetBundle bundle = null;
            var bundleCacheItem = GetAssetBundleFromCache(assetBundleInfo.m_bundleName);

            // ���cacheû������
            if (bundleCacheItem == null)
            {
                // ����bundle����
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

            // ��ȡasset����
            //string assetName = GetAssetNameByPath(path);
            string assetName = Path.GetFileName(path);
            Debug.LogError(string.Format("Load Asset  Name : " + assetName));
            // ��bundle������Դ
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
            // ���bundle�Ѿ����أ�ֱ��ʹ��
            var bundleCatchItem = GetAssetBundleFromCache(assetBundleInfo.m_bundleName);
            if (bundleCatchItem != null)
            {
                if (onComplete != null)
                    onComplete(bundleCatchItem.m_bundle, bundleCatchItem);
                yield break;
            }

            // ע��һ�������ֳ�
            BundleLoadingCtx bundleLoadingCtx;
            bool alreadyInLoading = RegBundleLoadingCtx(assetBundleInfo.m_bundleName, out bundleLoadingCtx);

            // ����Ѿ���һ�������ֳ����ڼ����У��ȴ��������
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

            // ����bundle
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
                // ע�������ֳ�
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }

            List<BundleCacheItem> dependBundleCacheList = null;
            if (bRoot)
            {
                // ��ȡ���е�ֱ������
                var dependenceList = m_assetBundleManifest.GetAllDependencies(assetBundleInfo.m_bundleName);

                // ����������bundle
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

            // bundle���سɹ�
            bundleLoadingCtx.m_isEnd = true;
            bundleLoadingCtx.m_bundle = loadedBundle;

            // ע��bundleCache
            bundleCatchItem = PushAssetBundleToCache(assetBundleInfo, loadedBundle);
            if (bRoot)
            {
                bundleCatchItem.m_dependBundleCacheList = dependBundleCacheList;
                bundleCatchItem.AddRefrence();
                bundleCatchItem.m_bRootLoad = true;
            }

            // ע�������ֳ�
            UnregBundleLoadingCtx(bundleLoadingCtx);

            // �Լ����������Ѿ���ɼ��أ�֪ͨ������
            if (onComplete != null)
                onComplete(loadedBundle, bundleCatchItem);
        }

        /// <summary>
        /// Ϊ��scene����bundle
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="noErrlog"></param>
        /// <param name="loadAync"></param>
        /// <returns></returns>
        protected IEnumerator LoadBundle4UnityScene(string scenePath, Action<string, BundleCacheItem> onComplete, bool noErrlog, bool loadAync)
        {
            // ���Ȼ�ȡ��Դ��Ӧ��bundle����
            SingleBundleInfo bundleInfo = BundlesHelper.GetBundleInfoByAssetPath(scenePath);

            // �����ȡ����bundleData��ֱ��ʧ��
            if (bundleInfo == null)
            {
                Debug.LogError(string.Format("LoadBundle4UnityScene bundleData=null {0}", scenePath));
                onComplete(scenePath, null);
                yield break;
            }

            
            // ����bundlecache
            AssetBundle bundle = null;
            var bundleCacheItem = GetAssetBundleFromCache(bundleInfo.m_bundleName);

            // ���cacheû������
            if (bundleCacheItem == null)
            {
                // ����bundle����
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
        /// �ȴ�assetbundle�������
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
                            // ע�������ֳ�
                            UnregBundleLoadingCtx(bundleLoadingCtx);
                            yield break;
                        }

                        bundleLoadingCtx.m_bundle = assetBundle;
                        if (onComplete != null)
                        {
                            BundleCacheItem bcItem = PushAssetBundleToCache(bundleInfo, assetBundle);
                            onComplete(bundleLoadingCtx.m_bundle, bcItem);
                        }

                        // ע�������ֳ�
                        UnregBundleLoadingCtx(bundleLoadingCtx);
                        yield break;
                    }
                }
                yield return null;
            }
            if (onComplete != null)
                onComplete(bundleLoadingCtx.m_bundle, GetAssetBundleFromCache(bundleInfo.m_bundleName));
            // ע�������ֳ�
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
        /// ʹ��www����LoadFromFileAsync����bundle
        /// </summary>
        /// <param name="bundleData"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        protected IEnumerator LoadBundleFromWWWOrStreamingImpl(string bundleName, bool loadAync, Action<AssetBundle> onComplete)
        {
            AssetBundle loadedBundle = null;
            string assetBundlePath = $"{AssetBundleRootRuntime}/{bundleName}";
            float startWaitTime = Time.realtimeSinceStartup;
            // �첽����
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
                // ͬ������
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
        /// ����һ�ΰ���asset�ĺ�bundle��ж��
        /// </summary>
        /// <param name="onComplete"></param>
        /// <param name="keepReserve"></param>
        public bool UnloadUnusedResourceAll(Action onComplete = null, HashSet<string> keepReserve = null)
        {
            // ��������ڽ����еļ���,�����ͷ���Դ
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
        /// �Ƿ����ڵȴ���Դж��
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


        #region bundle ����

        /// <summary>
        /// ��cache��ȡbundle
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
        /// ��bundle���浽cache��
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
        /// ����������
        /// </summary>
        /// <param name="item"></param>
        protected void OnBundleCacheHit(BundleCacheItem item)
        {
            
        }

        #endregion

        #region ����ctx

        /// <summary>
        /// ע��һ��bundle�����ֳ�
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
        /// ע�������ֳ�
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
        /// bundle�����ֳ�����
        /// </summary>
        protected Dictionary<string, BundleLoadingCtx> m_bundleLoadingCtxDict = new Dictionary<string, BundleLoadingCtx>();



        #endregion

        #region a

        /// <summary>
        /// bundle �Ķ�ʱ����
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
        /// ���ж��δʹ��bundle
        /// </summary>
        protected void CheckUnusedBundles()
        {
            // �����Ҫж��bundle
            if (m_waitingUnloadAllUnusedBundles)
            {
                // ж��bundle
                if (UnloadAllUnusedBundles())
                {
                    m_waitingUnloadAllUnusedBundles = false;
                }

                // ����ж����ɻص�
                if (m_eventOnUnloadUnusedResourceAllCompleted != null)
                {
                    m_eventOnUnloadUnusedResourceAllCompleted();
                    m_eventOnUnloadUnusedResourceAllCompleted = null;
                }
            }
        }

        /// <summary>
        /// ����������Դ
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
        /// ж�����е�bundle
        /// </summary>
        protected bool UnloadAllUnusedBundles()
        {
            // ��������ڽ����еļ���,�����ͷ���Դ
            if (m_loadingOpCount != 0)
            {
                Debug.LogWarning("UnloadAllUnusedBundles fail,  m_loadingOpCount != 0");
                return false;
            }

            m_bundleCachetoUnload.Clear();
            // �ռ���Ҫж�ص�
            foreach (var pair in m_bundleCacheDict)
            {
                BundleCacheItem item = pair.Value;

                // ��ref��Ϊ0���Լ� dontUnload�� ���ͷ�
                if (item.m_refCount > 0 || item.m_bundleInfo.m_isResident)
                {
                    continue;
                }

                m_bundleCachetoUnload.Add(item);
            }

            // ж��������Ҫж�ص�bundle
            foreach (var item in m_bundleCachetoUnload)
            {
                // Ϊ������bundle������
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
        /// ���ڽ��е�UnloadUnusedAssets
        /// </summary>
        protected AsyncOperation m_currUnloadUnusedAssetsOperation;

        
        #endregion

        /// <summary>
        /// ��һ��tick bundle��ʱ��
        /// </summary>
        protected float m_nextTickBundleTime = 0;

        /// <summary>
        /// bundle�Ļ���
        /// </summary>
        protected Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();

        /// <summary>
        /// ��Ҫж�ص��б�
        /// </summary>
        protected List<BundleCacheItem> m_bundleCachetoUnload = new List<BundleCacheItem>();


        /// <summary>
        /// �Ƿ����ڵȴ�ж��Bundle
        /// ����ȡ;�н��յ�����ʱ���øñ��λ���Ӻ���
        /// </summary>
        protected bool m_waitingUnloadAllUnusedBundles = false;

        /// <summary>
        /// �ص�
        /// </summary>
        protected event Action m_eventOnUnloadUnusedResourceAllCompleted;

        #region Ĭ��·��

        /// <summary>
        /// ����ʱab��·��
        /// </summary>
        public static string AssetBundleRootRuntime { get { return $"{Application.streamingAssetsPath}/AssetBundles"; } }

        /// <summary>
        /// resource�� meta
        /// </summary>
        public static string AssetBundleMetaNameInResources = $"BundleMetaInfo/BundleMetaDefault";

        #endregion

        #region Meta��Ϣ

        protected AssetBundleManifest m_assetBundleManifest;
        protected BundlesMetaInfo m_bundleMeta; 
        public BundlesMetaHelper BundlesHelper;


        #endregion
    }
}

