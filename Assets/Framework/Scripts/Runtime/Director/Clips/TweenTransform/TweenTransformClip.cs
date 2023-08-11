using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Framework.Runtime.Director
{
    [Serializable]
    public class TweenTransformClip : PlayableAsset, ITimelineClipAsset
    {
        public TweenTransformBehaviour template = new TweenTransformBehaviour();
        public ExposedReference<Transform> startLocation;
        public ExposedReference<Transform> endLocation;
        public string startNamedPoint;
        public string endNamedPoint;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TweenTransformBehaviour>.Create(graph, template);
            TweenTransformBehaviour clone = playable.GetBehaviour();
            clone.startLocation = startLocation.Resolve(graph.GetResolver());
            clone.endLocation = endLocation.Resolve(graph.GetResolver());
            return playable;
        }
    }
}


