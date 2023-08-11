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
        /// 加载配置
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
            //加载在init阶段需要加载的配置数据
            GameManager.Instance.StartCoroutine(InitLoadConfigData(pathSet, 1, 30, onEnd));
            return true;
        }

        /// <summary>
        /// 通过配置名获取加载路径
        /// </summary>
        /// <returns></returns>
        protected virtual string GetConfigPathByName(string configName)
        {
            return configName;
        }


        /// <summary>
        /// 预处理
        /// </summary>
        public virtual void PreInitConfig()
        {

            #region 对话信息

            #endregion

        }



        /// <summary>
        /// 执行初始化加载
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

            // 如果需要多线程
            if (threadCount > 1)
            {
                // 开始启动Worker
                var workers = new List<Thread>(threadCount);
                for (int i = 0; i < threadCount; i++)
                {
                    int id = i;
                    var thread = new Thread(() =>
                    {
                        // 处理所有非lua扩展类型
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

                // 等待所有worker完成
                while (workers.Find((t) => t.IsAlive) != null)
                {
                    yield return null;
                }
            }
            // 如果是单线程加载
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
        /// 初始化配置数据加载工作函数
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
                // 当有多个worker并行加载的时候通过id和模运算选择本worker需要加载的资源
                assetCount++;
                if (assetCount % (workerId + 1) != 0)
                {
                    continue;
                }

                string configDataName = Path.GetFileNameWithoutExtension(path);
                // 启动资源加载
                TextAsset configDataAsset = null;
                configDataAsset = SimpleResourceManager.Instance.LoadAssetSync<TextAsset>(path, true);
                // 无资源跳过 
                if (configDataAsset == null)
                {
                    continue;
                }

                // 进行反序列化
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

            // 完成反序列化，通知外部
            if (onEnd != null)
                onEnd(true);
        }

        #region 最小集方法

        /// <summary>
        /// 获取反序列化函数
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
        /// 配置文件资源加载完成后处理
        /// </summary>
        /// <param name="configDataName"></param>
        /// <param name="path"></param>
        /// <param name="configDataAsset"></param>
        /// <param name="inThreading"></param>
        /// <param name="isUnload"></param>
        /// <returns></returns>
        protected bool OnLoadConfigDataAssetEnd(string configDataName, TextAsset configDataAsset, bool inThreading, bool isUnload = true)
        {
            // 如果加载资源失败
            if (configDataAsset == null ||
                configDataAsset.text == null)
            {
                Debug.LogError(string.Format("ClientConfigDataLoader InitLoadConfigDataWorker fail {configDataName} = null"));

                // 检测标记为false时，允许资源为空，报告空资源的信息（建议开发时使用）
                return false;
            }


            // 执行反序列化
            var deserializFunc = GetDeserializFunc4ConfigData(configDataName);
            if (deserializFunc != null)
            {
                deserializFunc(configDataAsset.text);
            }
            else
            {
                Debug.LogWarning(string.Format("ConfigData: {0} has no DeserializFunc", configDataName));
            }

            // 卸载已完成反序列化的资源
            if (isUnload)
                Resources.UnloadAsset(configDataAsset);

            return true;
        }

        #endregion

        protected virtual void InitValidConfigDataName()
        {

        }


        /// <summary>
        /// 初始化反序列化函数 需要
        /// </summary>
        protected virtual void InitDeserializFunc4ConfigData()
        {

        }

        /// <summary>
        /// 配置名列表
        /// </summary>
        protected List<string> m_validConfigDataName = new List<string>();

        /// <summary>
        /// 反序列化函数的字典
        /// </summary>
        protected Dictionary<string, Action<string>> m_deserializFuncDict = new Dictionary<string, Action<string>>();

    }
}

