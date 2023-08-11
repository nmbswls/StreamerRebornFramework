using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Runtime.Director
{
    [Serializable]
    public class ShowBubbleClip : PlayableAsset
    {
        /// <summary>
        /// 气泡样式
        /// </summary>
        public string BubbleStyle;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ShowBubbleBehaviour>.Create(graph);
            ShowBubbleBehaviour clone = playable.GetBehaviour();
            clone.BubbleStyle = BubbleStyle;
            return playable;
        }
    }
}


