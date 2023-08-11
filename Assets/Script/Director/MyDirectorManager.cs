
using My.Framework.Runtime.Resource;
using My.Runtime;
using My.Runtime.Director;
using StreamerReborn;
using StreamerReborn.World;
using System;
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
    public class MyDirectorCutscene : DirectorCutscene
    {
        /// <summary>
        /// 准备资源
        /// </summary>
        public override void InitCutscene()
        {
            base.InitCutscene();

            var actors = GetDynamicRoot().GetComponentsInChildren<ActorControllerBase>();
            foreach (var actor in actors)
            {
                actor.BindFields();
                actor.Initialize(0, null);
            }

            UIControllerWorldOverlay.GetCurrent();
        }
    }
    /// <summary>
    /// Mono单例
    /// </summary>
    public class MyDirectorManager : DirectorManager
    {
        protected override DirectorCutscene CreateCutscene()
        {
            return new MyDirectorCutscene();
        }

        protected override void HandleTracks(TimelinePlayContext context)
        {
            var originTimeline = (TimelineAsset)context.m_contextPlayer.playableAsset;
            var rootTracks = originTimeline.GetRootTracks();
            foreach (var rootTrack in rootTracks)
            {
                var paramList = rootTrack.name.Split('.');
                switch(paramList[0])
                {
                    case "Actor":
                        {
                            if(!(rootTrack is GroupTrack groupTrack))
                            {
                                continue;
                            }
                            HandleActorTracks(groupTrack, paramList);
                        }
                        break;
                }
            }
        }
    
        protected void HandleActorTracks(GroupTrack rootTrack, string[] paramList)
        {
            if(paramList.Length < 1)
            {
                return;
            }
            int actorId;
            if(!int.TryParse(paramList[1], out actorId))
            {
                Debug.LogError("Param Error");
                return;
            }
            var subTracks = rootTrack.GetChildTracks();
            foreach (var track in subTracks)
            {
                // 自定义类型 可以直接绑定
                if (track is CutsceneTrackBase cutsceneTrack)
                {
                    cutsceneTrack.InitializeBinding(m_currCutscene.m_playableDirector, m_currCutscene);
                    continue;
                }

                // 非自定义类型 根据参数及其类型绑定
                // 基本是动画等控制类型
                switch (track.name)
                {
                    case "Bubble":
                        {
                            if (!(track is AnimationTrack animTrack))
                            {
                                break;
                            }
                            // 初始化绑定关系UISceneRoot
                            var bindingTargetGo = m_currCutscene.GetActorById(actorId);
                            if(bindingTargetGo == null)
                            {
                                Debug.LogError("Not Found BindingTarget");
                                break;
                            }
                            var actorCtrl = bindingTargetGo.GetComponent<ActorControllerBase>();
                            if(actorCtrl == null)
                            {
                                Debug.LogError("Not Found BindingTargets actorCtrl");
                                break;
                            }
                            // 创建临时资源 - bubble
                            var overlayUI = UIControllerWorldOverlay.GetCurrent();
                            var compBubble = overlayUI.FetchActorBubble(actorCtrl);
                            compBubble.gameObject.AddComponent<ExtendedAnimationTrackHandler_Bubble>();
                            actorCtrl.m_compBubble = compBubble;
                            m_currCutscene.m_playableDirector.SetGenericBinding(animTrack, compBubble.m_animator);
                        }
                        break;
                    default:
                        break;
                 }

            }
        }


        protected override void ReleaseTracks(TimelinePlayContext context)
        {
            var originTimeline = (TimelineAsset)context.m_contextPlayer.playableAsset;
            var rootTracks = originTimeline.GetRootTracks();
            foreach (var rootTrack in rootTracks)
            {
                var paramList = rootTrack.name.Split('.');
                switch (paramList[0])
                {
                    case "Actor":
                        {
                            if (!(rootTrack is GroupTrack groupTrack))
                            {
                                continue;
                            }
                            ReleaseActorTracks(groupTrack);
                        }
                        break;
                }
            }
        }

        public void ReleaseActorTracks(GroupTrack rootTrack)
        {
            var subTracks = rootTrack.GetChildTracks();

            foreach (var track in subTracks)
            {
                // 自定义类型 可以直接绑定
                if (track is CutsceneTrackBase cutsceneTrack)
                {
                    cutsceneTrack.InitializeBinding(m_currCutscene.m_playableDirector, m_currCutscene);
                    continue;
                }

                // 非自定义类型 根据参数及其类型绑定
                // 基本是动画等控制类型
                switch (track.name)
                {
                    case "Bubble":
                        {
                            if (!(track is ExtendedAnimationTrack animTrack))
                            {
                                break;
                            }
                            var animator = m_currCutscene.m_playableDirector.GetGenericBinding(animTrack) as Animator;
                            
                            var bubbleComp = animator.GetComponent<UIComponentActorBubble>();
                            if (bubbleComp == null)
                            {
                                Debug.LogError("Not Found BindingTargets UIComponentActorBubble");
                                break;
                            }
                            
                            // 销毁临时资源 - bubble
                            var overlayUI = UIControllerWorldOverlay.GetCurrent();
                            overlayUI.ReleaseActorBubble(bubbleComp, false);
                        }
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
