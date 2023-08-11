using My.Framework.Runtime.Director;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Runtime.Director
{
    /// <summary>
    /// 处理timeline消息 实现类
    /// </summary>
    public class ExtendedAnimationTrackHandler_Bubble : ExtendedAnimationTrackHandler
    {
        public UIComponentActorBubble CompBubble;
        public virtual void Awake()
        {
            CompBubble = GetComponent<UIComponentActorBubble>();
        }

        public override void OnClipStarted(List<EventData> eventDataList)
        {
            base.OnClipStarted(eventDataList);
            foreach (var eventData in eventDataList)
            {
                HandleEvent(eventData);
            }
            
        }

        public override void OnClipEnded(List<EventData> eventDataList)
        {
            base.OnClipEnded(eventDataList);
            foreach (var eventData in eventDataList)
            {
                HandleEvent(eventData);
            }
        }

        protected void HandleEvent(EventData eventData)
        {
            switch (eventData.EventName)
            {
                case "ShowBG":
                    {
                        CompBubble?.SetBubbleBGEnable(true);
                    }
                    break;
                case "HideBG":
                    {
                        CompBubble?.SetBubbleBGEnable(false);
                    }
                    break;
                case "ShowContent":
                    {
                        CompBubble?.SetBubbleContentEnable(true);
                    }
                    break;
                case "HideContent":
                    {
                        CompBubble?.SetBubbleContentEnable(false);
                    }
                    break;
            }
        }
    }
}
