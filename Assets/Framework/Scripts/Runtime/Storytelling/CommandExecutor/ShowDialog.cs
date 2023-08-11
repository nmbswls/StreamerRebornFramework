using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Storytelling
{

    /// <summary>
    /// Say命令 - 配置信息
    /// </summary>
    public class StoryCommandInfo_Say : StoryCommandInfoBase
    {
        /// <summary>
        /// 说话者
        /// </summary>
        public string Speaker;

        /// <summary>
        /// 对话内容
        /// </summary>
        public string Content;


        public bool IsAppend;
        public bool IsPending;

        /// <summary>
        /// 解析param
        /// </summary>
        public void ParseParamString(string paramString)
        {
            var paramList = paramString.Split('#');
            if (paramList.Length >= 1)
            {
                Content = paramList[0];
            }
            if (paramList.Length >= 2)
            {
                Speaker = paramList[1];
            }
            if (paramList.Length >= 3)
            {
                string flags = paramList[2];
                if (flags.Contains("A"))
                {
                    IsAppend = true;
                }
                if (flags.Contains("P"))
                {
                    IsPending = true;
                }
            }
        }

        public override string GetLogString()
        {
            return $"{Speaker} : {Content}";
        }
    }

    /// <summary>
    /// Say命令 - 运行时数据
    /// </summary>
    public class StoryCommandRuntimeData_Say : StoryCommandRuntimeData
    {
    }

    // <summary>
    /// Say命令 - 命令执行器
    /// </summary>
    public class StoryCommandExecutor_Say : StoryCommandExecutorBase
    {
        public override StoryCommandRuntimeData CreateRuntimeData()
        {
            return new StoryCommandRuntimeData_Say();
        }
        public override EnumCommandExecStatus Execute(StoryCommandInfoBase command, StoryContextBase ctx, IStoryCommandEnvBase env)
        {
            var runtimeData = (StoryCommandRuntimeData_Say)ctx.CurrRuntimeData;

            // 等待对话资源准备完毕
            if (!env.CheckDialogReady())
            {
                return EnumCommandExecStatus.Running;
            }

            if (!runtimeData.Inited)
            {
                runtimeData.Inited = true;

                var reslCommand = (StoryCommandInfo_Say)command;
                // 解析内容字符串
                var content = env.AnalysisInlineScript(reslCommand.Content);

                UIControllerDialogSimple.Say(content, reslCommand.Speaker, () => {
                    runtimeData.IsEnd = true;
                });
            }
            
            if(runtimeData.IsEnd)
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
