﻿using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Framework.Runtime.Director
{
    [Serializable]
    public class TweenTransformBehaviour : PlayableBehaviour
    {
        public enum TweenType
        {
            Linear,
            Deceleration,
            Harmonic,
            Custom,
        }

        public Transform startLocation;
        public Transform endLocation;
        public bool tweenPosition = true;
        public bool tweenRotation = true;
        public TweenType tweenType;
        public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public Vector3 startingPosition;
        public Quaternion startingRotation = Quaternion.identity;

        AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        AnimationCurve m_DecelerationCurve = new AnimationCurve
        (
            new Keyframe(0f, 0f, -k_RightAngleInRads, k_RightAngleInRads),
            new Keyframe(1f, 1f, 0f, 0f)
        );
        AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        const float k_RightAngleInRads = Mathf.PI * 0.5f;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (startLocation)
            {
                startingPosition = startLocation.position;
                startingRotation = startLocation.rotation;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
        }

        public float EvaluateCurrentCurve(float time)
        {
            if (tweenType == TweenType.Custom && !IsCustomCurveNormalised())
            {
                Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
                return 0f;
            }

            switch (tweenType)
            {
                case TweenType.Linear:
                    return m_LinearCurve.Evaluate(time);
                case TweenType.Deceleration:
                    return m_DecelerationCurve.Evaluate(time);
                case TweenType.Harmonic:
                    return m_HarmonicCurve.Evaluate(time);
                default:
                    return customCurve.Evaluate(time);
            }
        }

        bool IsCustomCurveNormalised()
        {
            if (!Mathf.Approximately(customCurve[0].time, 0f))
                return false;

            if (!Mathf.Approximately(customCurve[0].value, 0f))
                return false;

            if (!Mathf.Approximately(customCurve[customCurve.length - 1].time, 1f))
                return false;

            return Mathf.Approximately(customCurve[customCurve.length - 1].value, 1f);
        }
    }
}


