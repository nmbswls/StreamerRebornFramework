using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime.Resource
{
    /// <summary>
    /// 资源管理器,资源的加载接口
    /// </summary>
    public partial class SimpleResourceManager
    {
        /// <summary>
        /// 启动unityScene加载
        /// </summary>
        public IEnumerator LoadUnityScene(string path, Action<string, UnityEngine.SceneManagement.Scene?> onCompleted,
            bool noErrlog = false, bool loadAync = false, bool bAdditive = true)
        {
            m_loadingOpCount++;

            // 先解析路径
            string sceneName = Path.GetFileNameWithoutExtension(path);

            // 在Ready之前
            if (!Inited)
            {
                Debug.LogError("LoadUnityScene but  State != RMState.Ready");
                onCompleted(path, null);
                yield break;
            }

            // 运行时环境，或者允许在editor中加载bundle
            bool ret = false;
            AsyncOperation asyncOp = null;
            if (!Application.isEditor || m_loadAssetFromBundleInEditor)
            {
                // 加载bundle

                var iter = LoadBundle4UnityScene(path,
                    (lpath, bundleCacheItem) =>
                    {
                        ret = bundleCacheItem != null;
                    }, noErrlog, loadAync);

                while (iter.MoveNext())
                {
                    yield return null;
                }

                // 加载bundle失败
                if (!ret)
                {
                    onCompleted(path, null);
                    yield break;
                }

                // 加载场景
                asyncOp = SceneManager.LoadSceneAsync(path, bAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            // 判断加载是否成功
            var scene = SceneManager.GetSceneByPath(path);
            if (scene.IsValid() && scene.isLoaded)
            {
                goto LB_LOADEND;
            }

#if UNITY_EDITOR

            // 编辑器模式下 fallback到 LoadLevelAdditiveAsyncInPlayMode
            if (Application.isEditor)
            {
                asyncOp = EditorSceneManager.LoadSceneAsyncInPlayMode(path,new UnityEngine.SceneManagement.LoadSceneParameters(bAdditive ? LoadSceneMode.Additive:LoadSceneMode.Single));
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }
            scene = SceneManager.GetSceneByName(sceneName);
#endif


            LB_LOADEND:
            m_loadingOpCount--;

            // 通知调用者完成
            onCompleted(path, scene);
        }
    }
}
