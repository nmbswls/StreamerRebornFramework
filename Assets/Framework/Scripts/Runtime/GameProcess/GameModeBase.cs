using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace My.Framework.Runtime
{
    /// <summary>
    /// ���¹����ֳ�
    /// </summary>

    public partial class GameModeLoadPipeLineCtxBase
    {
        public virtual bool Start()
        {
            if (m_runing) return false;
            m_runing = true;
            return true;
        }

        public virtual void Clear()
        {
            m_runing = false;
            m_loadingStaticResCorutineCount = 0;
            m_loadingDynamicResCorutineCount = 0;
            m_updateViewAction = null;

            SceneInfoId = 0;
            FakeSceneName = "";
            RetLayer = null;
        }

        public bool IsRunning()
        {
            return m_runing;
        }

        /// <summary>
        /// ���س�����Ϣ
        /// </summary>
        public int SceneInfoId;
        public string FakeSceneName;
        
        /// <summary>
        /// ���߼��ؽ��
        /// </summary>
        public SceneLayerUnity RetLayer;

        /// <summary>
        /// �Ƿ���������
        /// </summary>
        protected bool m_runing;

        /// <summary>
        /// �͸ù����ֳ���ص���Դ���ع�������
        /// </summary>
        public int m_loadingStaticResCorutineCount;
        public int m_loadingDynamicResCorutineCount;

        /// <summary>
        /// ��Ҫ��updateViewʱִ�е�action
        /// </summary>
        public Action m_updateViewAction;
    }

    /// <summary>
    /// ��Ϸ
    /// </summary>
    public class GameModeBase
    {
        /// <summary>
        /// ��scene
        /// </summary>
        public SceneLayerUnity MainSceneLayer;

        /// <summary>
        /// ����scene �������ೡ��ʱʹ��
        /// </summary>
        public List<SceneLayerUnity> ManagedScenes = new List<SceneLayerUnity>();


        /// <summary>
        /// ����ӿ� ��ʼһ�μ��� TODO REMOVE FAKE
        /// </summary>
        public void TryStartLoad(string sceneName, Action onEnd = null)
        {
            var pipeline = AllocPipeLineCtx();
            pipeline.FakeSceneName = sceneName;
            pipeline.SceneInfoId = 1;
            pipeline.m_updateViewAction += onEnd;

            m_pipeLineWait2Start.Add(pipeline);
        }


        /// <summary>
        /// ���س��������ڴ�
        /// </summary>
        protected bool StartUpdatePipeline(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            // �ж��Ƿ�ʹ��Ĭ�ϵĹ����ֳ�
            StartPipeLineCtx(pipeCtx);

            List<string> dynamicResPathList = CollectAllDynamicResForLoad(pipeCtx);

            StartLoadScene(pipeCtx);
            StartLoadDynamicRes(pipeCtx, dynamicResPathList);
            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dTime"></param>
        public virtual void Tick(float dTime)
        {
            // ����pipline����
            {
                while (m_pipeLineWait2Start.Count != 0)
                {
                    var ctx = m_pipeLineWait2Start[0];
                    StartUpdatePipeline(ctx);
                    m_pipeLineWait2Start.Remove(ctx);
                }
            }
        }

        #region ��������д

        public virtual bool IsReserve { get { return false; } }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        protected virtual void Initialize(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (pipeCtx.m_updateViewAction != null)
            {
                pipeCtx.m_updateViewAction();
            }
        }

        /// <summary>
        /// �ռ�����
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectScenes4Load(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return new List<string>() { MainSceneResPath };
        }


        /// <summary>
        /// �ռ���Ҫ���صĶ�̬��Դ��·��
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectAllDynamicResForLoad(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return null;
        }

        /// <summary>
        /// ����PipeLineCtxʵ��
        /// </summary>
        /// <returns></returns>
        protected virtual GameModeLoadPipeLineCtxBase CreatePipeLineCtx()
        {
            return new GameModeLoadPipeLineCtxBase();
        }

        #endregion


        #region ���ߴ���

        /// <summary>
        /// ���������ֳ�
        /// </summary>
        /// <param name="pipeCtx"></param>
        /// <returns></returns>
        protected virtual bool StartPipeLineCtx(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (pipeCtx == null || !pipeCtx.Start())
            {
                return false;
            }
            // �������еĹ����ֳ���¼����
            m_runingPipeLineCtxList.Add(pipeCtx);

            return true;
        }

        /// <summary>
        /// ��������ֳ�
        /// </summary>
        /// <param name="pipeCtx"></param>
        protected virtual void ReleasePipeLineCtx(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            pipeCtx.Clear();
            m_runingPipeLineCtxList.Remove(pipeCtx);
            if (pipeCtx != m_pipeCtxDefault)
            {
                m_unusingPipeLineCtxList.Add(pipeCtx);
            }
        }

        /// <summary>
        /// ����һ�������ֳ�
        /// </summary>
        /// <returns></returns>
        protected virtual GameModeLoadPipeLineCtxBase AllocPipeLineCtx()
        {
            GameModeLoadPipeLineCtxBase ret;
            if (m_unusingPipeLineCtxList.Count != 0)
            {
                ret = m_unusingPipeLineCtxList[m_unusingPipeLineCtxList.Count - 1];
                m_unusingPipeLineCtxList.RemoveAt(m_unusingPipeLineCtxList.Count - 1);
            }
            else
            {
                ret = CreatePipeLineCtx();
            }
            return ret;
        }



        /// <summary>
        /// Ĭ�Ϲ����ֳ�
        /// </summary>
        protected GameModeLoadPipeLineCtxBase m_pipeCtxDefault = new GameModeLoadPipeLineCtxBase();

        /// <summary>
        /// �������еĹ����ֳ����б�
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_runingPipeLineCtxList = new List<GameModeLoadPipeLineCtxBase>();

        /// <summary>
        /// û��ʹ�õĹ����ֳ�
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_unusingPipeLineCtxList = new List<GameModeLoadPipeLineCtxBase>();

        /// <summary>
        /// �ȴ������Ĺ���
        /// </summary>
        protected List<GameModeLoadPipeLineCtxBase> m_pipeLineWait2Start = new List<GameModeLoadPipeLineCtxBase>();

        #endregion


        #region ��Դ����

        /// <summary>
        /// ��ʼ���ؾ�̬��Դ
        /// </summary>
        protected virtual void StartLoadScene(GameModeLoadPipeLineCtxBase pipeCtx)
        {

            List<string> scenes4Load = CollectScenes4Load(pipeCtx);

            if(scenes4Load == null || scenes4Load.Count == 0)
            {
                // �������ߣ���̬��Դ�������
                OnLoadStaticResCompleted(pipeCtx);
                return;
            }
            foreach(var scenePath in scenes4Load)
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                pipeCtx.m_loadingStaticResCorutineCount++;
                // ����scene
                SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUnity), sceneName, scenePath,
                            (layer) =>
                            {
                            // ����ʧ��
                            if (layer == null)
                                {
                                    Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(),
                                        "Battle"));
                                    return;
                                }


                                pipeCtx.RetLayer = (SceneLayerUnity)layer;
                                pipeCtx.RetLayer.SetReserve(true);
                            // �����м���--
                            pipeCtx.m_loadingStaticResCorutineCount--;
                            // �������ߣ���̬��Դ�������
                            OnLoadStaticResCompleted(pipeCtx);
                            });
            }
            
        }

        /// <summary>
        /// ������̬��Դ����
        /// </summary>
        /// <param name="resPathSet"></param>
        /// <param name="pipeCtx"></param>
        protected virtual void StartLoadDynamicRes(GameModeLoadPipeLineCtxBase pipeCtx, List<string> resPathList)
        {
            // ���˳�������Ҫ���ص���Դ
            if (resPathList == null || resPathList.Count == 0)
            {
                OnLoadDynamicResCompleted(pipeCtx, null);
                return;
            }
            var resPathSet = new HashSet<string>(resPathList);

            // ��������
            Dictionary<string, UnityEngine.Object> resDict = new Dictionary<string, UnityEngine.Object>();
            pipeCtx.m_loadingDynamicResCorutineCount++;
            SimpleResourceManager.Instance.StartLoadAssetsCorutine(resPathSet, resDict,
                () =>
                {
                    pipeCtx.m_loadingDynamicResCorutineCount--;
                    OnLoadDynamicResCompleted(pipeCtx, resDict);
                }, loadAsync: true);
        }

        /// <summary>
        /// ����̬��Դ�������
        /// </summary>
        protected virtual void OnLoadStaticResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            if (IsLoadAllResCompleted(pipeCtx)) OnLoadAllResCompleted(pipeCtx);
        }

        /// <summary>
        /// ����̬��Դ�������
        /// </summary>
        protected virtual void OnLoadDynamicResCompleted(GameModeLoadPipeLineCtxBase pipeCtx, Dictionary<string, UnityEngine.Object> resDict)
        {
            if (resDict != null && resDict.Count != 0)
            {
                // ���ռ��ص���Դ���浽m_dynamicResCacheDict
                foreach (var item in resDict)
                {
                    // ֻ����ɹ����ص����
                    if (item.Value != null)
                    {
                        
                    }
                }
            }
            if (IsLoadAllResCompleted(pipeCtx)) OnLoadAllResCompleted(pipeCtx);
        }

        /// <summary>
        /// �Ƿ����е���Դ���ض������
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadAllResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return pipeCtx.m_loadingStaticResCorutineCount == 0 && pipeCtx.m_loadingDynamicResCorutineCount == 0;
        }

        /// <summary>
        /// ��������Դ�������
        /// </summary>
        protected virtual void OnLoadAllResCompleted(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            // Push Layer
            if(pipeCtx.RetLayer != null)
            {
                // ���سɹ���¼layer
                ManagedScenes.Add(pipeCtx.RetLayer);
                if (MainSceneLayer == null)
                {
                    MainSceneLayer = pipeCtx.RetLayer;
                }
            }

            if (Stopped)
            {
                // ������Դ
                ClearAllContextAndRes();
                // ֪ͨ���߸������(ʧ��)
                if (pipeCtx.m_updateViewAction != null)
                    pipeCtx.m_updateViewAction();
                return;
            }

            // ������ʾ
            Initialize(pipeCtx);

            // �����ֳ�������
            ReleasePipeLineCtx(pipeCtx);
        }


        /// <summary>
        /// ���������ֳ�
        /// </summary>
        protected virtual void ClearAllContextAndRes()
        {
            foreach(var scene in ManagedScenes)
            {
                SceneLayerManager.Instance.FreeLayer(scene);
            }
        }

        /// <summary>
        /// ��Դ����
        /// </summary>
        public virtual string MainSceneResPath { get; set; }

        /// <summary>
        /// stop ֹͣ
        /// </summary>
        public bool Stopped { get; set; }

        #endregion

    }
}
