using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime.Storytelling
{
    /// <summary>
    /// 故事数据接口
    /// 具体存储逻辑由子类实现
    /// </summary>
    public class StoryBlock
    {
        /// <summary>
        /// 命令集合
        /// </summary>
        public List<StoryCommandInfoBase> m_commands = new List<StoryCommandInfoBase>();

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public StoryCommandInfoBase GetStoryCommand(int idx)
        {
            if (m_commands == null || idx >= m_commands.Count)
            {
                return null;
            }
            return m_commands[idx];
        }


        /// <summary>
        /// 当前故事块id
        /// </summary>
        public virtual int StoryBlockId { get; }

        /// <summary>
        /// 获取选项列表
        /// </summary>
        /// <returns></returns>
        public virtual List<OptionCommand> OptionCommands
        {
            get { return null; }
        }
    }

    /// <summary>
    /// 故事命令基类
    /// </summary>
    public class StoryCommandInfoBase
    {
        public virtual string CommandId { get; set; }

        public virtual string GetLogString()
        {
            return CommandId;
        }
    }

    /// <summary>
    /// 命令运行时数据类
    /// </summary>
    public class StoryCommandRuntimeData
    {
        /// <summary>
        /// 初始化标记
        /// </summary>
        public bool Inited;

        /// <summary>
        /// 通用结束标记位
        /// </summary>
        public bool IsEnd;

        /// <summary>
        /// 通用错误标记位
        /// </summary>
        public bool ErrorOccured;
    }

    /// <summary>
    /// 选项命令
    /// </summary>
    public class OptionCommand
    {
        public string Option { get; set; }
        public int NextStoryBlock { get; set; }
        public string Condition { get; set; }

        public OptionCommand()
        {

        }

        public OptionCommand(string optionStr)
        {
            var body = optionStr.Split('#');
            Option = body[0];
            if(body.Length >= 2)
            {
                int.TryParse(body[1], out var val);
                NextStoryBlock = val;
            }
            Condition = body.Length >= 3 ? body[2] : "";
        }
    }

    /// <summary>
    /// 故事上下文 基类
    /// </summary>
    public class StoryContextBase
    {
        /// <summary>
        /// 当前对话数据
        /// </summary>
        public StoryBlock CurrStoryBlock;

        /// <summary>
        /// 运行时数据
        /// </summary>
        public StoryCommandRuntimeData CurrRuntimeData;

        /// <summary>
        /// 当前对话下标
        /// </summary>
        public int DialogCommandIndex;

        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleID;

        /// <summary>
        /// 临时参数
        /// </summary>
        public Dictionary<string, int> tmpArgs = new Dictionary<string, int>();

        /// <summary>
        /// 可继承参数
        /// </summary>
        public Dictionary<string, int> crossDialogArgs = new Dictionary<string, int>();

        /// <summary>
        /// 自定义数据字典
        /// </summary>
        public Dictionary<string, object> customData = new Dictionary<string, object>();

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            CurrStoryBlock = null;
            DialogCommandIndex = 0;
            tmpArgs.Clear();
            customData.Clear();
        }

        /// <summary>
        /// 结束回调
        /// </summary>
        public Action OnDialogEnd;
    }

    /// <summary>
    /// 叙事命令环境
    /// </summary>
    public interface IStoryCommandEnvBase
    {
        /// <summary>
        /// 检查对话资源是否就绪
        /// </summary>
        /// <returns></returns>
        bool CheckDialogReady();

        /// <summary>
        /// 解析字符串内容
        /// </summary>
        /// <returns></returns>
        string AnalysisInlineScript(string rawText);

        /// <summary>
        /// 获取场景中点位置
        /// </summary>
        /// <param name="pointName"></param>
        /// <returns></returns>
        Transform GetNamedPoint(string pointName);

        /// <summary>
        /// 获取动态actor
        /// </summary>
        /// <param name="pointName"></param>
        /// <returns></returns>
        GameObject GetActor(int actorId);
    }

    /// <summary>
    /// 表达式环境
    /// </summary>
    public interface IStoryExpressionEnvBase
    {

    }

    /// <summary>
    /// 命令执行状态
    /// </summary>
    public enum EnumCommandExecStatus
    {
        Init,
        Success,
        Running,
        Fail,
    }

    /// <summary>
    /// 对话系统 - 指令
    /// 抛出后执行具体逻辑
    /// </summary>
    public abstract class StoryCommandExecutorBase
    {
        public abstract EnumCommandExecStatus Execute(StoryCommandInfoBase commandInfo, StoryContextBase ctx, IStoryCommandEnvBase env);

        public virtual StoryCommandRuntimeData CreateRuntimeData()
        {
            return new StoryCommandRuntimeData();
        }
    }

    /// <summary>
    /// 对话intent
    /// </summary>
    public class LaunchStoryIntent
    {
        /// <summary>
        /// 对话id
        /// </summary>
        public int StoryBlockId;
    }
}
