using My.Framework.Runtime.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace My.Framework.Runtime.Config
{
    public class ConfigDataLoaderBase
    {

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="onEnd"></param>
        public virtual bool TryLoadInitConfig(System.Action<bool> onEnd, out int initLoadDataCount)
        {
            var pathSet = new HashSet<string>();
            foreach (var configName in m_validConfigDataName)
            {
                pathSet.Add(GetConfigPathByName(configName));
            }
            initLoadDataCount = pathSet.Count;
            if(pathSet.Count == 0)
            {
                onEnd?.Invoke(true);
                return true;
            }
            //������init�׶���Ҫ���ص���������
            GameManager.Instance.StartCoroutine(InitLoadConfigData(pathSet, 1, 30, onEnd));
            return true;
        }

        /// <summary>
        /// ͨ����������ȡ����·��
        /// </summary>
        /// <returns></returns>
        protected virtual string GetConfigPathByName(string configName)
        {
            return configName;
        }


        /// <summary>
        /// Ԥ����
        /// </summary>
        public virtual void PreInitConfig()
        {

            #region �Ի���Ϣ

            #endregion

        }



        /// <summary>
        /// ִ�г�ʼ������
        /// </summary>
        /// <param name="pathSet"></param>
        /// <param name="threadCount"></param>
        /// <param name="loadCountForSingleYield"></param>
        /// <param name="onEnd"></param>
        /// <returns></returns>
        protected IEnumerator InitLoadConfigData(HashSet<string> pathSet, int threadCount,
            int loadCountForSingleYield, Action<bool> onEnd)
        {
            
            int workerRet = 0;

            // �����Ҫ���߳�
            if (threadCount > 1)
            {
                // ��ʼ����Worker
                var workers = new List<Thread>(threadCount);
                for (int i = 0; i < threadCount; i++)
                {
                    int id = i;
                    var thread = new Thread(() =>
                    {
                        // �������з�lua��չ����
                        var iter = InitLoadConfigDataWorker(
                            threadCount, id, pathSet, loadCountForSingleYield,
                            (lret) =>
                            {
                                if (!lret) Interlocked.Exchange(ref workerRet, -1);
                            });
                        while (iter.MoveNext()) { }
                    });
                    workers.Add(thread);
                    thread.Start();
                }

                // �ȴ�����worker���
                while (workers.Find((t) => t.IsAlive) != null)
                {
                    yield return null;
                }
            }
            // ����ǵ��̼߳���
            else
            {
                yield return InitLoadConfigDataWorker(1, 0, pathSet, loadCountForSingleYield,
                    onEnd: (lret) =>
                    {
                        if (!lret) Interlocked.Exchange(ref workerRet, -1);
                    });
            }

            if (onEnd != null) onEnd(workerRet == 0);
        }

        /// <summary>
        /// ��ʼ���������ݼ��ع�������
        /// </summary>
        /// <param name="workerCount"></param>
        /// <param name="workerId"></param>
        /// <param name="pathSet"></param>
        /// <param name="loadCountForSingleYield"></param>
        /// <param name="onEnd"></param>
        /// <returns></returns>
        protected IEnumerator InitLoadConfigDataWorker(
            int workerCount, int workerId,
            HashSet<string> pathSet,
            int loadCountForSingleYield,
            Action<bool> onEnd)
        {
            int assetCount = 0;
            bool inThreading = workerCount > 1;
            var loadOutTime = DateTime.Now.AddSeconds(0.5f);
            foreach (string path in pathSet)
            {
                // ���ж��worker���м��ص�ʱ��ͨ��id��ģ����ѡ��worker��Ҫ���ص���Դ
                assetCount++;
                if (assetCount % (workerId + 1) != 0)
                {
                    continue;
                }

                string configDataName = Path.GetFileNameWithoutExtension(path);
                // ������Դ����
                TextAsset configDataAsset = null;
                configDataAsset = SimpleResourceManager.Instance.LoadAssetSync<TextAsset>(path, true);
                // ����Դ���� 
                if (configDataAsset == null)
                {
                    continue;
                }

                // ���з����л�
                if (!OnLoadConfigDataAssetEnd(configDataName, configDataAsset, false))
                {
                    if (onEnd != null) onEnd(false);
                    yield break;
                }

                if (loadOutTime < DateTime.Now)
                {
                    loadOutTime = DateTime.Now.AddSeconds(0.5f);
                    yield return null;
                }
            }

            // ��ɷ����л���֪ͨ�ⲿ
            if (onEnd != null)
                onEnd(true);
        }

        #region ��С������

        /// <summary>
        /// ��ȡ�����л�����
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected Action<string> GetDeserializFunc4ConfigData(string typeName)
        {
            Action<string> func;
            if (m_deserializFuncDict.TryGetValue(typeName, out func))
                return func;

            return null;
        }

        /// <summary>
        /// �����ļ���Դ������ɺ���
        /// </summary>
        /// <param name="configDataName"></param>
        /// <param name="path"></param>
        /// <param name="configDataAsset"></param>
        /// <param name="inThreading"></param>
        /// <param name="isUnload"></param>
        /// <returns></returns>
        protected bool OnLoadConfigDataAssetEnd(string configDataName, TextAsset configDataAsset, bool inThreading, bool isUnload = true)
        {
            // ���������Դʧ��
            if (configDataAsset == null ||
                configDataAsset.text == null)
            {
                Debug.LogError(string.Format("ClientConfigDataLoader InitLoadConfigDataWorker fail {configDataName} = null"));

                // �����Ϊfalseʱ��������ԴΪ�գ��������Դ����Ϣ�����鿪��ʱʹ�ã�
                return false;
            }


            // ִ�з����л�
            var deserializFunc = GetDeserializFunc4ConfigData(configDataName);
            if (deserializFunc != null)
            {
                deserializFunc(configDataAsset.text);
            }
            else
            {
                Debug.LogWarning(string.Format("ConfigData: {0} has no DeserializFunc", configDataName));
            }

            // ж������ɷ����л�����Դ
            if (isUnload)
                Resources.UnloadAsset(configDataAsset);

            return true;
        }

        #endregion

        protected virtual void InitValidConfigDataName()
        {

        }


        /// <summary>
        /// ��ʼ�������л����� ��Ҫ
        /// </summary>
        protected virtual void InitDeserializFunc4ConfigData()
        {

        }

        /// <summary>
        /// �������б�
        /// </summary>
        protected List<string> m_validConfigDataName = new List<string>();

        /// <summary>
        /// �����л��������ֵ�
        /// </summary>
        protected Dictionary<string, Action<string>> m_deserializFuncDict = new Dictionary<string, Action<string>>();

    }
}

