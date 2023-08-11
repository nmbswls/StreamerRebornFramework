using My.Framework.Runtime.Director;
using My.Framework.Runtime.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime.Storytelling
{
    public class StoryCommandInfo_MoveActor : StoryCommandInfoBase
    {
        public int ActorId;
        public string FromNamedPoint;
        public string TargetNamedPoint;
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
                if (itemPram.Length < 4)
                {
                    continue;
                }
                if (!int.TryParse(itemPram[0], out var actorId))
                {
                    continue;
                }
                this.ActorId = actorId;
                FromNamedPoint = itemPram[1];
                TargetNamedPoint = itemPram[2];
                if(!float.TryParse(itemPram[3], out float duration) || duration <= 0.01f)
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
    public class StoryCommandRuntimeData_MoveActor : StoryCommandRuntimeData
    {
        public float m_timer;
        public Vector3 m_fromWorldPos;
        public Vector3 m_toWorldPos;
        public GameObject m_movingActor; // actor移动
    }

    /// <summary>
    /// 刷新actor
    /// </summary>
    public class StoryCommandExecutor_MoveActor : StoryCommandExecutorBase
    {

        public override StoryCommandRuntimeData CreateRuntimeData()
        {
            return new StoryCommandRuntimeData_MoveActor();
        }
        public override EnumCommandExecStatus Execute(StoryCommandInfoBase commandInfo, StoryContextBase ctx, IStoryCommandEnvBase env)
        {
            var runtimeData = (StoryCommandRuntimeData_MoveActor)ctx.CurrRuntimeData;
            var realCommandInfo = (StoryCommandInfo_MoveActor)commandInfo;

            if (!runtimeData.Inited)
            {
                runtimeData.Inited = true;

                var targetActor = env.GetActor(realCommandInfo.ActorId);
                if(targetActor == null)
                {
                    runtimeData.ErrorOccured = true;
                    runtimeData.IsEnd = true;
                    return EnumCommandExecStatus.Fail;
                }
                runtimeData.m_movingActor = targetActor;

                runtimeData.m_fromWorldPos = targetActor.transform.position;
                if (!string.IsNullOrEmpty(realCommandInfo.FromNamedPoint))
                {
                    var fromTrans = env.GetNamedPoint(realCommandInfo.FromNamedPoint);
                    if(fromTrans != null)
                    {
                        runtimeData.m_fromWorldPos = fromTrans.position;
                    }
                }
                runtimeData.m_toWorldPos = targetActor.transform.position;
                if (!string.IsNullOrEmpty(realCommandInfo.TargetNamedPoint))
                {
                    var targetTrans = env.GetNamedPoint(realCommandInfo.TargetNamedPoint);
                    if (targetTrans != null)
                    {
                        runtimeData.m_toWorldPos = targetTrans.position;
                    }
                }
            }

            runtimeData.m_timer += Time.deltaTime;
            // 移动
            runtimeData.m_movingActor.transform.position = Vector3.Lerp(runtimeData.m_fromWorldPos, runtimeData.m_toWorldPos, Mathf.Clamp(runtimeData.m_timer / realCommandInfo.Duration, 0, 1));
            if (runtimeData.m_timer >= realCommandInfo.Duration)
            {
                return EnumCommandExecStatus.Success;
            }
            else
            {
                return EnumCommandExecStatus.Running;
            }
        }
    }
}
