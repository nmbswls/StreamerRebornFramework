using My.Framework.Runtime.Resource;
using My.Framework.Runtime.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 管线现场
    /// </summary>
    public class UIRefreshPipelineCtx
    {
        /// <summary>
        /// 清理
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
        /// 设置更新掩码
        /// </summary>
        /// <param name="mask"></param>
        public void SetUpdateMask(ulong mask)
        {
            m_updateMask = mask;
        }

        /// <summary>
        /// 设置更新掩码
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
        /// 设置更新掩码
        /// </summary>
        public void AddUpdateMask(int index)
        {
            if (index < 64)
            {
                m_updateMask |= 1ul << index;
            }
        }

        /// <summary>
        /// 清理更新掩码
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
        /// 测试更新掩码
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
        /// 检测更新掩码是否全部处于清空状态
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateMaskClear()
        {
            return m_updateMask == 0;
        }

        /// <summary>
        /// 是否是初始化流程
        /// </summary>
        public bool m_isInitPipeLine = true;

        /// <summary>
        /// 是否是resume
        /// </summary>
        public bool m_isResume;

        /// <summary>
        /// 管线是否在运行中
        /// </summary>
        public bool m_isRuning;

        /// <summary>
        /// 当前这次pipeline是否有uilayer被加载
        /// </summary>
        public bool m_layerLoadedInPipe;

        /// <summary>
        /// 是否阻止所有的task的ui输入
        /// </summary>
        public bool m_blockGlobalUIInput = true;

        /// <summary>
        /// 更新掩码
        /// </summary>
        protected ulong m_updateMask;

        /// <summary>
        /// 回调
        /// </summary>
        public Action<bool> m_onPipelineEnd;
    }


    public abstract class UIControllerBase
    {
        /// <summary>
        /// 状态定义
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
            // 处于stop状态不应该被tick，报警
            if (State == UIState.Stopped)
            {
                return;
            }
            // 只有Running才能执行tick逻辑
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
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 显示ui
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
        /// 显示ui
        /// </summary>
        /// <returns></returns>
        public bool Resume(UIIntent intent, Action<bool> onRefreshPipelineEnd = null)
        {
            if (State != UIState.Suspend)
                return false;


            State = UIState.Running;


            m_isOpeningUI = true;

            // 显示Layer
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
        /// 挂起UI进程 不销毁 
        /// </summary>
        public void Suspend()
        {
            if (State != UIState.Running)
                return;

            // 记录暂停开始时间
            PauseStartTime = UnityEngine.Time.realtimeSinceStartup;

            // 挂起时清除ui阻塞
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

            // 在task暂停的时候，清理ui阻塞
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
        /// 运行中新需求
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
        /// 进行显示或恢复
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

            // 是否还有管线在运行中
            if (m_currPipelineCtx.m_isRuning)
            {
                return false;
            }

            if (intent != null &&
                (m_currIntent != intent || m_currMode != intent.TargetMode))
            {
                // 设置mode
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

            // 如果需要加载资源
            if (isNeedLoadStaticRes || isNeedLoadDynamicRes)
            {
                // 加载静态资源
                if (isNeedLoadStaticRes) StartLoadStaticRes();

                // 加载动态资源
                if (isNeedLoadDynamicRes) StartLoadDynamicRes();

                // UpdateView会由OnLoadAllResCompleted发起 
                return true;
            }

            // 如果不需要更新资源，直接完成
            OnLoadAllResCompleted();

            return true;
        }

        /// <summary>
        /// 解析intent为内部变量
        /// </summary>
        protected virtual void ParseIntent()
        {

        }

        /// <summary>
        /// 更新管线启动前回调
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
        /// 判断有没有layer在stack里面
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

        #region RefreshView 相关

        /// <summary>
        /// 启动更新显示
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
        /// 执行特效结束后的处理
        /// </summary>
        protected void OnAllRefreshEffectsEnd()
        {
            // 解除ui输入禁止
            EnableUIInput(true, true);

            var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

            ClearRefreshViewContext();

            // 解除ui输入禁止
            EnableUIInput(true, true);

            // 通知管线更新完成(成功)
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
        /// 构造管线现场
        /// </summary>
        /// <returns></returns>
        protected virtual UIRefreshPipelineCtx CreateRefreshPipeLineCtx()
        {
            return new UIRefreshPipelineCtx();
        }

        /// <summary>
        /// 更新 view前回调
        /// </summary>
        protected virtual void OnPreRefreshView()
        {

        }

        protected abstract void RefreshView();


        /// <summary>
        /// 更新 view后回调
        /// </summary>
        protected virtual void OnPostRefreshView()
        {

        }

        protected virtual void OnPostRefreshEffectEnd()
        {

        }


        #endregion

        #region controller控制

        /// <summary>
        /// 返回上一个UITask
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
        ///// 播放ui过程
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
        //        // 在管线中已经阻塞ui了，不用调用EnableUIInput
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
        /// 影藏所有的显示
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
        /// 设置当前的mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected virtual bool SetCurrentMode(string mode)
        {
            if (m_currMode == mode)
            {
                return true;
            }

            // 检查mode设置
            m_currMode = mode;

            return true;
        }

        #region 资源控制

        /// <summary>
        /// 是否需要更新静态资源
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadStaticRes()
        {
            return MainLayer == null;
        }

        /// <summary>
        /// 是否需要加载动态资源
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadDynamicRes()
        {
            return false;
        }

        /// <summary>
        /// 开始加载静态资源
        /// </summary>
        protected virtual void StartLoadStaticRes()
        {
            if (LayerDescArray == null || LayerDescArray.Length <= 0)
            {
                return;
            }

            var toLoadList = new List<LayerDesc>(LayerDescArray);
            // 构造layer句柄容器
            m_layerArray = new SceneLayerBase[LayerDescArray.Length];
            // 如果没有需要加载的layer了
            if (toLoadList.Count == 0)
            {
                // 继续管线，静态资源加载完成
                OnLoadStaticResCompleted();
                return;
            }
            m_loadingStaticResCorutineCount++;
            // 创建所有的layer
            for (int i = 0; i < toLoadList.Count; i++)
            {

                var layerDesc = toLoadList[i];
                string layerName = layerDesc.m_layerName;
                string layerResPath = layerDesc.m_layerResPath;
                bool isUILayer = layerDesc.m_isUILayer;
                // 闭包赋值
                int index = i;
                if (isUILayer)
                {
                    m_currPipelineCtx.m_layerLoadedInPipe = true;

                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUI), layerName, layerResPath,
                        (layer) =>
                        {
                        // 加载失败
                        if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(), layerName));
                                return;
                            }

                        // 加载成功记录layer
                        m_layerArray[index] = layer;
                        // 加载中计数--
                        m_loadingStaticResCorutineCount--;
                        // 继续管线，静态资源加载完成
                        OnLoadStaticResCompleted();
                        });
                }
                else if (layerResPath.EndsWith(".unity"))
                {
                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayerUnity), layerName, layerResPath,
                        (layer) =>
                        {
                            // 加载失败
                            if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(),
                                    layerName));
                                return;
                            }

                            // 加载成功记录layer
                            m_layerArray[index] = layer;
                            // 加载中计数--
                            m_loadingStaticResCorutineCount--;
                            // 继续管线，静态资源加载完成
                            OnLoadStaticResCompleted();
                        });
                }
                else
                {
                    m_currPipelineCtx.m_layerLoadedInPipe = true;

                    SceneLayerManager.Instance.CreateLayer(typeof(SceneLayer3D), layerName, layerResPath,
                        (layer) =>
                        {
                        // 加载失败
                        if (layer == null)
                            {
                                Debug.LogError(string.Format("Load Layer fail task={0} layer={1}", ToString(), layerName));
                                return;
                            }

                        // 加载成功记录layer
                        m_layerArray[index] = layer;
                        // 加载中计数--
                        m_loadingStaticResCorutineCount--;
                        // 继续管线，静态资源加载完成
                        OnLoadStaticResCompleted();
                        });
                }
            }
        }

        /// <summary>
        /// 启动加载动态资源
        /// </summary>
        protected virtual void StartLoadDynamicRes()
        {
            // 先收集所有需要的资源
            var resPathList = CollectAllDynamicResForLoad();
            if (resPathList == null || resPathList.Count == 0)
            {
                OnLoadDynamicResCompleted();
                return;
            }

            // 过滤出真正需要加载的资源
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
        /// 收集需要加载的动态资源的路径
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CollectAllDynamicResForLoad()
        {
            return null;
        }
        /// <summary>
        /// 当静态资源加载完成
        /// </summary>
        protected virtual void OnLoadStaticResCompleted()
        {
            if (!IsLoadAllResCompleted())
            {
                return;
            }
            // 如果加载资源中被Pause或者Stop，那么走到这里就不要继续往下了
            // 在尝试加载资源的时候，会产生全局锁，这个时候需要在返回之前先解锁。
            if (State == UIState.Suspend)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Paused, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                ClearRefreshViewContext();

                // 解除ui输入禁止
                EnableUIInput(true, true);

                // 通知管线更新完成(失败)
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

                // 解除ui输入禁止
                EnableUIInput(true, true);

                ClearAllContextAndRes();

                // 通知管线更新完成(失败)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                return;
            }

            OnLoadAllResCompleted();
        }

        /// <summary>
        /// 当动态资源加载完成
        /// </summary>
        protected virtual void OnLoadDynamicResCompleted()
        {
            if (!IsLoadAllResCompleted())
            {
                return;
            }

            // 如果该task在加载资源的过程中，已经被Pause或者Stop，那么走到这里就不要继续往下了
            // 在尝试加载资源的时候，会产生全局锁，这个时候需要在返回之前先解锁。
            if (State == UIState.Suspend)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Paused, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                ClearRefreshViewContext();

                // 解除ui输入禁止
                EnableUIInput(true, true);

                // 通知管线更新完成(失败)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                // 直接返回
                return;
            }
            else if (State == UIState.Stopped)
            {
                Debug.Log("UITaskBase::StartUpdatePipeLine::OnLoadStaticResCompleted TaskState == Stopped, Name: ");

                var onPipelineEnd = m_currPipelineCtx.m_onPipelineEnd;

                // 清理本次管线现场
                ClearRefreshViewContext();

                // 解除ui输入禁止
                EnableUIInput(true, true);

                // 通知管线更新完成(失败)
                if (onPipelineEnd != null)
                {
                    onPipelineEnd(false);
                }

                return;
            }

            OnLoadAllResCompleted();
        }

        /// <summary>
        /// 当所有资源加载完成
        /// </summary>
        protected virtual void OnLoadAllResCompleted()
        {
            Debug.Log(string.Format("OnLoadAllResCompleted task={0}", ToString()));

            if (m_currPipelineCtx.m_layerLoadedInPipe)
            {
                // 初始化layer的状态
                InitLayerStateOnLoadAllResCompleted();

                // 这里attach所有的uictrl
                InitAllUIComponents();
            }

            // 后续处理
            PostOnLoadAllResCompleted();

            // 更新显示
            StartRefreshView();
        }


        /// <summary>
        /// 初始化layer的状态
        /// </summary>
        protected virtual void InitLayerStateOnLoadAllResCompleted()
        {
            //优先将UnitySceneLayer压栈
            // 如果不这么做 有问题吗？
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

            // 将mainlayer压栈显示
            if (MainLayer.State != SceneLayerBase.LayerState.InStack)
            {
                SceneLayerManager.Instance.PushLayer(MainLayer);
            }
        }

        /// <summary>
        /// 当OnRefreshViewEnd的时候清理现场
        /// </summary>
        protected virtual void ClearRefreshViewContext()
        {
            m_currPipelineCtx.Clear();
        }

        /// <summary>
        /// 初始化所有的uictrl
        /// </summary>
        protected virtual void InitAllUIComponents()
        {
            if (UIComponentDescArray == null ||
                UIComponentDescArray.Length == 0)
            {
                return;
            }

            // 构造ctrl的容器
            if (m_uiCompArray == null)
            {
                m_uiCompArray = new UIComponentBase[UIComponentDescArray.Length];
            }

            // 当前这次加载的ctrl的列表
            var currLoadinCompList = new List<UIComponentBase>();

            // 遍历所有描述符，初始化之
            for (int i = 0; i < UIComponentDescArray.Length; i++)
            {
                // 跳过已经初始化的组件
                if (m_uiCompArray[i] != null)
                {
                    continue;
                }

                // 按照desc中的layer名称获取layer
                var desc = UIComponentDescArray[i];
                var layerDesc = GetLayerDescByName(desc.m_attachLayerName);
                SceneLayerBase layer = GetLayerByName(desc.m_attachLayerName);

                // 当没有找到对应的layer
                if (layer == null)
                {
                    Debug.LogError($"InitAllUIComponents fail for comp={desc.m_compName} can not find layer={desc.m_attachLayerName} in ui={GetType()}");
                    continue;
                }

                // 将ctrl attach到 layer
                UIComponentBase comp = null; // 绑定component 可以直接拖动 也可以代码中添加
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

            // 创建了所有ctrl之后，二次遍历进行bind
            foreach (var ctrl in currLoadinCompList)
            {
                if (ctrl != null)
                {
                    ctrl.BindFields();
                }
            }
        }

        /// <summary>
        /// 当OnLoadAllResCompleted结束
        /// </summary>
        protected virtual void PostOnLoadAllResCompleted()
        {

        }

        /// <summary>
        /// 是否所有的资源加载都完成了
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsLoadAllResCompleted()
        {
            return m_loadingStaticResCorutineCount == 0 && m_loadingDynamicResCorutineCount == 0;
        }

        /// <summary>
        /// 从输入集合中过滤出真正需要加载的资源列表
        /// </summary>
        /// <param name="resPathList"></param>
        /// <returns></returns>
        protected HashSet<string> CalculateDynamicResReallyNeedLoad(List<string> resPathList)
        {
            HashSet<string> pathSet = new HashSet<string>();

            foreach (string path in resPathList)
            {
                // 如果资源缓存中已经包含，则不用再次加载
                if (string.IsNullOrEmpty(path) || m_dynamicResCacheDict.ContainsKey(path))
                {
                    continue;
                }
                pathSet.Add(path);
            }
            return pathSet;
        }
        /// <summary>
        /// 正在加载静态资源的数量
        /// </summary>
        protected int m_loadingStaticResCorutineCount;

        /// <summary>
        /// 正在加载动态资源正在加载的数量
        /// </summary>
        protected int m_loadingDynamicResCorutineCount;

        /// <summary>
        /// 由RefreshView触发的正在播放的 动画特效过程数量
        /// </summary>
        protected List<PlayingRefreshViewEffectItem> m_playingRefreshViewEffectList = new List<PlayingRefreshViewEffectItem>();
        public class PlayingRefreshViewEffectItem
        {
            public string m_name;
            public float? m_timeOutTime;
            public Action<string> m_onTimeOut;
        }

        /// <summary>
        /// 当前的管线现场
        /// </summary>
        protected UIRefreshPipelineCtx m_currPipelineCtx;

        /// <summary>
        /// 动态资源缓存
        /// </summary>
        protected Dictionary<string, UnityEngine.Object> m_dynamicResCacheDict = new Dictionary<string, UnityEngine.Object>();

        #endregion

        /// <summary>
        /// 设置ui操作阻塞
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
            // 构造层容器
            m_layerArray = new SceneLayerBase[LayerDescArray.Length];
        }

        /// <summary>
        /// 清理所有现场
        /// </summary>
        protected virtual void ClearAllContextAndRes()
        {
            // 销毁所有的layer
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
            // 清理动态资源
            m_dynamicResCacheDict.Clear();
        }

        #region layer component 相关

        /// <summary>
        /// 获取指定名称的layerdesc
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
        /// 获取指定名称的layer
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
        /// 主layer的句柄，对于大多数task只有一个layer
        /// </summary>
        protected virtual SceneLayerBase MainLayer { get { return (m_layerArray == null || m_layerArray.Length == 0) ? null : m_layerArray[0]; } }

        /// <summary>
        /// layer的句柄数组,第一个是主layer
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
        /// uictrl的句柄数组
        /// </summary>
        protected UIComponentBase[] m_uiCompArray;

        #endregion


        /// <summary>
        /// 状态
        /// </summary>
        public UIState State { get; set; }

        /// <summary>
        /// 当前的intent
        /// </summary>
        protected UIIntent m_currIntent;
        public UIIntent CurrentIntent { get { return m_currIntent; } }
        /// <summary>
        /// 当前的模式
        /// </summary>
        protected string m_currMode;

        /// <summary>
        /// 是否允许ui输入
        /// </summary>
        protected bool IsUIInputEnable { get { return m_isUIInputEnable && IsGlobalUIInputEnable; } }
        protected bool m_isUIInputEnable = true;

        /// <summary>
        /// 全局锁
        /// </summary>
        protected static bool IsGlobalUIInputEnable
        {
            //get { return UIManager.Instance.IsGlobalUIInputEnable(); }
            get { return false; }
        }
    }
}


