using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using UnityEngine;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 结算源类型
    /// </summary>
    public enum EnumTriggereSourceType
    {
        Invalid = 0,
        BeforeCast,
        OnCast,
        PostCast,
        BattleStart,
        TurnStart,
        TurnEnd,
        BuffTriggered,
    }

    /// <summary>
    /// 一次操作触发的结算
    /// </summary>
    public class BattleResolveContext
    {
        public BattleResolveContext(EnumTriggereSourceType sourceType)
        {
            m_sourceType = sourceType;
        }
        /// <summary>
        /// 触发源类型
        /// </summary>
        public EnumTriggereSourceType m_sourceType;

        /// <summary>
        /// 临时数据字典 
        /// 与该次结算相关的数据
        /// </summary>
        public Dictionary<string, object> m_contextParamDict = new Dictionary<string, object>();


        /// <summary>
        /// 当前正在展开的节点列表
        /// </summary>
        public Queue<EffectNode> m_effectNodeQueue = new Queue<EffectNode>();

        ///// <summary>
        ///// 等待展开的TriggerNode列表
        ///// </summary>
        //public List<TriggerNode> m_pendingTriggerList = new List<TriggerNode>();

        /// <summary>
        /// 当前正在等待的process数量
        /// </summary>
        public int m_waitingProcessCount = 0;

        /// <summary>
        /// 是否正在处理
        /// </summary>
        public bool m_isTicking = false;

        /// <summary>
        /// 
        /// </summary>
        public Action EventOnFinish;
        /// <summary>
        /// 开启结算现场
        /// </summary>
        public void OpenResolveCtx(EnumTriggereSourceType sourceType, Action eventOnFinish)
        {
            EventOnFinish = eventOnFinish;
        }

        /// <summary>
        /// 增加触发源
        /// </summary>
        public void AddEffectNode(EffectNode node)
        {
            m_effectNodeQueue.Enqueue(node);
        }

        ///// <summary>
        ///// 增加触发源
        ///// </summary>
        //public void AddTriggerNode(TriggerNode node)
        //{
        //    m_pendingTriggerList.Add(node);
        //}
    }

    
    public interface IOptSource
    {
        string GetOpt(int resolveId);
    }

    /// <summary>
    /// 对外接口
    /// </summary>
    public interface IBattleLogicResolver
    {
        /// <summary>
        /// 开启结算现场
        /// </summary>
        BattleResolveContext OpenResolveCtx(EnumTriggereSourceType sourceType);

        /// <summary>
        /// 立即tick结算
        /// </summary>
        void TickResolve();

        /// <summary>
        /// 获取当前结算现场
        /// </summary>
        BattleResolveContext CurrContext { get; }

    }

    public class BattleLogicCompResolver : BattleLogicCompBase, IBattleLogicResolver
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
            m_compProcessManager = m_owner.CompGet<BattleLogicCompProcessManager>(GamePlayerCompNames.ProcessManager);

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

        public override void Tick(float dt)
        {
            TickResolve();
        }

        /// <summary>
        /// 立即tick结算
        /// </summary>
        public void TickResolve()
        {
            // 已经结束的战斗不再处理
            if (m_compRuler.IsFinishRuleMatch())
            {
                return;
            }

            // 尝试将pending的移动至current
            if (m_blockingResolveContext == null)
            {
                // 没有等待中
                if (m_pendingResolveContext.Count != 0)
                {
                    m_blockingResolveContext = m_pendingResolveContext[0];
                    m_pendingResolveContext.RemoveAt(0);
                    m_compProcessManager.PushProcessToCache(new BattleShowProcess_Print("new resolve context to showup"));
                }
            }


            if (m_blockingResolveContext == null)
            {
                return;
            }


            // 依次处理效果
            while (true)
            {
                if (m_compRuler.IsFinishRuleMatch())
                {
                    break;
                }
                
                // 所有节点执行完毕 结束
                if(m_blockingResolveContext.m_effectNodeQueue.Count == 0)
                {
                    m_blockingResolveContext.EventOnFinish?.Invoke();
                    m_blockingResolveContext = null;
                    break;
                }

                var effectNode = m_blockingResolveContext.m_effectNodeQueue.Peek();

                // 节点为持续型节点 需要等待结束
                if (!HandleEffect(effectNode, m_blockingResolveContext))
                {
                    break;
                }

                // 等待上层process结束
                if (effectNode.NeedWaitProcess() && m_compProcessManager.IsPlayingProcess())
                {
                    break;
                }

                m_blockingResolveContext.m_effectNodeQueue.Dequeue();
                m_compRuler.CheckBattleFinish();
            }
        }


        /// <summary>
        /// 开启结算现场
        /// </summary>
        public BattleResolveContext OpenResolveCtx(EnumTriggereSourceType sourceType)
        {
            var ctx = new BattleResolveContext(sourceType);
            m_pendingResolveContext.Add(ctx);

            return ctx;
        }


        ///// <summary>
        ///// 触发源 上层主动触发
        ///// </summary>
        ///// <param name="triggerNode"></param>
        //public void TriggerResolve(TriggerNode triggerNode)
        //{
        //    m_blockingResolveContext.AddTriggerNode(triggerNode);
        //    TickResolve();
        //}

        //public virtual void HandleTrigger(TriggerNode triggerNode, BattleResolveContext ctx)
        //{
        //    switch(triggerNode.SourceType)
        //    {
        //        case EnumTriggereSourceType.TurnStart:
        //            {
        //                var node = (TriggerNodeTurnStart)triggerNode;
        //                var actors = m_compActorContainer.GetActorsByCamp(node.ControllerId);
        //                foreach(var actor in actors)
        //                {
        //                    actor.OnTurnStart();
        //                }
        //            }
        //            break;
        //        case EnumTriggereSourceType.TurnEnd:
        //            {
        //                var node = (TriggerNodeTurnStart)triggerNode;
        //                var actors = m_compActorContainer.GetActorsByCamp(node.ControllerId);
        //                foreach (var actor in actors)
        //                {
        //                    actor.OnTurnStart();
        //                }
        //            }
        //            break;
        //        case EnumTriggereSourceType.BattleStart:
        //        {
        //            var node = (TriggerNodeBattleStart)triggerNode;

        //            foreach (var actor in m_compActorContainer.m_battleActorList)
        //            {
        //                actor.OnBattleStart();
        //            }
        //        }
        //            break;
        //        case EnumTriggereSourceType.BeforeAttack:
        //        {
        //            var node = (TriggerNodeBeforeAttack)triggerNode;
        //            var actor = m_compActorContainer.GetActor(node.AttackerId);
        //            actor.PushEvent(new LogicEventDefault(EventIDConsts.AddBuff)); //结算仅希望对buff进行触发 不应该以事件形式进行 不如直接trigger
        //        }
        //            break;
        //        case EnumTriggereSourceType.Attack:
        //        {
        //            var node = (TriggerNodeAttack)triggerNode;
        //            var actor = m_compActorContainer.GetActor(node.AttackerId);
        //            var skillInst = node.SkillInstance;
        //            // 开始释放
        //            skillInst.OnSkillCast();
        //        }
        //            break;
        //    }
        //}

        /// <summary>
        /// 处理单个效果
        /// </summary>
        /// <param name="effectNode"></param>
        /// <param name="ctx"></param>
        public virtual bool HandleEffect(EffectNode effectNode, BattleResolveContext ctx)
        {
            // 抛出事件 例如通知ui层显示对应ui
            if (!effectNode.Handled)
            {
                // 预处理 例如准备数据等
                PreHandleEffect(effectNode, ctx);
                EventOnEffectNodeHandled?.Invoke(effectNode);
                effectNode.Handled = true;
            }

            // 节点数据准备完毕
            if (!effectNode.IsDataReady)
            {
                return false;
            }


            switch (effectNode.EffectType)
            {
                case BattleEffectType.AddBuff:
                    {
                        var realNode = (EffectNodeAddBuff)effectNode;
                        uint targetId = 1;
                        var actor = m_compActorContainer.GetActor(targetId);
                        actor.AddBuff( realNode.BuffId);
                    }
                    break;
                case BattleEffectType.ChooseCard:
                {
                    var realNode = (EffectNodeChooseCard)effectNode;
                    // 增加card to hand
                    Debug.Log("CHoose card effect resolved.");
                }
                    break;
                case BattleEffectType.Damage:
                {
                    var realNode = (EffectNodeDamage)effectNode;
                    uint targetId = 1;
                    var actor = m_compActorContainer.GetActor(targetId);
                    actor.ApplyDamage(2000);
                    actor.ApplyHpStateModify();
                }
                    break;
                case BattleEffectType.Show:
                {
                    var realNode = (EffectNodeShow)effectNode;
                    m_compProcessManager.PushProcessToCache(new BattleShowProcess_Show("show info", 1f));
                    m_compProcessManager.PushBlockBarProcess();
                    m_compProcessManager.FlushAndRaiseEvent();
                }
                    break;
            }

            return true;
        }


        /// <summary>
        /// 预处理节点
        /// </summary>
        /// <param name="effectNode"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual void PreHandleEffect(EffectNode effectNode, BattleResolveContext ctx)
        {
            switch (effectNode.EffectType)
            {
                case BattleEffectType.ChooseCard:
                {
                    var realNode = (EffectNodeChooseCard)effectNode;
                    realNode.IsDataReady = false;
                    realNode.ChooseSet = new List<int>() { 2, 3, 4 };
                    return;
                }
            }
        }

        #region 结算效果



        #endregion

        /// <summary>
        /// 节点被处理
        /// 部分节点在被处理时需要通知ui层给予额外输入
        /// </summary>
        public Action<EffectNode> EventOnEffectNodeHandled;

        /// <summary>
        /// 当前结算context
        /// 仅在需要阻塞的场景下使用
        /// </summary>
        protected BattleResolveContext m_blockingResolveContext;

        /// <summary>
        /// 等待中的结算现场
        /// </summary>
        protected List<BattleResolveContext> m_pendingResolveContext = new List<BattleResolveContext>();
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
        protected BattleLogicCompProcessManager m_compProcessManager;
    }
}
