using System;
using JetBrains.Annotations;
using StreamerReborn.World;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Runtime.Director
{
    [Serializable]
    public class DoExpressionBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// 表情id
        /// </summary>
        public string ExpressionId;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            ActorControllerBase actor = playerData as ActorControllerBase;
            if (actor == null)
            {
                Debug.LogError("ShowBubbleBehaviour Binding Actor Error.");
                return;
            }
            float normalisedTime = (float)(playable.GetTime() / playable.GetDuration());
            actor.SampleFadeIn(normalisedTime);
        }
    }
}


