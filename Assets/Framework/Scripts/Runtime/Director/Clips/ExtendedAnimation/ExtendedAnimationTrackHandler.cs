using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Director
{
    [Serializable]
    public class EventData
    {
        public string EventName;
        public string EventParams;

        public override string ToString()
        {
            return $"EventName:{EventName} EventParams:{EventParams}";
        }
    }

    public class ExtendedAnimationTrackHandler : MonoBehaviour
    {
        public virtual void OnClipStarted(List<EventData> eventDataList)
        {
            foreach(var eventData in eventDataList)
            {
                Debug.Log($"OnClipStarted with eventData {eventData}");
            }
        }

        public virtual void OnClipEnded(List<EventData> eventDataList)
        {

            foreach (var eventData in eventDataList)
            {
                Debug.Log($"OnClipEnded with eventData {eventData}");
            }
        }

        public void OnWeightChanged(float newWeight)
        {
            Debug.Log("OnWeightChanged = " + newWeight);
        }
    }
}


