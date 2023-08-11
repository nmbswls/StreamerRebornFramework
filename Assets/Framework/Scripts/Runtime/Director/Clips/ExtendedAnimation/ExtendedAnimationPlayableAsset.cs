using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Framework.Runtime.Director
{
    public class AnimationEventBehaviour : PlayableBehaviour
    {
        public GameObject target;

        public List<EventData> BeginEvents;
        public List<EventData> EndEvents;

        bool wasPlaying = false;
        float oldWeight = float.MinValue;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if(BeginEvents != null && BeginEvents.Count > 0)
            {
                target.SendMessage("OnClipStarted", BeginEvents, SendMessageOptions.DontRequireReceiver);
            }
            
            wasPlaying = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (wasPlaying)
            {
                if (EndEvents != null && EndEvents.Count > 0)
                {
                    target.SendMessage("OnClipEnded", EndEvents, SendMessageOptions.DontRequireReceiver);
                }
            }
            wasPlaying = false;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (info.effectiveWeight != oldWeight)
                target.SendMessage("OnWeightChanged", info.effectiveWeight, SendMessageOptions.DontRequireReceiver);
            oldWeight = info.effectiveWeight;
        }
    }

    public class AnimationEventPlayableAsset : AnimationPlayableAsset
    {
        

        public GameObject target;

        public List<EventData> BeginEvents;
        public List<EventData> EndEvents;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = base.CreatePlayable(graph, go);

            var target = FindTarget(go);
            if (target == null)
                return playable;

            var scriptPlayable = ScriptPlayable<AnimationEventBehaviour>.Create(graph);
            scriptPlayable.GetBehaviour().target = target;
            scriptPlayable.AddInput(playable, 0, 1.0f);
            scriptPlayable.SetPropagateSetTime(true);

            scriptPlayable.GetBehaviour().BeginEvents = BeginEvents;
            scriptPlayable.GetBehaviour().EndEvents = EndEvents;

            return scriptPlayable;
        }

        // Finds the binding gameObject. Usually this is passed to ProcessFrame, but animation tracks
        //  don't have that called.
        GameObject FindTarget(GameObject go)
        {
            var director = go.GetComponent<PlayableDirector>();
            if (director == null)
                return null;

            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null)
                return null;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (!(track is ExtendedAnimationTrack))
                    continue;

                foreach (var c in track.GetClips())
                {
                    // we belong to this track
                    if (c.asset == this)
                    {
                        var obj = director.GetGenericBinding(track);
                        if (obj == null)
                            return null;

                        // usually the binding is an animator
                        var animator = obj as Animator;
                        if (animator != null)
                            return animator.gameObject;

                        // could also be a game object, or null
                        var gameObject = obj as GameObject;
                        return gameObject;
                    }
                }
            }

            return null;
        }
    }
}


