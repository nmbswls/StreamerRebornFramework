using My.Framework.Runtime.Director;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Storytelling
{
    public class StoryCommandInfo_Timeline : StoryCommandInfoBase
    {
        /// <summary>
        /// timeline地址
        /// </summary>
        public string TimelineName;

        /// <summary>
        /// 解析param
        /// </summary>
        public void ParseParamString(string paramString)
        {
            var paramList = paramString.Split('#');
            if (paramList.Length >= 1)
            {
                TimelineName = paramList[0];
            }
        }
    }

    /// <summary>
    /// Say命令 - 运行时数据
    /// </summary>
    public class StoryCommandRuntimeData_Timeline : StoryCommandRuntimeData
    {
    }


    /// <summary>
    /// 播放timeline
    /// </summary>
    public class StoryCommandExecutor_Timeline : StoryCommandExecutorBase
    {
        public override StoryCommandRuntimeData CreateRuntimeData()
        {
            return new StoryCommandRuntimeData_Timeline();
        }
        public override EnumCommandExecStatus Execute(StoryCommandInfoBase commandInfo, StoryContextBase ctx, IStoryCommandEnvBase env)
        {
            var runtimeData = (StoryCommandRuntimeData_Timeline)ctx.CurrRuntimeData;
            if(!runtimeData.Inited)
            {
                runtimeData.Inited = true;
                var realCommandInfo = (StoryCommandInfo_Timeline)commandInfo;

                DirectorManager.Instance.PlayTimeline(realCommandInfo.TimelineName,
                    (ret) => {
                        if (ret)
                        {
                            runtimeData.ErrorOccured = false;
                        }
                        else
                        {
                            runtimeData.ErrorOccured = true;
                        }
                        runtimeData.IsEnd = true;
                    });
            }

            // 只有首次执行时 改变状态
            if (!runtimeData.IsEnd)
            {
                return EnumCommandExecStatus.Running;
            }
            return runtimeData.ErrorOccured ? EnumCommandExecStatus.Fail : EnumCommandExecStatus.Success;
        }
    }
}
