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
    /// ��Դ������,��Դ�ļ��ؽӿ�
    /// </summary>
    public partial class SimpleResourceManager
    {
        /// <summary>
        /// ����unityScene����
        /// </summary>
        public IEnumerator LoadUnityScene(string path, Action<string, UnityEngine.SceneManagement.Scene?> onCompleted,
            bool noErrlog = false, bool loadAync = false, bool bAdditive = true)
        {
            m_loadingOpCount++;

            // �Ƚ���·��
            string sceneName = Path.GetFileNameWithoutExtension(path);

            // ��Ready֮ǰ
            if (!Inited)
            {
                Debug.LogError("LoadUnityScene but  State != RMState.Ready");
                onCompleted(path, null);
                yield break;
            }

            // ����ʱ����������������editor�м���bundle
            bool ret = false;
            AsyncOperation asyncOp = null;
            if (!Application.isEditor || m_loadAssetFromBundleInEditor)
            {
                // ����bundle

                var iter = LoadBundle4UnityScene(path,
                    (lpath, bundleCacheItem) =>
                    {
                        ret = bundleCacheItem != null;
                    }, noErrlog, loadAync);

                while (iter.MoveNext())
                {
                    yield return null;
                }

                // ����bundleʧ��
                if (!ret)
                {
                    onCompleted(path, null);
                    yield break;
                }

                // ���س���
                asyncOp = SceneManager.LoadSceneAsync(path, bAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            // �жϼ����Ƿ�ɹ�
            var scene = SceneManager.GetSceneByPath(path);
            if (scene.IsValid() && scene.isLoaded)
            {
                goto LB_LOADEND;
            }

#if UNITY_EDITOR

            // �༭��ģʽ�� fallback�� LoadLevelAdditiveAsyncInPlayMode
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

            // ֪ͨ���������
            onCompleted(path, scene);
        }
    }
}
