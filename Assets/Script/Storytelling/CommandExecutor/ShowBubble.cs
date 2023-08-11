
using My.Runtime;
using StreamerReborn;
using StreamerReborn.World;
using UnityEngine;

namespace My.Framework.Runtime.Storytelling
{
    public class StoryCommandInfo_ShowBubble : StoryCommandInfoBase
    {
        public int ActorId;
        public string BubbleStyle;
        public float Duration;

        /// <summary>
        /// 解析param
        /// </summary>
        public void ParseParamString(string paramString)
        {
            var paramList = paramString.Split(';');
            foreach (var item in paramList)
            {
                var itemPram = item.Split('#');
                if (itemPram.Length < 3)
                {
                    continue;
                }
                if (!int.TryParse(itemPram[0], out var actorId))
                {
                    continue;
                }
                this.ActorId = actorId;
                BubbleStyle = itemPram[1];
                if(!float.TryParse(itemPram[2], out float duration) || duration <= 0.01f)
                {
                    duration = 1.0f;
                }
                Duration = duration;
            }
        }
    }


    /// <summary>
    /// Say命令 - 运行时数据
    /// </summary>
    public class StoryCommandRuntimeData_ShowBubble : StoryCommandRuntimeData
    {
        public ActorControllerBase m_actor; // actor移动
        public UIComponentActorBubble m_compBubble; // 对应bubble
    }

    /// <summary>
    /// 显示头顶bubble
    /// 要求脱手显示
    /// </summary>
    public class StoryCommandExecutor_ShowBubble : StoryCommandExecutorBase
    {

        public override StoryCommandRuntimeData CreateRuntimeData()
        {
            return new StoryCommandRuntimeData_ShowBubble();
        }
        public override EnumCommandExecStatus Execute(StoryCommandInfoBase commandInfo, StoryContextBase ctx, IStoryCommandEnvBase env)
        {
            var runtimeData = (StoryCommandRuntimeData_ShowBubble)ctx.CurrRuntimeData;
            var realCommandInfo = (StoryCommandInfo_ShowBubble)commandInfo;

            if (!runtimeData.Inited)
            {
                runtimeData.Inited = true;

                var targetActor = env.GetActor(realCommandInfo.ActorId);
                if(targetActor == null)
                {
                    return EnumCommandExecStatus.Fail;
                }
                runtimeData.m_actor = targetActor.GetComponent<ActorControllerBase>();
                if(runtimeData.m_actor == null)
                {
                    return EnumCommandExecStatus.Fail;
                }
                // 创建临时资源 - bubble
                var overlayUI = UIControllerWorldOverlay.GetCurrent();
                var compBubble = overlayUI.FetchActorBubble(runtimeData.m_actor);

                runtimeData.m_compBubble = compBubble;

                runtimeData.m_compBubble.ShowBubble(realCommandInfo.BubbleStyle, realCommandInfo.Duration, () => {
                    runtimeData.IsEnd = true;
                });
                return EnumCommandExecStatus.Running;
            }

            if(!runtimeData.IsEnd)
            {
                return EnumCommandExecStatus.Running;
            }
            
            return EnumCommandExecStatus.Success;
        }
    }
}
