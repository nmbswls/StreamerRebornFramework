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
    /// build ��Ϣ
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
            // ͨ�������ļ��ռ���Ϣ
            CollectAssetBundleInfo();

            // ����Ԫ����
            GenerateBundleMetaInfo();

            // ��������
            BuildAssetBundles();

            // ������ָ��Ŀ¼
            ExportStreamingAssetFiles();
        }

        /// <summary>
        /// ͨ��bundle description ��ȡ��Ҫ�����·��
        /// </summary>
        /// <returns></returns>
        public static bool CollectAssetBundleInfo()
        {
            ClearBundleMap();

            var dirRoot = RuntimeAssetsPathInEditor;
            var bundleDirs = GetAllBundleDir(dirRoot);
            var unityScenePathList = GetUnityScenePathList(bundleDirs).ToList();


            var bundleDirsMaps = new Dictionary<string, int>();

            // ��ʼ��bundleDirsMaps
            foreach (var path in bundleDirs)
            {
                bundleDirsMaps.Add(path, 0);
                var bundleDesc = TryGetBundleDescriptionByPath(path);
                if (bundleDesc == null) continue;
                FillBundleInfoByFolder(path, bundleDesc, ref unityScenePathList);
            }

            //ˢ�±༭��
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("CollectAssetBundleInfo completed");
            EditorUtility.ClearProgressBar();
            return true;
        }

        /// <summary>
        /// ͨ��֮ǰ�ռ���·�� ��������Ԫ����
        /// </summary>
        public static void GenerateBundleMetaInfo()
        {
            var metaPath = $"{ResourcesFolder}/{SimpleResourceManager.AssetBundleMetaNameInResources}.asset";
            // ���Լ�����Դ�ļ�,���ߴ���һ��
            var bundleData = AssetDatabase.LoadAssetAtPath(metaPath, typeof(BundlesMetaInfo)) as BundlesMetaInfo;
            if (bundleData == null)
            {
                bundleData = CreateScriptableObjectAsset<BundlesMetaInfo>(metaPath);
            }

            int countFiles = 0;

            // ͨ��AssetDatabase�������е�bundle,���m_assetList
            foreach (var pBuildData in mAssetBundleBuildDict.Values)
            {
                string bundleName = pBuildData.msBundleName;

                SingleBundleInfo info = bundleData.m_bundleInfoList.Find((d) => (d.m_bundleName == bundleName && d.m_variant == pBuildData.msVariant));

                if (info == null)
                {
                    // Ϊһ��bundle����SingleBundleData
                    info = new SingleBundleInfo();

                    // ��SingleBundleData��ӵ��б���
                    bundleData.m_bundleInfoList.Add(info);
                }

                info.m_bundleName = bundleName;


                // ��ȡbundle������׺�����ֺͺ�׺
                // ʹ�����е����ݣ����bundledata
                if (pBuildData.mDesc != null)
                {
                    info.m_isResident = pBuildData.mDesc.mIsResident;
                    info.m_variant = pBuildData.mDesc.m_bundleVariantName;
                }

                // ��ȡһ��bundle������������Դ
                List<string> tempAssetPathList = new List<string>();

                var assetPaths = pBuildData.mAssetPaths;
                foreach (var path in assetPaths)
                {
                    // .meta��β,(DS_Store mac �µ�ϵͳ�ļ�),����
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

                // Ϊһ��bundle���asset��������
                info.m_assetList.Clear();
                info.m_assetList.AddRange(tempAssetPathList);

                info.m_assetList.Sort();
                countFiles += tempAssetPathList.Count;
            }
            // �Ƴ�����assetdatabase�в��ٴ��ڵ�bundle
            System.Diagnostics.Debug.Assert(bundleData != null, "bundleData != null");
            List<string> tempBundleList = new List<string>();
            foreach (AssetBundleBuildData dd in mAssetBundleBuildDict.Values)
            {
                tempBundleList.Add(dd.msBundleName);
            }
            bundleData.m_bundleInfoList.RemoveAll((data) => !tempBundleList.Contains(data.m_bundleName));

            // ���б�����,���������ȶ����������ݱ仯
            bundleData.m_bundleInfoList.Sort((b1, b2) => { return string.CompareOrdinal(b1.m_bundleName, b2.m_bundleName); });

            // ����
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
            // ˢ�±༭��
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

            // ˢ�±༭��
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("BuildAssetBundles complete");

            return true;
        }

        private static AssetBundleManifest mAssetBundleManifest;
        public static bool ExportStreamingAssetFiles()
        {
            // �Ѽ���Ҫ�������ļ��б�
            List<string> bundleFiles2Copy = new List<string>();

            var metaPath = $"{ResourcesFolder}/{SimpleResourceManager.AssetBundleMetaNameInResources}.asset";
            // �����bundledata��bundle���
            var importer = AssetImporter.GetAtPath(metaPath);
            importer.assetBundleName = null;


            // ����bundledata�������ļ��б�
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

            // ����menifest�ļ�
            bundleFiles2Copy.Add(string.Format("{0}/{1}", AssetBundlesBuildOutputPathWithVersion, "Default"));

            // ����ļ���StreamAsset
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
        /// ͨ���ļ����� ��
        /// </summary>
        /// <param name="path"></param>
        private static void FillBundleInfoByFolder(string path, BundleDescription bundleDesc, ref List<string> unityScenePathList)
        {
            // ��ȡbundle���ֺ�bundle���������
            GetBundleNameAndVariantFromBundlePath(path, bundleDesc, out var bundleName, out var bundleVariant);

            // ����bundle
            AssetBundleBuildData pBundleData = CreateAssetBundleData(path, bundleName, bundleVariant);
            pBundleData.mDesc = bundleDesc;

            // ����ÿһ���ļ�������Ҫ���ļ�����bundle name
            string[] fileNames = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToArray();
            fileNames = PathHelper.ReformPathStrings(fileNames).ToArray();
            foreach (string name in fileNames)
            {
                // ��������Ҫ���õ��ļ�
                if (IsFileSkipBundleInfoUpdateInAssetDatabase(name, unityScenePathList))
                {
                    continue;
                }

                AddAssetPathBundleInfo(path, name);
            }
        }

        /// <summary>
        /// ��ȡ������Ҫ���bundle��·��
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

            // ���·����ȥ���Ƿ���·��
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
        /// ���bundleDir��û�в��Ϸ������
        /// </summary>
        /// <param name="bundleDirs"></param>
        private static void CheckBundleDirs(List<string> bundleDirs)
        {
            HashSet<string> illegalBundleDirs = new HashSet<string>();
            // ���ĳ��·��������·����������Ϊ�Ƿ�
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

            // �Ƴ��Ƿ�·��
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
        /// �ļ��Ƿ�����bundle��Ϣ����
        /// </summary>
        /// <returns></returns>
        private static bool IsFileSkipBundleInfoUpdateInAssetDatabase(string fileName,
            List<string> unityScenePathList)
        {
            // .meta��β,(DS_Store mac �µ�ϵͳ�ļ�),����
            if (fileName.EndsWith(".meta") || fileName.EndsWith("DS_Store"))
            {
                return true;
            }

            // ��.unity�ļ���ͬһ��Ŀ¼�µ����з�.unity�ļ���Ҳͬ��������
            // ��ΪĬ�ϻᱻ����scene��bundle,��.unity�ļ�����Ҫ���õ�
            if (!fileName.EndsWith(".unity") && unityScenePathList != null &&
                unityScenePathList.Any(unityScenePath => fileName.Contains(unityScenePath)))
            {
                return true;
            }

            // ����bundleDesc����
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
        /// ��ָ��·����ȡbundle���ƺͱ���
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
        /// ��bundle������ȥ�����ڵ�����ƣ�����bundle�ļ�����̫������ļ��޷�����������
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
        /// ���Ի�ȡ·���µ�bundleDescription������Ϊ��
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
        /// ��ȡ·����Ӧ��bundlename
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replaceLastFolderNameStr"></param>
        /// <param name="autoExtension"></param>
        /// <returns></returns>
        public static string GetBundleNameByAssetPath(string path, string replaceLastFolderNameStr = "")
        {

            // �����Ҫ�滻��Դ·���е����һ��Ŀ¼���֣����滻
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
        /// �滻·�������һ��Ŀ¼������
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
        /// BundleMeta ����·��
        /// </summary>
        public static string ResourcesFolder
        {
            get
            {
                return $"Assets/Resources/";
            }
        }

        /// <summary>
        /// AssetBundle ��Դ��·��
        /// </summary>
        public static string RuntimeAssetsPathInEditor = "Assets/RuntimeAssets";
        

        /// <summary>
        /// AssetBundles build��ʱ������·��
        /// </summary>
        public static string AssetBundlesBuildOutputPath = "AssetBundles";
        /// <summary>
        /// AssetBundles build��ʱ������·�� ����ƽ̨��Ϣ
        /// </summary>
        public static string AssetBundlesBuildOutputPathWithVersion
        {
            get
            {
                return AssetBundlesBuildOutputPath + "/" + "Default";
            }
        }
        /// <summary>
        /// streamingAssets��bundle�Ļ���·��
        /// </summary>
        public static string StreamingAssetsBundlePath
        {
            get
            {
                return string.Format("{0}/AssetBundles", Application.streamingAssetsPath);
            }
        }


        #endregion

        #region ��ʱ����

        /// <summary>
        /// ����һ����ScriptableObject��չ����Դ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullAssetPath">���к�׺����·��</param>
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



