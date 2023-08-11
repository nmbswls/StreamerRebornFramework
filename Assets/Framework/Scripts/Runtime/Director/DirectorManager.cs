
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace My.Framework.Runtime.Director
{
    /// <summary>
    /// 导演cutscene基类
    /// </summary>
    public class DirectorCutscene
    {
        /// <summary>
        /// 是否就绪
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// 是否结束
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 已加载场景
        /// </summary>
        public UnityEngine.SceneManagement.Scene? m_scene = null;

        /// <summary>
        /// 是否是正常结束
        /// </summary>
        public Action<bool> m_actionOnEnd;

        /// <summary>
        /// CameraWrapper
        /// </summary>
        public CameraWrapper m_cameraWrapper;

        /// <summary>
        /// 场景自带的director
        /// 如果有涉及到多个同时播放
        /// 应该使用多个playable director封装timeline
        /// 然后用总playable director 处理timeline
        /// TODO 应该是个list 分别绑定 分别播放
        /// </summary>
        public PlayableDirector m_playableDirector;

        #region 生命周期方法

        public virtual void OnBefore()
        {
            var currState = GameManager.Instance.GameWorld.GetCurrState();
            currState.Pause();
        }

        public virtual void OnAfter()
        {
            var currState = GameManager.Instance.GameWorld.GetCurrState();
            currState.Resume();
        }

        #endregion

        /// <summary>
        /// 初始化cutscene
        /// move into cutscene class
        /// </summary>
        public virtual void InitCutscene()
        {
            var rootGameObjects = m_scene.Value.GetRootGameObjects();

            var camera = Array.Find(rootGameObjects, go => go.name == "Camera").GetComponent<Camera>();
            var virtualCameraRoot = Array.Find(rootGameObjects, go => go.name == "VirtualCameraRoot");
            m_cameraWrapper = new CameraWrapper(camera, virtualCameraRoot);

            var playableDirectorGo = Array.Find(rootGameObjects, go => go.name == "PlayableDirector");
            if (playableDirectorGo != null) { m_playableDirector = playableDirectorGo.GetComponent<PlayableDirector>(); }
            
        }


        public Transform GetDynamicRoot()
        {
            var rootGameObjects = m_scene.Value.GetRootGameObjects();
            var dynamicRoot = Array.Find(rootGameObjects, go => go.name == "DynamicRoot");
            return dynamicRoot.transform;
        }

        public GameObject GetActorById(int actorId)
        {
            if (m_scene == null) return null;

            var rootGameObjects = m_scene.Value.GetRootGameObjects();
            var dynamicRoot = Array.Find(rootGameObjects, go => go.name == "DynamicRoot");
            if (dynamicRoot == null) return null;

            var target = dynamicRoot.transform.Find(actorId + "");
            if (target == null)
            {
                return null;
            }
            return target.gameObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namedPointName"></param>
        /// <returns></returns>
        public Transform GetNamedPoint(string namedPointName)
        {
            if (m_scene == null) return null;

            var rootGameObjects = m_scene.Value.GetRootGameObjects();
            var namedPointsRoot = Array.Find(rootGameObjects, go => go.name == "NamedPoints");
            if (namedPointsRoot == null) return null;

            var target = namedPointsRoot.transform.Find(namedPointName);
            if (target == null)
            {
                return null;
            }
            return target;
        }
    }

    /// <summary>
    /// Mono单例
    /// </summary>
    public class DirectorManager : MonoSingleton<DirectorManager>
    {
        public TimelineAsset TestAsset;
        /// <summary>
        /// 外部赋值？准备场景后处理
        /// </summary>
        public PlayableDirector playableDirector;

        protected override void Awake()
        {
            base.Awake();
            playableDirector = GetComponent<PlayableDirector>();
            playableDirector.stopped += OnPlayableDirectorStopped;
            m_currPlayContext = new TimelinePlayContext();
        }


        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                SetupCutscene(0);
                //PlayTimeline("");
            }
        }

        /// <summary>
        /// 准备演员 可以是scene
        /// 如果改成iemulator
        /// </summary>
        public void SetupCutscene(int cutsceneId, Action<bool> onEnd = null)
        {
            StartCoroutine(PlayCutscene(cutsceneId, onEnd));
        }

        /// <summary>
        /// 协程方式开始播放cutscene
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <param name="onEnd"></param>
        /// <returns></returns>
        public IEnumerator PlayCutscene(int cutsceneId, Action<bool> onEnd = null)
        {
            if (m_currCutscene != null)
            {
                Debug.LogError("SetupCutscene Fail. IsPlaying");
                yield break;
            }

            m_currCutscene = CreateCutscene();
            m_currCutscene.OnBefore();
            m_currCutscene.m_actionOnEnd = onEnd;

            string scenePath = ScenePathGetById(cutsceneId);
            int entryStoryBlock = EntryDialogBlockGetById(cutsceneId);

            // 加载scene
            yield return SimpleResourceManager.Instance.LoadUnityScene(scenePath,
                (scenePath, scene) =>
                {
                    // 加载失败
                    if (scene == null)
                    {
                        Debug.LogError(string.Format("Load unity scene fail task={0} layer={1}", ToString(),
                            scenePath));
                        return;
                    }
                    m_currCutscene.IsReady = true;
                    m_currCutscene.m_scene = scene.Value;
                });

            // 检查是否加载失败
            if (!m_currCutscene.IsReady)
            {
                m_currCutscene.m_actionOnEnd?.Invoke(false);
                ReleaseCutscene();
                yield break;
            }

            m_currCutscene.InitCutscene();
            var oldCutscene = m_currCutscene;

            GameManager.Instance.StorytellingSystem.LaunchStoryBlock(entryStoryBlock, () => {
                if (m_currCutscene != oldCutscene)
                {
                    Debug.LogError("something wrong happened. cutscene has changed when story block end.");
                    return;
                }
                oldCutscene.IsEnd = true;
            });
            
            // 等待cutscene结束
            while(!m_currCutscene.IsEnd)
            {
                // other check condition
                yield return null;
            }

            m_currCutscene.m_actionOnEnd?.Invoke(true);

            var showEffect = UIControllerScreenEffectSimple.ShowScreenEffect(0);
            while(!showEffect.IsLayerInStack())
            {
                yield return null;
            }

            bool waitEffect = true;
            showEffect.FadeEnterBlack(0.3f, ()=> { waitEffect = false; });

            // 当卸载过快时，等待黑屏过程结束
            while (waitEffect)
            {
                yield return null;
            }

            var asyncOp = SceneManager.UnloadSceneAsync(m_currCutscene.m_scene.Value);
            while(!asyncOp.isDone)
            {
                yield return null;
            }
            ReleaseCutscene();

            showEffect.FadeQuitBlack(0.3f);
        }

        /// <summary>
        /// 准备场景需要做什么
        /// </summary>
        public virtual void PrepareScene(Action onPrepareEnd)
        {
            // 获取当前scene handler 并判断类型是否正确
            var currState = GameManager.Instance.GameWorld.GetCurrState();
            if (currState.StateType != GameWorldStateTypeDefineBase.SimpleHall)
            {
                return;
            }
            GameWorldStateSimpleHall state = (GameWorldStateSimpleHall)currState;
            var handler = state.GetMainSceneHandler();
            // 禁用当前需要被禁用的对象
            // 例如:禁用所有角色

            handler.m_dynamicRoot.gameObject.SetActive(false);
            GameObject tempRoot = new GameObject("tempRoot");
            tempRoot.transform.parent = handler.MainRootGameObject.transform;

            // 加载scene
            SimpleResourceManager.Instance.StartLoadSceneCorutine("Assets/Scenes/Cutscene_01.unity",
                (scenePath, scene) =>
                {
                    // 加载失败
                    if (scene == null)
                    {
                        Debug.LogError(string.Format("Load unity scene fail task={0} layer={1}", ToString(),
                            scenePath));
                        return;
                    }
                    m_currCutscene = new DirectorCutscene();
                    m_currCutscene.m_scene = scene.Value;

                }, loadAync:true);

            // 准备需要的NPC等 
        }

        /// <summary>
        /// 创建cutscene对象
        /// </summary>
        protected virtual DirectorCutscene CreateCutscene()
        {
            return new DirectorCutscene();
        }

        /// <summary>
        /// 结束场景
        /// 不包括卸载场景 卸载场景需要异步
        /// TODO 看具体怎么抽出方法
        /// </summary>
        protected void ReleaseCutscene()
        {
            m_currCutscene?.OnAfter();
            m_currCutscene = null;
        }

        #region 实现类cutscene

        

        /// <summary>
        /// 根据cutsceneid 获取信息
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <returns></returns>
        protected virtual string ScenePathGetById(int cutsceneId)
        {
            return "Assets/Scenes/Cutscene_01.unity";
        }

        /// <summary>
        /// 根据cutsceneid 获取信息
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <returns></returns>
        protected virtual int EntryDialogBlockGetById(int cutsceneId)
        {
            return 10000;
        }

        #endregion

        /// <summary>
        /// 当前cutscene封装
        /// </summary>
        public DirectorCutscene m_currCutscene = null;



        #region 播放timeline

        /// <summary>
        /// 检查播放状态
        /// </summary>
        protected void OnPlayableDirectorStopped(PlayableDirector playableDirector)
        {
            if (!m_currPlayContext.m_isRunning)
            {
                return;
            }

            ReleaseTracks(m_currPlayContext);

            m_currPlayContext.m_onPlayEnd?.Invoke(true);
            m_currPlayContext.Clear();
        }

        /// <summary>
        /// 播放timeline
        /// </summary>
        /// <param name="timelineAsset"></param>
        public void PlayTimeline(string timelineName, Action<bool> onEnd = null)
        {
            
            var tempTimeline = TimelineAsset.CreateInstance<TimelineAsset>();

            var track = tempTimeline.CreateTrack<ControlTrack>(null, "cutscene");
            var clip = track.CreateClip<ControlPlayableAsset>();
            var playableClip = (ControlPlayableAsset)(clip.asset);
            clip.duration = m_currCutscene.m_playableDirector.playableAsset.duration;
            playableDirector.SetReferenceValue(playableClip.sourceGameObject.exposedName, m_currCutscene.m_playableDirector.gameObject);
            playableDirector.playableAsset = tempTimeline;


            m_currPlayContext.m_name = timelineName;
            m_currPlayContext.m_onPlayEnd = onEnd;
            m_currPlayContext.m_isRunning = true;
            m_currPlayContext.m_contextPlayer = m_currCutscene.m_playableDirector;
            HandleTracks(m_currPlayContext);

            playableDirector.Play();
        }

        protected virtual void HandleTracks(TimelinePlayContext context)
        {

        }

        /// <summary>
        /// 释放track 资源
        /// </summary>
        /// <param name="originTimeline"></param>
        protected virtual void ReleaseTracks(TimelinePlayContext context)
        {

        }

        /// <summary>
        /// PausePlay
        /// </summary>
        public void PausePlay(int src)
        {
            m_pauseCounter += 1;
            // 当0-1时 执行暂停逻辑
            if (m_pauseCounter == 1)
            {
                playableDirector.Pause();
            }
        }

        /// <summary>
        /// ResumePlay
        /// </summary>
        public void ResumePlay(int src)
        {
            m_pauseCounter -= 1;
            if (m_pauseCounter == 0)
            {
                playableDirector.Resume();
            }
        }

        /// <summary>
        /// 取消当前播放
        /// </summary>
        public void CancelTimeline()
        {
            if (!m_currPlayContext.m_isRunning)
            {
                return;
            }
            m_currPlayContext.m_onPlayEnd?.Invoke(false);
            m_currPlayContext.Clear();
        }

        /// <summary>
        /// 暂停计数器
        /// </summary>
        protected int m_pauseCounter;

        /// <summary>
        /// 一个播放现场
        /// 保存播放者 以及 初始化信息
        /// </summary>
        protected class TimelinePlayContext
        {
            public string m_name;
            public bool m_isRunning;

            public PlayableDirector m_contextPlayer;

            public Action<bool> m_onPlayEnd;

            public void Clear()
            {
                m_name = string.Empty;
                m_isRunning = false;
                m_onPlayEnd = null;
            }
        }

        /// <summary>
        /// Todo 应该支持各场景自己有director 然后分别播放 回调
        /// </summary>
        protected TimelinePlayContext m_currPlayContext;

        #endregion

    }
}
