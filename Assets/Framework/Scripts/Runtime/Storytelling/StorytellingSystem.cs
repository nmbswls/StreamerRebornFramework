using My.ConfigData;
using My.Framework.Runtime.Director;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace My.Framework.Runtime.Storytelling
{
    
    /// <summary>
    /// 对话系统基类
    /// </summary>
    public class StorytellingSystemBase :  IStoryCommandEnvBase, IStoryExpressionEnvBase
    {

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual bool Initialize()
        {
            m_expressionEvaluator = new ExpressionEvaluator();
            m_expressionEnvWrapper = CreateExpressionEnvWrapper();

            m_expressionEvaluator.Context = m_expressionEnvWrapper;
            RegisterCommandExecutorss();
            return true;
        }

        /// <summary>
        /// tick一下
        /// </summary>
        public void Tick()
        {
            ProcessCommands();
        }


        /// <summary>
        /// 是否正在运行
        /// </summary>
        /// <returns></returns>
        public bool IsDialogRunning()
        {
            return m_runningCxt != null;
        }



        #region env方法

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Transform GetDynamicRoot()
        {
            return DirectorManager.Instance.m_currCutscene?.GetDynamicRoot();
        }

        #endregion

        #region 对外接口

        /// <summary>
        /// 切换跳转
        /// </summary>
        /// <param name="storyBlockId"></param>
        /// <param name="env"></param>
        public void SwitchStoryBlock(int storyBlockId, StoryContextBase context)
        {
            var nextDialog = GetStoryBlockById(storyBlockId);
            if (nextDialog == null)
            {
                Debug.LogWarning($"对话 {storyBlockId} 不存在。");
                CompleteDialog(context);
            }
            // 重设现场
            context.Clear();
            context.CurrStoryBlock = nextDialog;

            // 准备资源
            OnDialogStart(storyBlockId);
        }

        /// <summary>
        /// 开启对话
        /// </summary>
        /// <param name="storyBlockId"></param>
        public void LaunchStoryBlock(int storyBlockId, Action onDialogEnd = null)
        {
            var intent = CreateDefaultIntent(storyBlockId);
            LaunchStoryBlock(intent, onDialogEnd);
        }

        /// <summary>
        /// 开启对话
        /// </summary>
        /// <param name="dialogId"></param>
        public void LaunchStoryBlock(LaunchStoryIntent intent, Action onDialogEnd = null)
        {
            // 如果不在运行 则进行运行
            // 是否统一到异步流程里？
            if (!IsRunningDialog)
            {
                StartDialogImpl(intent, onDialogEnd);
            }
            else
            {
                PendingDialogQueue.Enqueue(intent);
            }
        }

        /// <summary>
        /// 结束一次对话
        /// </summary>
        public void CancelDialog(StoryContextBase context)
        {
            if(m_runningCxt == null)
            {
                return;
            }
            PendingDialogQueue.Clear();
            OnAllDialogComplete = null;
            CompleteDialog(context);
        }

        #endregion

        #region 内部条件检查

        /// <summary>
        /// 检查条件
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool CheckCondition(string condition)
        {
            return true;
        }

        #endregion

        #region 内部方法


        /// <summary>
        /// 执行叙事命令
        /// </summary>
        protected void ProcessCommands()
        {
            
            // 当前无对话现场，直接返回
            if (m_runningCxt == null)
            {
                return;
            }

            // 持续运行命令 直到出现指令全部执行完毕
            // 或是出现running状态指令
            while (m_runningCxt.DialogCommandIndex < m_runningCxt.CurrStoryBlock.m_commands.Count)
            {
                var currCommand = m_runningCxt.CurrStoryBlock.m_commands[m_runningCxt.DialogCommandIndex];
                var execRet = RunDialogCommand(currCommand, m_runningCxt);

                // 如果是持续命令在等待或运行，延迟到下一帧执行
                if (execRet == EnumCommandExecStatus.Running)
                {
                    break;
                }

                // 执行结果为失败
                if (execRet == EnumCommandExecStatus.Fail)
                {
                    CancelDialog(m_runningCxt);
                    break;
                }

                // 如果不是最后一条命令 则继续运行
                if (m_runningCxt.DialogCommandIndex < m_runningCxt.CurrStoryBlock.m_commands.Count - 1)
                {
                    m_runningCxt.DialogCommandIndex += 1;
                    m_runningCxt.CurrRuntimeData = null;
                    continue;
                }

                int jumpStoryBlock = 0;
                // check是否有后续选项
                var haveOption = RunDialogOption(m_runningCxt, ref jumpStoryBlock);

                // 有默认跳转事件时，进行跳转
                if (jumpStoryBlock != 0)
                {
                    SwitchStoryBlock(jumpStoryBlock, m_runningCxt);
                }
                else
                {
                    CompleteDialog(m_runningCxt);
                }
                break;
            }
        }

        /// <summary>
        /// 运行剧情指令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="environment"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EnumCommandExecStatus RunDialogCommand(StoryCommandInfoBase command, StoryContextBase context)
        {
            if (m_registerExecutors.TryGetValue(command.CommandId, out var CommandExecutor))
            {
                try
                {
                    // 初始化运行时数据
                    if(context.CurrRuntimeData == null)
                    {
                        context.CurrRuntimeData = CommandExecutor.CreateRuntimeData();
                    }
                    return CommandExecutor.Execute(command, context, this);
                }
                catch (Exception e)
                {
                    throw new ArgumentNullException($"指令 {command.GetLogString()} 执行错误！", e);
                }
            }
            else
            {
                throw new ArgumentNullException($"指令 {command.GetLogString()} 不存在！");
            }
        }

        /// <summary>
        /// 显示对话跳转选项
        /// </summary>
        /// <param name="optionCommands"></param>
        /// <param name="env"></param>
        /// <param name="jumpEvent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected bool RunDialogOption(StoryContextBase context, ref int nextBlock)
        {
            List<OptionCommand> optionCommands = context.CurrStoryBlock.OptionCommands;

            ClearOptions();
            var haveOption = false;
            foreach (var optionCommand in optionCommands)
            {
                // 如果存在默认跳转事件，赋值
                if (optionCommand.Option == "Default")
                {
                    nextBlock = optionCommand.NextStoryBlock;
                    continue;
                }

                try
                {
                    if (CheckCondition(optionCommand.Condition))
                    {
                        haveOption = true;
                        AddOption(optionCommand.Option, () =>
                        {
                            if (optionCommand.NextStoryBlock != 0)
                                SwitchStoryBlock(optionCommand.NextStoryBlock, null);
                            else
                                CompleteDialog(context);
                        });
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"选项 [{optionCommand.Option}]" +
                                                $" {optionCommand.Condition} 触发判断失败！请检查表达式是否正确！", e);
                }
            }

            return haveOption;
        }

        /// <summary>
        /// 完成对话
        /// </summary>
        /// <param name="context"></param>
        protected void CompleteDialog(StoryContextBase context)
        {
            // 异常结束时 context可能为空
            // 否则正常调用结束回调
            if (context != null)
            {
                if (context.OnDialogEnd != null)
                {
                    context.OnDialogEnd.Invoke();
                }
            }
            if (PendingDialogQueue.Count > 0)
            {
                var dialogIntent = PendingDialogQueue.Dequeue();
                StartDialogImpl(dialogIntent);
            }
            else
            {
                IsRunningDialog = false;
                OnAllDialogComplete?.Invoke();
                OnAllDialogComplete = null;
                m_runningCxt = null;
                OnDialogFinish();
            }
        }

        /// <summary>
        /// 内部根据intent开始一个对话
        /// </summary>
        /// <param name="dialogIntent"></param>
        protected void StartDialogImpl(LaunchStoryIntent dialogIntent, Action onDialogEnd = null)
        {
            m_runningCxt = CreateDialogContext(dialogIntent);
            if (onDialogEnd != null)
            {
                m_runningCxt.OnDialogEnd = onDialogEnd;
            }

            OnDialogStart(dialogIntent.StoryBlockId);

            // 触发一次执行逻辑
            // 如果没执行完毕，会在之后的tick中继续执行
            ProcessCommands();
        }


        /// <summary>
        /// 解析输入字符串中所有复杂表达式
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string AnalysisInlineScript(string text)
        {
            StringBuilder finallyText = new StringBuilder(text);
            // 匹配[&和&]之间的内容
            Regex regex = new Regex(@"\[&(?<expression>[\s\S]*?)&]");

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var expression = match.Groups["expression"].Value;
                var getValue = m_expressionEvaluator.Evaluate(expression).ToString();
                finallyText.Replace(match.Value, getValue);
            }
            return finallyText.ToString();
        }

        public Transform GetNamedPoint(string pointName)
        {
            return DirectorManager.Instance.m_currCutscene.GetNamedPoint(pointName);
        }

        public GameObject GetActor(int actorId)
        {
            return DirectorManager.Instance.m_currCutscene.GetActorById(actorId);
        }

        #endregion

        #region 显示层回调


        /// <summary>
        /// 是否准备好必要资源
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckDialogReady()
        {
            var dialogCtrl = UIManager.Instance.FindUIControllerByName("Dialog") as UIControllerDialogSimple;
            if(dialogCtrl == null)
            {
                UIControllerDialogSimple.InitDialog("");
                return false;
            }

            if(dialogCtrl.IsAnyPipelineRunning())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 准备对话窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        protected virtual void OnDialogStart(int storyBlockId)
        {
            
        }

        /// <summary>
        /// 对话结束
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        protected virtual void OnDialogFinish()
        {
            var dialog = UIManager.Instance.FindUIControllerByName("Dialog") as UIControllerDialogSimple;
            dialog?.Suspend();
        }

        /// <summary>
        /// 添加option
        /// 回调给显示层
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        protected virtual void AddOption(string text, Action callback)
        {

        }

        /// <summary>
        /// 清理option
        /// 通知显示层清理选项
        /// </summary>
        protected virtual void ClearOptions()
        {

        }

        #endregion

        #region 供子类继承的方法

        /// <summary>
        /// 注册命令执行器
        /// </summary>
        protected virtual void RegisterCommandExecutorss()
        {
            RegisterCommandExecutor("Say", new StoryCommandExecutor_Say());
            RegisterCommandExecutor("Timeline", new StoryCommandExecutor_Timeline());
            RegisterCommandExecutor("SpawnActor", new StoryCommandExecutor_SpawnActor());
            RegisterCommandExecutor("MoveActor", new StoryCommandExecutor_MoveActor());
        }

        /// <summary>
        /// 对话数据
        /// </summary>
        protected virtual StoryBlock GetStoryBlockById(int storyBlockId)
        {
            return null;
        }

        /// <summary>
        /// 根据对话id创建基本intent
        /// </summary>
        /// <param name="dialogId"></param>
        /// <returns></returns>
        protected virtual LaunchStoryIntent CreateDefaultIntent(int storyBlockId)
        {
            return new LaunchStoryIntent() { StoryBlockId = storyBlockId };
        }

        /// <summary>
        /// 根据intent创建对应context
        /// </summary>
        /// <param name="dialogIntent"></param>
        /// <returns></returns>
        protected virtual StoryContextBase CreateDialogContext(LaunchStoryIntent dialogIntent)
        {
            var dialogData = GetStoryBlockById(dialogIntent.StoryBlockId);
            return new StoryContextBase() { CurrStoryBlock = dialogData };
        }

        #endregion

        #region 缓存依赖


        #endregion

        #region 内部变量

        /// <summary>
        /// 复杂逻辑计算
        /// </summary>
        protected ExpressionEvaluator m_expressionEvaluator;

        /// <summary>
        /// 环境封装类
        /// 避免evaluator调用到不对外的方法
        /// </summary>
        protected class ExpressionEnvWrapper : IStoryExpressionEnvBase
        {
            public IStoryExpressionEnvBase m_env;
            public ExpressionEnvWrapper(IStoryExpressionEnvBase env)
            {
                m_env = env;
            }

            
            #region 转发调用

            #endregion
        }

        /// <summary>
        /// 创建解析表达式环境封装wrapper
        /// </summary>
        /// <returns></returns>
        protected virtual ExpressionEnvWrapper CreateExpressionEnvWrapper()
        {
            return new ExpressionEnvWrapper(this);
        }


        /// <summary>
        /// 复杂逻辑计算
        /// </summary>
        protected ExpressionEnvWrapper m_expressionEnvWrapper;

        #endregion

        #region 内部状态

        /// <summary>
        /// 正在运行的现场
        /// </summary>
        protected StoryContextBase m_runningCxt;
        /// <summary>
        /// 等待运行的对话列表
        /// </summary>
        protected Queue<LaunchStoryIntent> PendingDialogQueue = new Queue<LaunchStoryIntent>();

        /// <summary>
        /// 是否正在运行
        /// ToDo 和runningctx合并
        /// </summary>
        public bool IsRunningDialog = false;

        /// <summary>
        /// 所有事件结束回调
        /// </summary>
        public event Action OnAllDialogComplete;

        #endregion

        #region 注册命令

        /// <summary>
        /// 注册执行器
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="executor"></param>
        public void RegisterCommandExecutor(string commandId, StoryCommandExecutorBase executor)
        {
            m_registerExecutors[commandId] = executor;
        }

        

        /// <summary>
        /// 对话注册事件
        /// </summary>
        protected Dictionary<string, StoryCommandExecutorBase> m_registerExecutors = new Dictionary<string, StoryCommandExecutorBase>();

        #endregion
    }


}
