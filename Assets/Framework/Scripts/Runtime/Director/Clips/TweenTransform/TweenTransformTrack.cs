using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Framework.Runtime.Director
{
    [TrackColor(0.855f, 0.8623f, 0.870f)]
    [TrackClipType(typeof(TweenTransformClip))]
    [TrackBindingType(typeof(Transform))]
    public class TweenTransformTrack : CutsceneTrackBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string MyName;

        /// <summary>
        /// 
        /// </summary>
        public int BindingActorId;

        /// <summary>
        /// 初始化绑定信息
        /// </summary>
        public override void InitializeBinding(PlayableDirector director, DirectorCutscene cutscene)
        {
            // 初始化绑定关系UISceneRoot
            var bindingTargetGo = cutscene.GetActorById(BindingActorId);
            director.SetGenericBinding(this, bindingTargetGo.transform);

            // 初始化每个 Clip
            foreach (var clip in GetClips())
            {
                var realClip = (TweenTransformClip)clip.asset;
                if (realClip == null) continue;
                if (!string.IsNullOrEmpty(realClip.startNamedPoint))
                {
                    realClip.startLocation.defaultValue = cutscene.GetNamedPoint(realClip.startNamedPoint);

                }
                if (!string.IsNullOrEmpty(realClip.endNamedPoint))
                {
                    realClip.endLocation.defaultValue = cutscene.GetNamedPoint(realClip.endNamedPoint);
                }
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TweenTransformMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var comp = director.GetGenericBinding(this) as Transform;
            if (comp == null)
                return;
            var so = new UnityEditor.SerializedObject(comp);
            var iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                if (iter.hasVisibleChildren)
                    continue;
                driver.AddFromName<Transform>(comp.gameObject, iter.propertyPath);
            }
#endif
            base.GatherProperties(director, driver);
        }
    }
}


