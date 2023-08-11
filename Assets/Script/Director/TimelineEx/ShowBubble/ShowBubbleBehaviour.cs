using System;
using JetBrains.Annotations;
using StreamerReborn.World;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Runtime.Director
{
    [Serializable]
    public class ShowBubbleBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// 气泡样式
        /// </summary>
        public string BubbleStyle;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            ActorControllerBase actor = info.output.GetUserData() as ActorControllerBase;
            if (actor == null)
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("When not playing there is no binding");
                }
                else
                {
                    Debug.LogError("ShowBubbleBehaviour Binding Actor Error.");
                }
                return;
            }
            actor.ShowHeadBubble(BubbleStyle, (float)playable.GetDuration());
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            ActorControllerBase actor = info.output.GetUserData() as ActorControllerBase;
            if (actor == null)
            {
                if(!Application.isPlaying)
                {
                    Debug.LogWarning("When not playing there is no binding");
                }
                else
                {
                    Debug.LogError("ShowBubbleBehaviour Binding Actor Error.");
                }
                
                return;
            }
            actor.HideHeadBubble();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            ActorControllerBase actor = playerData as ActorControllerBase;
            if (actor == null)
            {
                Debug.LogError("ShowBubbleBehaviour Binding Actor Error.");
                return;
            }
            //actor.m_compBubble.SampleShowBubble((float)playable.GetTime());
        }
    }
}


