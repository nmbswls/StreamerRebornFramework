using My.ConfigData;
using My.Framework.Runtime.Storytelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StreamerReborn
{
    public class MyStoryBlock : StoryBlock
    {
        public ConfigDataStoryBlockInfo m_rawConfig;

        public override int StoryBlockId { get { return m_rawConfig.ID; } }

        public override List<OptionCommand> OptionCommands
        {
            get { return m_rawConfig.CommandOptionList; }
        }
    }

    public class MyStorytellingSystem : StorytellingSystemBase
    {

        /// <summary>
        /// 注册命令执行器
        /// </summary>
        protected override void RegisterCommandExecutorss()
        {
            base.RegisterCommandExecutorss();
            RegisterCommandExecutor("ShowBubble", new StoryCommandExecutor_ShowBubble());
        }

        public override bool Initialize()
        {
            if(!base.Initialize())
            {
                Debug.LogError("MyStorytellingSystem");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 对话数据
        /// </summary>
        protected override StoryBlock GetStoryBlockById(int storyBlockId)
        {
            if(m_storyBlockDict.TryGetValue(storyBlockId, out MyStoryBlock ret))
            {
                return ret;
            }
            ret = PreprocessStoryBlock(storyBlockId);
            m_storyBlockDict.Add(storyBlockId, ret);
            return ret;
        }

        #region 预处理

        /// <summary>
        /// 创建并预处理故事块
        /// </summary>
        /// <returns></returns>
        protected MyStoryBlock PreprocessStoryBlock(int storyBlockId)
        {
            var conf = GameStatic.ConfigDataLoader.GetConfigDataStoryBlockInfo(storyBlockId);
            if (conf == null)
            {
                return null;
            }
            MyStoryBlock ret = new MyStoryBlock();
            ret.m_rawConfig = conf;

            foreach(var commandConf in conf.CommandList)
            {
                StoryCommandInfoBase storyCommandBase = null;
                switch(commandConf.CommandId)
                {
                    case "Say":
                    {
                        var realCommand = new StoryCommandInfo_Say();
                        realCommand.ParseParamString(commandConf.ParamString);
                        storyCommandBase = realCommand;
                    }
                    break;
                    case "Timeline":
                        {
                            var realCommand = new StoryCommandInfo_Timeline();
                            realCommand.ParseParamString(commandConf.ParamString);
                            storyCommandBase = realCommand;
                        }
                        break;
                    case "SpawnActor":
                        {
                            var realCommand = new StoryCommandInfo_SpawnActor();
                            realCommand.ParseParamString(commandConf.ParamString);
                            storyCommandBase = realCommand;
                        }
                        break;
                    case "MoveActor":
                        {
                            var realCommand = new StoryCommandInfo_MoveActor();
                            realCommand.ParseParamString(commandConf.ParamString);
                            storyCommandBase = realCommand;
                        }
                        break;
                    case "ShowBubble":
                        {
                            var realCommand = new StoryCommandInfo_ShowBubble();
                            realCommand.ParseParamString(commandConf.ParamString);
                            storyCommandBase = realCommand;
                        }
                        break;
                    default:
                        continue;
                }
                storyCommandBase.CommandId = commandConf.CommandId;
                ret.m_commands.Add(storyCommandBase);
            }
            
            return ret;
        }

        protected Dictionary<int, MyStoryBlock> m_storyBlockDict = new Dictionary<int, MyStoryBlock>();

        #endregion

        /// <summary>
        /// 环境封装类
        /// 避免evaluator调用到不对外的方法
        /// </summary>
        protected class MyExpressionEnvWrapper : ExpressionEnvWrapper
        {
            public MyExpressionEnvWrapper(IStoryExpressionEnvBase env):base(env)
            {
            }

            #region 全部转发

            #endregion
        }
    }
}
