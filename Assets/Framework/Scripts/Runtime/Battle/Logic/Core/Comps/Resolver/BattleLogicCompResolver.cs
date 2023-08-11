using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public interface IProcessHolder
    {
        /// <summary>
        /// 推入process
        /// </summary>
        /// <param name="process"></param>
        void PushProcess(BattleShowProcess process);

        /// <summary>
        /// 是否正在播放
        /// </summary>
        /// <returns></returns>
        bool IsPlayingProcess();

        /// <summary>
        /// 将构筑生成的process进行播放
        /// </summary>
        void ProcessFlushAndPlay();
    }

    /// <summary>
    /// 一次操作触发的结算
    /// </summary>
    public class BattleResolveContext
    {
        /// <summary>
        /// 临时数据字典 
        /// 与该次结算相关的数据
        /// </summary>
        public Dictionary<string, object> m_contextParamDict = new Dictionary<string, object>();

        /// <summary>
        /// 当前正在展开的节点列表
        /// </summary>
        public Queue<EffectNode> m_effectNodeQueue = new Queue<EffectNode>();

        /// <summary>
        /// 等待展开的TriggerNode列表
        /// </summary>
        public List<TriggerNode> m_pendingTriggerList = new List<TriggerNode>();

        /// <summary>
        /// 当前正在等待的process数量
        /// </summary>
        public int m_waitingProcessCount = 0;

        /// <summary>
        /// 是否正在处理
        /// </summary>
        public bool m_isTicking = false;

        /// <summary>
        /// 增加触发源
        /// </summary>
        public void AddEffectNode(EffectNode node)
        {
            m_effectNodeQueue.Enqueue(node);
        }

        /// <summary>
        /// 增加触发源
        /// </summary>
        public void AddTriggerNode(TriggerNode node)
        {
            m_pendingTriggerList.Add(node);
        }
    }

    
    public interface IOptSource
    {
        string GetOpt(int resolveId);
    }

    /// <summary>
    /// 对外接口
    /// </summary>
    public interface IBattleLogicCompResolver
    {

    }

    public class BattleLogicCompResolver : BattleLogicCompBase, IBattleLogicCompResolver
    {

        public BattleLogicCompResolver(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName
        {
            get { return GamePlayerCompNames.Resolver; }
        }


        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!base.Initialize())
            {
                return false;
            }

            m_compActorContainer = m_owner.CompGet<BattleLogicCompActorContainer>(GamePlayerCompNames.ActorManager);
            m_compRuler = m_owner.CompGet<BattleLogicCompRuler>(GamePlayerCompNames.Ruler);
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            if (!base.PostInitialize())
            {
                return false;
            }


            return true;
        }

        #endregion


        protected IProcessHolder m_processHolder;
        protected IOptSource source;

        public void Tick(float dt)
        {
            TickResolve();
        }

        public void TickResolve()
        {
            // 已经结束的战斗不再处理
            if (m_compRuler.IsFinishRuleMatch())
            {
                return;
            }

            if (m_blockingResolveContext == null)
            {
                return;
            }
            // 正在ticking 不处理
            if(m_blockingResolveContext.m_isTicking)
            {
                return;
            }
            m_blockingResolveContext.m_isTicking = true;

            // 依次处理效果
            while (true)
            {
                if (m_compRuler.IsFinishRuleMatch())
                {
                    break;
                }
                if(m_blockingResolveContext.m_effectNodeQueue.Count == 0)
                {
                    if(m_blockingResolveContext.m_pendingTriggerList.Count == 0)
                    {
                        break;
                    }
                    HandleTrigger(m_blockingResolveContext.m_pendingTriggerList[0], m_blockingResolveContext);
                    continue;
                }

                var effectNode = m_blockingResolveContext.m_effectNodeQueue.Peek();

                // 节点未准备完毕
                if (!effectNode.IsReady())
                {
                    break;
                }
                
                if(!effectNode.Handled)
                {
                    // 处理effect
                    HandleEffect(effectNode, m_blockingResolveContext);
                    effectNode.Handled = true;
                }

                // 节点等待上层process结束
                if (m_processHolder.IsPlayingProcess())
                {
                    break;
                }

                m_blockingResolveContext.m_effectNodeQueue.Dequeue();
                m_compRuler.CheckBattleFinish();
            }

            m_blockingResolveContext.m_isTicking = false;
        }
        
        /// <summary>
        /// 触发源 上层主动触发
        /// </summary>
        /// <param name="triggerNode"></param>
        public void TriggerResolve(TriggerNode triggerNode)
        {
            m_blockingResolveContext.m_pendingTriggerList.Add(triggerNode);
            TickResolve();
        }

        public virtual void HandleTrigger(TriggerNode triggerNode, BattleResolveContext ctx)
        {
            switch(triggerNode.SourceType)
            {
                case EnumTriggereSourceType.TurnStart:
                    {
                        var node = (TriggerNodeTurnStart)triggerNode;
                        var actors = m_compActorContainer.GetActorsByCamp(node.ControllerId);
                        foreach(var actor in actors)
                        {
                            actor.OnTurnStart();
                        }
                    }
                    break;
                case EnumTriggereSourceType.TurnEnd:
                    {
                        var node = (TriggerNodeTurnStart)triggerNode;
                        var actors = m_compActorContainer.GetActorsByCamp(node.ControllerId);
                        foreach (var actor in actors)
                        {
                            actor.OnTurnStart();
                        }
                    }
                    break;
                case EnumTriggereSourceType.BattleStart:
                {
                    var node = (TriggerNodeBattleStart)triggerNode;
                    
                    foreach (var actor in m_compActorContainer.m_battleActorList)
                    {
                        actor.OnBattleStart();
                    }
                }
                    break;
            }
        }
        
        public virtual void HandleEffect(EffectNode effectNode, BattleResolveContext ctx)
        {
            switch(effectNode.EffectType)
            {
                case BattleEffectType.AddBuff:
                    {
                        uint actorId = 1;
                        var actor = m_compActorContainer.GetActor(actorId);
                        actor.AddBuff(1);
                    }
                    break;
            }
        }

        /// <summary>
        /// 当前结算context
        /// 仅在需要阻塞的场景下使用
        /// </summary>
        protected BattleResolveContext m_blockingResolveContext = new BattleResolveContext();
        public BattleResolveContext CurrContext { get { return m_blockingResolveContext; } }

        #region 创建effect

        //public virtual EffectNode CreateEffectNode(BattleEffectType effectType, BuildParam param = null)
        //{
        //    switch (effectType)
        //    {
        //        case BattleEffectType.AddBuff:
        //            {
        //                return new EffectNodeBuff();
        //            }
        //            break;
        //        default:
        //            {
        //                return null;
        //            }

        //    }
        //}

        #endregion

        protected BattleLogicCompRuler m_compRuler;
        protected BattleLogicCompActorContainer m_compActorContainer;
    }
}
