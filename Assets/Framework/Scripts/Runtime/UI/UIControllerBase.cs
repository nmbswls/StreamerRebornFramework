using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// �����ֳ�
    /// </summary>
    public class UIRefreshPipelineCtx
    {
        /// <summary>
        /// ����
        /// </summary>
        public virtual void Clear()
        {
            m_isInitPipeLine = false;
            m_layerLoadedInPipe = false;
            m_isResume = false;
            m_updateMask = 0;
            m_isRuning = false;
            m_onPipelineEnd = null;
        }

        /// <summary>
        /// ���ø�������
        /// </summary>
        /// <param name="mask"></param>
        public void SetUpdateMask(ulong mask)
        {
            m_updateMask = mask;
        }

        /// <summary>
        /// ���ø�������
        /// </summary>
        /// <param name="indexs"></param>
        public void SetUpdateMask(params int[] indexs)
        {
            m_updateMask = 0;

            foreach (int index in indexs)
            {
                if (index < 64)
                {
                    m_updateMask |= 1ul << index;
                }
            }
        }

        /// <summary>
        /// ���ø�������
        /// </summary>
        public void AddUpdateMask(int index)
        {
            if (index < 64)
            {
                m_updateMask |= 1ul << index;
            }
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="index"></param>
        public void ClearUpdateMask(int index)
        {
            if (index < 64)
            {
                m_updateMask &= (ulong)(~(1 << index));
            }
        }

        /// <summary>
        /// ���Ը�������
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsNeedUpdate(int index)
        {
            if (index < 64)
            {
                return (m_updateMask & 1ul << index) != 0;
            }
            return false;
        }

        /// <summary>
        /// �����������Ƿ�ȫ���������״̬
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateMaskClear()
        {
            return m_updateMask == 0;
        }

        /// <summary>
        /// �Ƿ��ǳ�ʼ������
        /// </summary>
        public bool m_isInitPipeLine = true;

        /// <summary>
        /// �Ƿ���resume
        /// </summary>
        public bool m_isResume;

        /// <summary>
        /// �����Ƿ���������
        /// </summary>
        public bool m_isRuning;

        /// <summary>
        /// ��ǰ���pipeline�Ƿ���uilayer������
        /// </summary>
        public bool m_layerLoadedInPipe;

        /// <summary>
        /// �Ƿ���ֹ���е�task��ui����
        /// </summary>
        public bool m_blockGlobalUIInput = true;

        /// <summary>
        /// ��������
        /// </summary>
        protected ulong m_updateMask;

        /// <summary>
        /// �ص�
        /// </summary>
        public Action<bool> m_onPipelineEnd;
    }


    public abstract class UIControllerBase
    {
        /// <summary>
        /// ״̬����
        /// </summary>
        public enum UIState
        {
            Init,
            Running,
            Suspend,
            Stopped,
        }

        public UIControllerBase(string name)
        {
            this.Name = name;
        }

        public void Tick(float dt)
        {
            // ����stop״̬��Ӧ�ñ�tick������
            if (State == UIState.Stopped)
            {
                return;
            }
            // ֻ��Running����ִ��tick�߼�
            if (State != UIState.Running)
                return;

            if (m_playingRefreshViewEffectList.Count != 0)
            {
                foreach (var item in m_playingRefreshViewEffectList)
                {
                    if (item != null && item.m_timeOutTime != null)
                    {
                        if (item.m_timeOutTime <= Time.time)
                        {
                            m_playingRefreshViewEffectList.Remove(item);
                            break;
                        }
                    }
                }

                if (m_playingRefreshViewEffectList.Count == 0)
                {
                    OnAllRefreshEffectsEnd();
                }
            }


            OnTick(dt);
        }


        /// <summary>
        /// ����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ��ʾui
        /// </summary>
        /// <returns></returns>
        public bool Start(UIIntent intent, Action<bool> onRefreshPipelineEnd = null)
        {
            if (State != UIState.Init)
                return false;

            State = UIState.Running;

            OnPreRefreshPipeline();

            m_isOpeningUI = true;

            m_currPipelineCtx = GetRefreshPipelineCtx();
            if (!StartRefreshPipeLine(intent, onRefreshPipelineEnd))
            {
                Stop();
                return false;
            }

            return true;
        }

        /// <summary>
        /// ��ʾui
        /// </summary>
        /// <returns></returns>
        public bool Resume(UIIntent intent, Action<bool> onRefreshPipelineEnd = null)
        {
            if (State != UIState.Suspend)
                return false;


            State = UIState.Running;


            m_isOpeningUI = true;

            // ��ʾLayer
            InitLayerStateOnLoadAllResCompleted();


            m_currPipelineCtx.m_isResume = true;
            if (!StartRefreshPipeLine(intent, onRefreshPipelineEnd))
            {
                State = UIState.Suspend;
                return false;
            }

            return true;
        }

        /// <summary>
        /// ����UI���� ������ 
        /// </summary>
        public void Suspend()
        {
            if (State != UIState.Running)
                return;

            // ��¼��ͣ��ʼʱ��
            PauseStartTime = UnityEngine.Time.realtimeSinceStartup;

            // ����ʱ���ui����
            EnableUIInput(true, true);
            //UIManager.Instance.GlobalUIInputBlockClear(Name);

            HideAllView();

            OnSuspend();

            State = UIState.Suspend;
        }

        public void Stop()
        {
            if (State == UIState.Stopped)
                return;

            State = UIState.Stopped;

            // ��task��ͣ��ʱ������ui����
            EnableUIInput(true, true);
            //UIManager.Instance.GlobalUIInputBlockClear(Name);

            ClearAllContextAndRes();

            OnStop();

            if (m_uiCompArray != null)
            {
                foreach (var comp in m_uiCompArray)
                {
                    if (comp != null)
                    {
                        comp.Clear();
                    }
                }
                m_uiCompArray = null;
            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public virtual bool OnNewIntent(UIIntent intent)
        {
            if (State != UIState.Running)
            {
                return false;
            }

            if (!StartRefreshPipeLine(intent))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// ������ʾ��ָ�
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public bool StartRefreshPipeLine(UIIntent intent, Action<bool> onRefreshPipelineEnd = null)
        {
            if (State != UIState.Running)
            {
                Debug.LogError($"UIControllerBase.StartRefreshPipeLine Error: State {State} != UIState.Running");
                return false;
            }

            // �Ƿ��й�����������
            if (m_currPipelineCtx.m_isRuning)
            {
                return false;
            }

            if (intent != null &&
                (m_currIntent != intent || m_currMode != intent.TargetMode))
            {
                // ����mode
                if (!SetCurrentMode(intent.TargetMode))
                {
                    Debug.LogError(string.Format("UITaskBase.StartUpdatePipeLine fail error mode {0}", intent.TargetMode));
                    return false;
                }
                m_currIntent = intent;
                ParseIntent();
            }

            m_currPipelineCtx.m_isRuning = true;
            m_currPipelineCtx.m_onPipelineEnd = onRefreshPipelineEnd;

            bool isNeedLoadStaticRes = IsNeedLoadStaticRes();
            bool isNeedLoadDynamicRes = IsNeedLoadDynamicRes();

            // �����Ҫ������Դ
            if (isNeedLoadStaticRes || isNeedLoadDynamicRes)
            {
                // ���ؾ�̬��Դ
                if (isNeedLoadStaticRes) StartLoadStaticRes();

                // ���ض�̬��Դ
                if (isNeedLoadDynamicRes) StartLoadDynamicRes();

                // UpdateView����OnLoadAllResCompleted���� 
                return true;
            }

            // �������Ҫ������Դ��ֱ�����
            OnLoadAllResCompleted();

            return true;
        }

        /// <summary>
        /// ����intentΪ�ڲ�����
        /// </summary>
        protected virtual void ParseIntent()
        {

        }

        /// <summary>
        /// ���¹�������ǰ�ص�
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnPreRefreshPipeline()
        {
            return true;
        }

        protected virtual bool OnStop()
        {
            return true;
        }

        protected virtual bool OnSuspend()
        {
            return true;
        }

        protected virtual void OnTick(float dt)
        {

        }

        /// <summary>
        /// �ж���û��layer��stack����
        /// </summary>
        /// <returns></returns>
        public bool IsLayerInStack()
        {
            if (m_layerArray == null)
                return false;
            foreach (var layer in m_layerArray)
            {
                if (layer != null && layer.State == SceneLayerBase.LayerState.InStack)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool m_isOpeningUI = false;
        protected bool m_isNeedCloseOnPostUpdateViewEnd = false;

        public float PauseStartTime { get; set; }

        private Action<bool> m_onReady;

        #region RefreshView ���

        /// <summary>
        /// ����������ʾ
        /// </summary>
        protected void StartRefreshView()
        {
            OnPreRefreshView();

            RefreshView();

            OnPostRefreshView();

            if (m_playingRefreshViewEffectList.Count != 0)
            {
                return;
            }

            OnAllRefreshEffectsEnd();
        }

        /// <summary>
        /// ִ����Ч������Ĵ���
        /// </summary>
        protected void OnAllRefreshEffectsEnd()
        {
            // ���ui�����ֹ
            EnableUIInput(true, true);

            var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

            ClearRefreshViewContext();

            // ���ui�����ֹ
            EnableUIInput(true, true);

            // ֪ͨ���߸������(�ɹ�)
            if (onPipelineEnd != null)
            {
                onPipelineEnd(true);
            }

            //if (m_isNeedRegisterPlayerContextEvents)
            //{
            //    m_isNeedRegisterPlayerContextEvents = false;
            //    if (!m_isPlayerContextEventsRegistered)
            //    {
            //        m_isPlayerContextEventsRegistered = true;
            //        RegisterPlayerContextEvents();
            //    }
            //}

            m_isOpeningUI = false;

            //if (State == UIState.Running && m_piplineQueue.Count != 0)
            //{
            //    var item = m_piplineQueue[0];
            //    m_piplineQueue.RemoveAt(0);
            //    StartUpdatePipeLine(item.m_intent, item.m_onlyUpdateView, item.m_canBeSkip);
            //}

            OnPostRefreshEffectEnd();


            if (m_isNeedCloseOnPostUpdateViewEnd)
            {
                m_isNeedCloseOnPostUpdateViewEnd = false;
                Stop();
            }
        }


        protected UIRefreshPipelineCtx GetRefreshPipelineCtx()
        {
            if (m_currPipelineCtx == null)
            {
                m_currPipelineCtx = CreateRefreshPipeLineCtx();
            }

            return m_currPipelineCtx;
        }

        /// <summary>
        /// ��������ֳ�
        /// </summary>
        /// <returns></returns>
        protected virtual UIRefreshPipelineCtx CreateRefreshPipeLineCtx()
        {
            return new UIRefreshPipelineCtx();
        }

        /// <summary>
        /// ���� viewǰ�ص�
        /// </summary>
        protected virtual void OnPreRefreshView()
        {

        }

        protected abstract void RefreshView();


        /// <summary>
        /// ���� view��ص�
        /// </summary>
        protected virtual void OnPostRefreshView()
        {

        }

        protected virtual void OnPostRefreshEffectEnd()
        {

        }


        #endregion

        #region controller����

        /// <summary>
        /// ������һ��UITask
        /// </summary>
        /// <returns></returns>
        protected UIControllerBase ReturnPrevUIController()
        {
            var intentReturnable = m_currIntent as UIIntentReturnable;
            if (intentReturnable == null || intentReturnable.PrevUIIntent == null)
            {
                return null;
            }

            var uiController = UIManager.Instance.ReturnUIController(intentReturnable.PrevUIIntent);
            if (uiController == null)
            {
                uiController = UIManager.Instance.StartUIController(intentReturnable.PrevUIIntent);
            }
            return uiController;
        }

        #endregion

        #region uiprocess

        ///// <summary>
        ///// ����ui����
        ///// </summary>
        ///// <param name="process"></param>
        ///// <param name="blockui"></param>
        ///// <param name="onEnd"></param>
        //public void PlayUIProcess(UIProcess process, bool blockui = true, Action<UIProcess, bool> onEnd = null)
        //{
        //    if (process == null)
        //    {
        //        return;
        //    }

        //    if (m_currPipeLineCtx.m_isRuning && blockui)
        //    {
        //        // �ڹ������Ѿ�����ui�ˣ����õ���EnableUIInput
        //        // if (blockui) EnableUIInput(false, false);

        //        RegUpdateViewPlayingEffect(process.ToString());

        //        process.Start((lprocess, isComplete) =>
        //        {
        //            if (onEnd != null)
        //            {
        //                onEnd(lprocess, isComplete);
        //            }

        //            UnregUpdateViewPlayingEffect(process.ToString());

        //        });
        //    }
        //    else
        //    {
        //        if (blockui) EnableUIInput(false, false);
        //        process.Start((lprocess, isComplete) =>
        //        {
        //            if (blockui) EnableUIInput(true, true);

        //            if (onEnd != null)
        //            {
        //                onEnd(lprocess, isComplete);
        //            }

        //        });
        //    }
        //}

        #endregion

        /// <summary>
        /// Ӱ�����е���ʾ
        /// </summary>
        protected virtual void HideAllView()
        {
            if (m_layerArray == null || m_layerArray.Length == 0)
            {
                return;
            }

            foreach (var layer in m_layerArray)
            {
                if (layer != null && layer.State == SceneLayerBase.LayerState.InStack)
                {
                    SceneLayerManager.Instance.PopLayer(layer);
                    var sceneLayer = layer as SceneLayerUnity;
                    if (sceneLayer != null)
                    {
                        HideOrShowUnitySceneLayer(sceneLayer, false);
                    }
                }
            }
        }

        protected static void HideOrShowUnitySceneLayer(SceneLayerUnity layer, bool show)
        {
            if (layer == null) return;

            foreach (var go in layer.UnitySceneRootObjs)
            {
                go.SetActive(show);
            }
        }

        /// <summary>
        /// ���õ�ǰ��mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected virtual bool SetCurrentMode(string mode)
        {
            if (m_currMode == mode)
            {
                return true;
            }

            // ���mode����
            m_currMode = mode;

            return true;
        }

        #region ��Դ����

        /// <summary>
        /// �Ƿ���Ҫ���¾�̬��Դ
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadStaticRes()
        {
            return MainLayer == null;
        }

        /// <summary>
        /// �Ƿ���Ҫ���ض�̬��Դ
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadDynamicRes()
        {
            return false;
        }

        /// <summary>
        /// ��ʼ���ؾ�̬��Դ
        /// </summary>
        protected virtual void StartLoadStaticRes()
        {
            if (LayerDescArray == null || LayerDescArray.Length <= 0)
            {
                return;
            }

            var toLoadList = new List<LayerDesc>(LayerDescArray);
            // ����layer�������
            m_layerArray = new SceneLayerBase[LayerDescArray.Length];
            // ���û����Ҫ���ص�layer��
            if (toLoadList.Count == 0)
            {
                // �������ߣ���̬��Դ�������
                OnLoadStaticResCompleted();
                return;
            }
            m_loadingStaticResCorutineCount++;
            // �������е�layer
            for (int i = 0; i < toLoadList.Count; i++)
            {

                var layerDesc = toLoadList[i];
                string layerName = layerDesc.m_layerName;
                string layerResPath = layerDesc.m_layerResPath;
                bool isUILayer = layerDesc.m_isUILayer;
                // �հ���ֵ
                int index = i;
                if (isUILayer)
                {
                    m_currPipelineCtx.m_layerLoadedInPipe = true;

                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUI), layerName, layerResPath,
                        (layer) =>
                        {
                        // ����ʧ��
                        if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(), layerName));
                                return;
                            }

                        // ���سɹ���¼layer
                        m_layerArray[index] = layer;
                        // �����м���--
                        m_loadingStaticResCorutineCount--;
                        // �������ߣ���̬��Դ�������
                        OnLoadStaticResCompleted();
                        });
                }
                else if (layerResPath.EndsWith(".unity"))
                {
                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUnity), layerName, layerResPath,
                        (layer) =>
                        {
                            // ����ʧ��
                            if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(),
                                    layerName));
                                return;
                            }

                            // ���سɹ���¼layer
                            m_layerArray[index] = layer;
                            // �����м���--
                            m_loadingStaticResCorutineCount--;
                            // �������ߣ���̬��Դ�������
                            OnLoadStaticResCompleted();
                        });
                }
                else
                {
                    m_currPipelineCtx.m_layerLoadedInPipe = true;

                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayer3D), layerName, layerResPath,
                        (layer) =>
                        {
                        // ����ʧ��
                        if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(), layerName));
                                return;
                            }

                        // ���سɹ���¼layer
                        m_layerArray[index] = layer;
                        // �����м���--
                        m_loadingStaticResCorutineCount--;
                        // �������ߣ���̬��Դ�������
                        OnLoadStaticResCompleted();
                        });
                }
            }
        }

        /// <summary>
        /// �������ض�̬��Դ
        /// </summary>
        protected virtual void StartLoadDynamicRes()
        {
            // ���ռ�������Ҫ����Դ
            var resPathList = CollectAllDynamicResForLoad();
            if (resPathList == null || resPathList.Count == 0)
            {
                OnLoadDynamicResCompleted();
                return;
            }

            // ���˳�������Ҫ���ص���Դ
            var resPathSet = CalculateDynamicResReallyNeedLoad(resPathList);
            if (resPathSet == null || resPathSet.Count == 0)
            {
                OnLoadDynamicResCompleted();
                return;
            }
            m_loadingDynamicResCorutineCount++;
            SimpleResourceManager.Instance.StartLoadAssetsCorutine(resPathSet, m_dynamicResCacheDict,
                () =>
                {
                    m_loadingDynamicResCorutineCount--;
                    OnLoadDynamicResCompleted();
                });

        }

        /// <summary>
        /// �ռ���Ҫ���صĶ�̬��Դ��·��
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectAllDynamicResForLoad()
        {
            return null;
        }
        /// <summary>
        /// ����̬��Դ�������
        /// </summary>
        protected virtual void OnLoadStaticResCompleted()
        {
            if (!IsLoadAllResCompleted())
            {
                return;
            }
            // ���������Դ�б�Pause����Stop����ô�ߵ�����Ͳ�Ҫ����������
            // �ڳ��Լ�����Դ��ʱ�򣬻����ȫ���������ʱ����Ҫ�ڷ���֮ǰ�Ƚ�����
            if (State == UIState.Suspend)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Paused, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                ClearRefreshViewContext();

                // ���ui�����ֹ
                EnableUIInput(true, true);

                // ֪ͨ���߸������(ʧ��)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                return;
            }
            else if (State == UIState.Stopped)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Stopped, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                ClearRefreshViewContext();

                // ���ui�����ֹ
                EnableUIInput(true, true);

                ClearAllContextAndRes();

                // ֪ͨ���߸������(ʧ��)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                return;
            }

            OnLoadAllResCompleted();
        }

        /// <summary>
        /// ����̬��Դ�������
        /// </summary>
        protected virtual void OnLoadDynamicResCompleted()
        {
            if (!IsLoadAllResCompleted())
            {
                return;
            }

            // �����task�ڼ�����Դ�Ĺ����У��Ѿ���Pause����Stop����ô�ߵ�����Ͳ�Ҫ����������
            // �ڳ��Լ�����Դ��ʱ�򣬻����ȫ���������ʱ����Ҫ�ڷ���֮ǰ�Ƚ�����
            if (State == UIState.Suspend)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Paused, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                ClearRefreshViewContext();

                // ���ui�����ֹ
                EnableUIInput(true, true);

                // ֪ͨ���߸������(ʧ��)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                // ֱ�ӷ���
                return;
            }
            else if (State == UIState.Stopped)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Stopped, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                // �����ι����ֳ�
                ClearRefreshViewContext();

                // ���ui�����ֹ
                EnableUIInput(true, true);

                // ֪ͨ���߸������(ʧ��)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                return;
            }

            OnLoadAllResCompleted();
        }

        /// <summary>
        /// ��������Դ�������
        /// </summary>
        protected virtual void OnLoadAllResCompleted()
        {
            Debug.Log(string.Format("OnLoadAllResCompleted task={0}", ToString()));

            if (m_currPipelineCtx.m_layerLoadedInPipe)
            {
                // ��ʼ��layer��״̬
                InitLayerStateOnLoadAllResCompleted();

                // ����attach���е�uictrl
                InitAllUIComponents();
            }

            // ��������
            PostOnLoadAllResCompleted();

            // ������ʾ
            StartRefreshView();
        }


        /// <summary>
        /// ��ʼ��layer��״̬
        /// </summary>
        protected virtual void InitLayerStateOnLoadAllResCompleted()
        {
            //���Ƚ�UnitySceneLayerѹջ
            // �������ô�� ��������
            foreach (var layer in m_layerArray)
            {
                var sceneLayer = layer as SceneLayerUnity;
                if (sceneLayer != null && sceneLayer.State != SceneLayerBase.LayerState.InStack)
                {
                    SceneLayerManager.Instance.PushLayer(sceneLayer);
                    HideOrShowUnitySceneLayer(sceneLayer, true);
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(sceneLayer.Scene);
                }
            }

            // ��mainlayerѹջ��ʾ
            if (MainLayer.State != SceneLayerBase.LayerState.InStack)
            {
                SceneLayerManager.Instance.PushLayer(MainLayer);
            }
        }

        /// <summary>
        /// ��OnRefreshViewEnd��ʱ�������ֳ�
        /// </summary>
        protected virtual void ClearRefreshViewContext()
        {
            m_currPipelineCtx.Clear();
        }

        /// <summary>
        /// ��ʼ�����е�uictrl
        /// </summary>
        protected virtual void InitAllUIComponents()
        {
            if (UIComponentDescArray == null ||
                UIComponentDescArray.Length == 0)
            {
                return;
            }

            // ����ctrl������
            if (m_uiCompArray == null)
            {
                m_uiCompArray = new UIComponentBase[UIComponentDescArray.Length];
            }

            // ��ǰ��μ��ص�ctrl���б�
            var currLoadinCompList = new List<UIComponentBase>();

            // ������������������ʼ��֮
            for (int i = 0; i < UIComponentDescArray.Length; i++)
            {
                // �����Ѿ���ʼ�������
                if (m_uiCompArray[i] != null)
                {
                    continue;
                }

                // ����desc�е�layer���ƻ�ȡlayer
                var desc = UIComponentDescArray[i];
                var layerDesc = GetLayerDescByName(desc.m_attachLayerName);
                SceneLayerBase layer = GetLayerByName(desc.m_attachLayerName);

                // ��û���ҵ���Ӧ��layer
                if (layer == null)
                {
                    Debug.LogError($"InitAllUIComponents fail for comp={desc.m_compName} can not find layer={desc.m_attachLayerName} in ui={GetType()}");
                    continue;
                }

                // ��ctrl attach�� layer
                UIComponentBase comp = null; // ��component ����ֱ���϶� Ҳ���Դ��������
                var sceneLayer = layer as SceneLayerUnity;
                if (sceneLayer != null && layer.LayerPrefabRoot == null)
                {
                    foreach (var go in sceneLayer.UnitySceneRootObjs)
                    {
                        if (go.name == desc.m_attachPath)
                        {
                            comp = go.GetComponent<UIComponentBase>();
                            break;
                        }
                    }
                }
                else
                {
                    comp = layer.LayerPrefabRoot.GetComponent<UIComponentBase>();
                }
                //ctrl = UIControllerBase.AddControllerToGameObject(layer.LayerPrefabRoot, desc.m_attachP
                    //ath, desc.m_ctrlTypeDNName, desc.m_ctrlName, desc.m_luaModuleName);

                if (comp == null)
                {
                    Debug.LogError("comp not found");
                    continue;
                }

                comp.Initlize(desc.m_compName);

                m_uiCompArray[i] = comp;
                currLoadinCompList.Add(comp);
            }

            // ����������ctrl֮�󣬶��α�������bind
            foreach (var ctrl in currLoadinCompList)
            {
                if (ctrl != null)
                {
                    ctrl.BindFields();
                }
            }
        }

        /// <summary>
        /// ��OnLoadAllResCompleted����
        /// </summary>
        protected virtual void PostOnLoadAllResCompleted()
        {

        }

        /// <summary>
        /// �Ƿ����е���Դ���ض������
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadAllResCompleted()
        {
            return m_loadingStaticResCorutineCount == 0 && m_loadingDynamicResCorutineCount == 0;
        }

        /// <summary>
        /// �����뼯���й��˳�������Ҫ���ص���Դ�б�
        /// </summary>
        /// <param name="resPathList"></param>
        /// <returns></returns>
        protected HashSet<string> CalculateDynamicResReallyNeedLoad(List<string> resPathList)
        {
            HashSet<string> pathSet = new HashSet<string>();

            foreach (string path in resPathList)
            {
                // �����Դ�������Ѿ������������ٴμ���
                if (string.IsNullOrEmpty(path) || m_dynamicResCacheDict.ContainsKey(path))
                {
                    continue;
                }
                pathSet.Add(path);
            }
            return pathSet;
        }
        /// <summary>
        /// ���ڼ��ؾ�̬��Դ������
        /// </summary>
        protected int m_loadingStaticResCorutineCount;

        /// <summary>
        /// ���ڼ��ض�̬��Դ���ڼ��ص�����
        /// </summary>
        protected int m_loadingDynamicResCorutineCount;

        /// <summary>
        /// ��RefreshView���������ڲ��ŵ� ������Ч��������
        /// </summary>
        protected List<PlayingRefreshViewEffectItem> m_playingRefreshViewEffectList = new List<PlayingRefreshViewEffectItem>();
        public class PlayingRefreshViewEffectItem
        {
            public string m_name;
            public float? m_timeOutTime;
            public Action<string> m_onTimeOut;
        }

        /// <summary>
        /// ��ǰ�Ĺ����ֳ�
        /// </summary>
        protected UIRefreshPipelineCtx m_currPipelineCtx;

        /// <summary>
        /// ��̬��Դ����
        /// </summary>
        protected Dictionary<string, UnityEngine.Object> m_dynamicResCacheDict = new Dictionary<string, UnityEngine.Object>();

        #endregion

        /// <summary>
        /// ����ui��������
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="isGlobalEnable"></param>
        public virtual void EnableUIInput(bool isEnable, bool isGlobalEnable)
        {
            m_isUIInputEnable = isEnable;
            //UIManager.Instance.GlobalUIInputEnable(Name, isGlobalEnable);
        }


        public virtual void InitlizeBeforeManagerStartIt()
        {
            // ���������
            m_layerArray = new SceneLayerBase[LayerDescArray.Length];
        }

        /// <summary>
        /// ���������ֳ�
        /// </summary>
        protected virtual void ClearAllContextAndRes()
        {
            // �������е�layer
            if (m_layerArray != null)
            {
                foreach (var layer in m_layerArray)
                {
                    if (layer != null)
                    {
                        SceneLayerManager.Instance.FreeLayer(layer);
                    }
                }
            }
            // ����̬��Դ
            m_dynamicResCacheDict.Clear();
        }

        #region layer component ���

        /// <summary>
        /// ��ȡָ�����Ƶ�layerdesc
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected LayerDesc GetLayerDescByName(string name)
        {
            foreach (var desc in LayerDescArray)
            {
                if (desc.m_layerName == name)
                {
                    return desc;
                }
            }
            return null;
        }

        /// <summary>
        /// ��ȡָ�����Ƶ�layer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected SceneLayerBase GetLayerByName(string name)
        {
            foreach (var layer in m_layerArray)
            {
                if (layer != null && layer.LayerName == name)
                {
                    return layer;
                }
            }
            return null;
        }

        public class LayerDesc
        {
            public string m_layerName;
            public string m_layerResPath;
            public bool m_isUILayer = true;
        }
        protected virtual LayerDesc[] LayerDescArray
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// ��layer�ľ�������ڴ����taskֻ��һ��layer
        /// </summary>
        protected virtual SceneLayerBase MainLayer { get { return (m_layerArray == null || m_layerArray.Length == 0) ? null : m_layerArray[0]; } }

        /// <summary>
        /// layer�ľ������,��һ������layer
        /// </summary>
        protected SceneLayerBase[] m_layerArray;

        public class UIComponentDesc
        {
            public string m_compName;
            public string m_compTypeName;
            public string m_attachLayerName;
            public string m_attachPath;
        }
        protected virtual UIComponentDesc[] UIComponentDescArray
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// uictrl�ľ������
        /// </summary>
        protected UIComponentBase[] m_uiCompArray;

        #endregion


        /// <summary>
        /// ״̬
        /// </summary>
        public UIState State { get; set; }

        /// <summary>
        /// ��ǰ��intent
        /// </summary>
        protected UIIntent m_currIntent;
        public UIIntent CurrentIntent { get { return m_currIntent; } }
        /// <summary>
        /// ��ǰ��ģʽ
        /// </summary>
        protected string m_currMode;

        /// <summary>
        /// �Ƿ�����ui����
        /// </summary>
        protected bool IsUIInputEnable { get { return m_isUIInputEnable && IsGlobalUIInputEnable; } }
        protected bool m_isUIInputEnable = true;

        /// <summary>
        /// ȫ����
        /// </summary>
        protected static bool IsGlobalUIInputEnable
        {
            //get { return UIManager.Instance.IsGlobalUIInputEnable(); }
            get { return false; }
        }
    }
}


