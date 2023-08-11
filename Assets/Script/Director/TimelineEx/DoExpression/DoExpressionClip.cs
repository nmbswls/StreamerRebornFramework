using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Runtime.Director
{
    [Serializable]
    public class DoExpressionClip : PlayableAsset
    {
        /// <summary>
        /// 表情id
        /// </summary>
        public string ExpressionId;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DoExpressionBehaviour>.Create(graph);
            DoExpressionBehaviour clone = playable.GetBehaviour();
            clone.ExpressionId = ExpressionId;
            return playable;
        }
    }
}


