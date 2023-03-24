using My.Framework.Runtime.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace My.Framework.Editor.Build
{
    /// <summary>
    /// build 信息
    /// </summary>
    public class AssetBundleBuildData
    {
        public string msFolder;
        public string msBundleName = "";
        public string msVariant = "";
        public List<string> mAssetPaths = new List<string>(64);

        public BundleDescription mDesc;

        /// <summary>
        /// Add Asset into Asset path List
        /// </summary>
        /// <param name="path"></param>
        public void AddAssetPath(string path)
        {
            mAssetPaths.Add(path);
        }
    }

    public class AssetBuildManager
    {
        

        [MenuItem("My/AssetBuild/Build")]
        public static void TestBuildAssetBundles()
        {
            // 通过配置文件收集信息
            CollectAssetBundleInfo();

            // 生成元数据
            GenerateBundleMetaInfo();

            // 导出数据
            BuildAssetBundles();

            // 保存至指定目录
            ExportStreamingAssetFiles();
        }

        /// <summary>
        /// 通过bundle description 获取需要打包的路径
        /// </summary>
        /// <returns></returns>
        public static bool CollectAssetBundleInfo()
        {
            ClearBundleMap();

            var dirRoot = RuntimeAssetsPathInEditor;
            var bundleDirs = GetAllBundleDir(dirRoot);
            var unityScenePathList = GetUnityScenePathList(bundleDirs).ToList();


            var bundleDirsMaps = new Dictionary<string, int>();

            // 初始化bundleDirsMaps
            foreach (var path in bundleDirs)
            {
                bundleDirsMaps.Add(path, 0);
                var bundleDesc = TryGetBundleDescriptionByPath(path);
                if (bundleDesc == null) continue;
                FillBundleInfoByFolder(path, bundleDesc, ref unityScenePathList);
            }

            //刷新编辑器
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("CollectAssetBundleInfo completed");
            EditorUtility.ClearProgressBar();
            return true;
        }

        /// <summary>
        /// 通过之前收集的路径 构建部分元数据
        /// </summary>
        public static void GenerateBundleMetaInfo()
        {
            var metaPath = $"{ResourcesFolder}/{SimpleResourceManager.AssetBundleMetaNameInResources}.asset";
            // 尝试加载资源文件,或者创建一个
            var bundleData = AssetDatabase.LoadAssetAtPath(metaPath, typeof(BundlesMetaInfo)) as BundlesMetaInfo;
            if (bundleData == null)
            {
                bundleData = CreateScriptableObjectAsset<BundlesMetaInfo>(metaPath);
            }

            int countFiles = 0;

            // 通过AssetDatabase遍历所有的bundle,填充m_assetList
            foreach (var pBuildData in mAssetBundleBuildDict.Values)
            {
                string bundleName = pBuildData.msBundleName;

                SingleBundleInfo info = bundleData.m_bundleInfoList.Find((d) => (d.m_bundleName == bundleName && d.m_variant == pBuildData.msVariant));

                if (info == null)
                {
                    // 为一个bundle构造SingleBundleData
                    info = new SingleBundleInfo();

                    // 将SingleBundleData添加到列表中
                    bundleData.m_bundleInfoList.Add(info);
                }

                info.m_bundleName = bundleName;


                // 获取bundle不带后缀的名字和后缀
                // 使用其中的数据，填充bundledata
                if (pBuildData.mDesc != null)
                {
                    info.m_isResident = pBuildData.mDesc.mIsResident;
                    info.m_variant = pBuildData.mDesc.m_bundleVariantName;
                }

                // 获取一个bundle包含的所有资源
                List<string> tempAssetPathList = new List<string>();

                var assetPaths = pBuildData.mAssetPaths;
                foreach (var path in assetPaths)
                {
                    // .meta结尾,(DS_Store mac 下的系统文件),跳过
                    if (path.EndsWith(".meta") || path.EndsWith("DS_Store"))
                        continue;
                    if (path.StartsWith("~"))
                        continue;
                    string ext = Path.GetExtension(path);
                    if (ext.Equals(".prefab"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.Equals(".mat"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.Equals(".asset") || ext.Equals(".txt") || ext.Equals(".json"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.ToLower().Equals(".png") || ext.ToLower().Equals(".tga") || ext.ToLower().Equals(".jpg"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.ToLower().Equals(".playable"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.ToLower().Equals(".shader"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                    else if (ext.Equals(".bytes") || ext.Equals(".unity"))
                    {
                        tempAssetPathList.Add(path.Replace(Application.dataPath, "").Replace('\\', '/'));
                    }
                }

                // 为一个bundle填充asset包含数据
                info.m_assetList.Clear();
                info.m_assetList.AddRange(tempAssetPathList);

                info.m_assetList.Sort();
                countFiles += tempAssetPathList.Count;
            }
            // 移除所有assetdatabase中不再存在的bundle
            System.Diagnostics.Debug.Assert(bundleData != null, "bundleData != null");
            List<string> tempBundleList = new List<string>();
            foreach (AssetBundleBuildData dd in mAssetBundleBuildDict.Values)
            {
                tempBundleList.Add(dd.msBundleName);
            }
            bundleData.m_bundleInfoList.RemoveAll((data) => !tempBundleList.Contains(data.m_bundleName));

            // 对列表排序,避免排序不稳定带来的内容变化
            bundleData.m_bundleInfoList.Sort((b1, b2) => { return string.CompareOrdinal(b1.m_bundleName, b2.m_bundleName); });

            // 保存
            EditorUtility.SetDirty(bundleData);
            AssetDatabase.SaveAssets();

            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("GenerateBundleMetaInfo completed: " + countFiles);
        }


        public static bool BuildAssetBundles()
        {
            if (!Directory.Exists(AssetBundlesBuildOutputPathWithVersion))
            {
                Directory.CreateDirectory(AssetBundlesBuildOutputPathWithVersion);
            }
            mBundleBuilds.Clear();
            foreach (AssetBundleBuildData pData in mAssetBundleBuildDict.Values)
            {
                if (pData.mAssetPaths.Count > 0)
                {
                    AssetBundleBuild pBuild = new AssetBundleBuild();
                    pBuild.assetBundleName = pData.msBundleName;
                    pBuild.assetNames = pData.mAssetPaths.ToArray();
                    pBuild.assetBundleVariant = pData.msVariant;
                    mBundleBuilds.Add(pBuild);
                }
            }
            // 刷新编辑器
            //AssetDatabase.Refresh();
            // ABENCRYPT

            mAssetBundleManifest = BuildPipeline.BuildAssetBundles(AssetBundlesBuildOutputPathWithVersion, mBundleBuilds.ToArray(),
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);

            if (mAssetBundleManifest == null || mAssetBundleManifest.GetAllAssetBundles() == null || mAssetBundleManifest.GetAllAssetBundles().Length == 0)
            {
                UnityEngine.Debug.LogErrorFormat("BuildPipeline.BuildAssetBundles() {0}, {1}",
                    AssetBundlesBuildOutputPathWithVersion, EditorUserBuildSettings.activeBuildTarget);
                return false;
            }

            // 刷新编辑器
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("BuildAssetBundles complete");

            return true;
        }

        private static AssetBundleManifest mAssetBundleManifest;
        public static bool ExportStreamingAssetFiles()
        {
            // 搜集需要导出的文件列表
            List<string> bundleFiles2Copy = new List<string>();

            var metaPath = $"{ResourcesFolder}/{SimpleResourceManager.AssetBundleMetaNameInResources}.asset";
            // 清理掉bundledata的bundle标记
            var importer = AssetImporter.GetAtPath(metaPath);
            importer.assetBundleName = null;


            // 加载bundledata，计算文件列表
            BundlesMetaInfo bundleData = AssetDatabase.LoadAssetAtPath(metaPath, typeof(BundlesMetaInfo)) as BundlesMetaInfo;
            foreach (var bundle in bundleData.m_bundleInfoList)
            {
                string fullFileName = string.Format("{0}/{1}", AssetBundlesBuildOutputPathWithVersion, bundle.m_bundleName);
                if (bundle.m_variant != string.Empty)
                {
                    fullFileName = $"{fullFileName}.{bundle.m_variant.ToLower()}";
                }
                bundleFiles2Copy.Add(fullFileName);
            }

            // 复制menifest文件
            bundleFiles2Copy.Add(string.Format("{0}/{1}", AssetBundlesBuildOutputPathWithVersion, "Default"));

            // 输出文件到StreamAsset
            if (!Directory.Exists(StreamingAssetsBundlePath))
            {
                Directory.CreateDirectory(StreamingAssetsBundlePath);
            }

            foreach (var item in bundleFiles2Copy)
            {
                string targetPath = string.Format("{0}/{1}", StreamingAssetsBundlePath, Path.GetFileName(item));
                if (File.Exists(item))
                {
                    File.Copy(item, targetPath, true);
                    UnityEngine.Debug.Log(string.Format("Copy {0} => {1}", item, targetPath));
                }
                else
                    UnityEngine.Debug.LogError(string.Format("Copy {0} => {1} fail", item, targetPath));
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("ExportStreamingAssetFiles completed");

            return true;
        }

        /// <summary>
        /// Create one bundle data with folder name and bundle name
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="bundleName"></param>
        /// <param name="variants"></param>
        /// <param name="isResave"></param>
        private static AssetBundleBuildData CreateAssetBundleData(string folderName, string bundleName, string variantName)
        {
            folderName = folderName.Replace("\\", "/");
            if (!mAssetBundleBuildDict.ContainsKey(folderName))
            {
                AssetBundleBuildData pData = new AssetBundleBuildData();
                pData.msFolder = folderName;
                pData.msBundleName = bundleName;
                pData.msVariant = variantName;
                mAssetBundleBuildDict.Add(folderName, pData);
                return pData;
            }
            else
            {
                Debug.LogError("Already Exsit: " + folderName);
                return null;
            }
        }

        /// <summary>
        /// 通过文件夹名 搞
        /// </summary>
        /// <param name="path"></param>
        private static void FillBundleInfoByFolder(string path, BundleDescription bundleDesc, ref List<string> unityScenePathList)
        {
            // 获取bundle名字和bundle变体的名字
            GetBundleNameAndVariantFromBundlePath(path, bundleDesc, out var bundleName, out var bundleVariant);

            // 创建bundle
            AssetBundleBuildData pBundleData = CreateAssetBundleData(path, bundleName, bundleVariant);
            pBundleData.mDesc = bundleDesc;

            // 遍历每一个文件，给必要的文件设置bundle name
            string[] fileNames = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToArray();
            fileNames = PathHelper.ReformPathStrings(fileNames).ToArray();
            foreach (string name in fileNames)
            {
                // 跳过不需要设置的文件
                if (IsFileSkipBundleInfoUpdateInAssetDatabase(name, unityScenePathList))
                {
                    continue;
                }

                AddAssetPathBundleInfo(path, name);
            }
        }

        /// <summary>
        /// 获取所有需要打成bundle的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<string> GetAllBundleDir(string path)
        {
            List<string> bundleDirs = new List<string>();

            var bundleDescriptionFiles = Directory.GetFiles(path,
                string.Format("{0}.asset", BundleDescription.BundleDescriptionAssetName),
                SearchOption.AllDirectories);

            foreach (string bundleDescriptionFile in bundleDescriptionFiles)
            {
                string bundleDir = Path.GetDirectoryName(bundleDescriptionFile);
                if (!string.IsNullOrEmpty(bundleDir))
                    bundleDirs.Add(PathHelper.ReformPathString(bundleDir));
            }

            // 检查路径，去掉非法的路径
            CheckBundleDirs(bundleDirs);

            return bundleDirs;
        }


        public static IEnumerable<string> GetUnityScenePathList(IEnumerable<string> bundleDirs)
        {
            foreach (var path in bundleDirs)
            {
                foreach (var name in Directory.GetFiles(path, "*.unity", SearchOption.AllDirectories))
                {
                    if (!name.EndsWith(".unity"))
                        continue;
                    var unityScenePath = Path.GetDirectoryName(name);
                    yield return PathHelper.ReformPathString(unityScenePath);
                }
            }
        }



        /// <summary>
        /// 检查bundleDir有没有不合法的情况
        /// </summary>
        /// <param name="bundleDirs"></param>
        private static void CheckBundleDirs(List<string> bundleDirs)
        {
            HashSet<string> illegalBundleDirs = new HashSet<string>();
            // 如果某个路径被其他路径包含，则为非法
            foreach (string bundleDir in bundleDirs)
            {
                foreach (string compareStr in bundleDirs)
                {
                    int index = compareStr.LastIndexOf("/", StringComparison.Ordinal);
                    string subStr = index > 0 ? compareStr.Substring(0, index) : compareStr;
                    if (bundleDir != compareStr && subStr.Contains(bundleDir))
                    {
                        illegalBundleDirs.Add(bundleDir);
                        illegalBundleDirs.Add(compareStr);
                        Debug.LogError(string.Format("CheckBundleDirs Error: bundle dir: {0} contains bundle dir: {1}",
                            compareStr, bundleDir));
                    }
                }
            }

            // 移除非法路径
            foreach (string illegalBundleDir in illegalBundleDirs)
            {
                bundleDirs.Remove(illegalBundleDir);
            }
        }


        /// <summary>
        /// Clean Bundle map
        /// </summary>
        private static void ClearBundleMap()
        {
            mBundleBuilds.Clear();
            mAssetBundleBuildDict.Clear();
        }

        static List<AssetBundleBuild> mBundleBuilds = new List<AssetBundleBuild>();
        static Dictionary<string, AssetBundleBuildData> mAssetBundleBuildDict = new Dictionary<string, AssetBundleBuildData>();

        /// <summary>
        /// 文件是否跳过bundle信息更新
        /// </summary>
        /// <returns></returns>
        private static bool IsFileSkipBundleInfoUpdateInAssetDatabase(string fileName,
            List<string> unityScenePathList)
        {
            // .meta结尾,(DS_Store mac 下的系统文件),跳过
            if (fileName.EndsWith(".meta") || fileName.EndsWith("DS_Store"))
            {
                return true;
            }

            // 与.unity文件在同一个目录下的所有非.unity文件，也同样跳过，
            // 因为默认会被打入scene的bundle,而.unity文件是需要设置的
            if (!fileName.EndsWith(".unity") && unityScenePathList != null &&
                unityScenePathList.Any(unityScenePath => fileName.Contains(unityScenePath)))
            {
                return true;
            }

            // 跳出bundleDesc本身
            if (fileName.EndsWith(string.Format("{0}.asset", BundleDescription.BundleDescriptionAssetName)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// add assetpath into bundle build map
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="folderName"></param>
        private static void AddAssetPathBundleInfo(string folderName, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(folderName))
            {
                Debug.LogError(string.Format("string.IsNullOrEmpty{0} || string.IsNullOrEmpty{1}", assetPath, folderName));
                return;
            }
            AddAssetBundleBuild(folderName, assetPath);
        }

        /// <summary>
        /// Add asset into Bundle Data which's key is foldername
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="assetPath"></param>
        private static void AddAssetBundleBuild(string folderName, string assetPath)
        {
            if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(assetPath))
                return;
            folderName = PathHelper.ReformPathString(folderName);
            assetPath = PathHelper.ReformPathString(assetPath);
            if (mAssetBundleBuildDict.ContainsKey(folderName))
            {
                mAssetBundleBuildDict[folderName].AddAssetPath(assetPath);
            }
            else
            {
                Debug.LogError("Not Exsit: " + folderName);
            }
        }

        #region path

        /// <summary>
        /// 从指定路径获取bundle名称和变体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bundleDesc"></param>
        /// <param name="bundleName"></param>
        /// <param name="bundleVariant"></param>
        private static void GetBundleNameAndVariantFromBundlePath(string path,
            BundleDescription bundleDesc,
            out string bundleName, out string bundleVariant)
        {
            string abbreviateBundlePath = RemoveRootPathForBundleName(path, RuntimeAssetsPathInEditor);

            bundleName = GetBundleNameByAssetPath(abbreviateBundlePath,
                bundleDesc.m_replaceLastFolderNameStr);
            bundleVariant = bundleDesc.m_bundleVariantName;

            // string realBundleName;
            if (!string.IsNullOrEmpty(bundleDesc.m_bundleVariantName))
            {
                bundleName = string.Format("{0}.{1}", bundleName, bundleVariant.ToLower());
            }
        }

        /// <summary>
        /// 从bundle名字中去掉根节点的名称，避免bundle文件名字太长造成文件无法拷贝的问题
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        private static string RemoveRootPathForBundleName(string bundleName, string rootPath)
        {
            int index = bundleName.IndexOf(rootPath);
            if (index >= 0)
            {
                int startIndex = index + rootPath.Length + 1;
                string result = bundleName.Substring(startIndex, bundleName.Length - startIndex);
                return result;
            }
            return bundleName;
        }

        /// <summary>
        /// 尝试获取路径下的bundleDescription，可以为空
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static BundleDescription TryGetBundleDescriptionByPath(string path)
        {
            string[] fileNames = Directory.GetFiles(path,
                string.Format("{0}.asset", BundleDescription.BundleDescriptionAssetName),
                SearchOption.TopDirectoryOnly);

            foreach (string name in fileNames)
            {
                BundleDescription bundleDesc = AssetDatabase.LoadAssetAtPath<BundleDescription>(name);
                if (bundleDesc != null)
                    return bundleDesc;
            }
            return null;
        }

        /// <summary>
        /// 获取路径对应的bundlename
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replaceLastFolderNameStr"></param>
        /// <param name="autoExtension"></param>
        /// <returns></returns>
        public static string GetBundleNameByAssetPath(string path, string replaceLastFolderNameStr = "")
        {

            // 如果需要替换资源路径中的最后一个目录名字，则替换
            if (!string.IsNullOrEmpty(replaceLastFolderNameStr))
            {
                path = ReplaceLastFolderName(path, replaceLastFolderNameStr);
            }

            path = path.Replace("/", "_");
            if (path.Contains("\\\\"))
                path = path.Replace("\\\\", "_");
            if (path.Contains("\\"))
                path = path.Replace("\\", "_");
            if (path.Contains("."))
                path = path.Replace(".", "_");
            return path.ToLower();
        }

        /// <summary>
        /// 替换路径中最后一个目录的名字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replaceStr"></param>
        /// <returns></returns>
        public static string ReplaceLastFolderName(string path, string replaceStr)
        {
            path = path.Replace("\\", "/");
            path = path.Replace("//", "/");

            int lastIndex = path.LastIndexOf('/');
            if (lastIndex == -1)
                return path;

            string subString = path.Substring(0, lastIndex + 1);
            return string.Format("{0}{1}", subString, replaceStr);
        }


        /// <summary>
        /// BundleMeta 保存路径
        /// </summary>
        public static string ResourcesFolder
        {
            get
            {
                return $"Assets/Resources/";
            }
        }

        /// <summary>
        /// AssetBundle 资源根路径
        /// </summary>
        public static string RuntimeAssetsPathInEditor = "Assets/RuntimeAssets";
        

        /// <summary>
        /// AssetBundles build的时候的输出路径
        /// </summary>
        public static string AssetBundlesBuildOutputPath = "AssetBundles";
        /// <summary>
        /// AssetBundles build的时候的输出路径 加上平台信息
        /// </summary>
        public static string AssetBundlesBuildOutputPathWithVersion
        {
            get
            {
                return AssetBundlesBuildOutputPath + "/" + "Default";
            }
        }
        /// <summary>
        /// streamingAssets中bundle的基本路径
        /// </summary>
        public static string StreamingAssetsBundlePath
        {
            get
            {
                return string.Format("{0}/AssetBundles", Application.streamingAssetsPath);
            }
        }


        #endregion

        #region 临时工具

        /// <summary>
        /// 创建一个从ScriptableObject扩展的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullAssetPath">带有后缀名的路径</param>
        static public T CreateScriptableObjectAsset<T>(string fullAssetPath) where T : ScriptableObject
        {
            if (!Directory.Exists(Path.GetDirectoryName(fullAssetPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullAssetPath));
            }
            ScriptableObject asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullAssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath(fullAssetPath, typeof(T)) as T;
        }

        #endregion
    }
}



