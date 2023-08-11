using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public enum EnumChainNodeType
    {
        Invalid,
        Card,
        Buff,
        AudiencePerk,
        AudienceSatisfied,
        AudienceLeave,
    }

    public enum ChainExecStatus
    {
        Invalid,
        Success,
        Waiting,
    }

    /// <summary>
    /// 结算节点
    /// </summary>
    public class ResolveNodeBase
    {

        public virtual EnumChainNodeType Type
        {
            get { return EnumChainNodeType.Invalid; }
        }

        public bool IsResolving { get; set; }

        public ResolveNodeBase()
        {
            
        }

    }

    public class ChainNodeUseCard : ResolveNodeBase
    {
        public CardInstanceInfo m_cardInstance;
        public uint Target;

        /// <summary>
        /// 当前point
        /// </summary>
        private int m_currPoint = 0;

        public ChainNodeUseCard() : base()
        {

        }

        public override EnumChainNodeType Type
        {
            get { return EnumChainNodeType.Card; }
        }
    }

    public class ChainNodeAudiencePerk : ResolveNodeBase
    {
        public int perkInfo;
        public ChainNodeAudiencePerk(int perkInfo) : base()
        {
            this.perkInfo = perkInfo;
        }

        public override EnumChainNodeType Type
        {
            get { return EnumChainNodeType.AudiencePerk; }
        }
    }

    /// <summary>
    /// 卡片结算现场
    /// </summary>
    public class CardResolveContext
    {
        public List<ResolveNodeBase> ChainNodes = new List<ResolveNodeBase>();

        public bool m_needBreak;

        public bool m_isWaiting; // 能否挪到上层

        //public void Clear()
        //{
        //    HeadNode = null;
        //    Iter = null;

        //    m_needBreak = false;
        //    m_isWaiting = false;
        //}
    }

    /// <summary>
    /// 结算卡片效果
    /// </summary>
    public class BattleCardResolveManager
    {
        //public BattleManager Owner;
        public IBattleProcessHandler BattleProcessHandler;

        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dt"></param>
        public void Tick(float dt)
        {
            if(Context != null)
            {
                while (Context.ChainNodes.Count > 0)
                {
                    var firstNode = Context.ChainNodes[0];
                    HandleResolve(firstNode);
                    // 正在解算中 跳出
                    if (firstNode.IsResolving)
                    {
                        break;
                    }
                    Context.ChainNodes.RemoveAt(0);
                };
            }
        }


        /// <summary>
        /// 开启新连锁 主动使用/结算中被动效果触发
        /// </summary>
        public void CreateUseCardCtx()
        {
            var newCtx = new CardResolveContext();
            ChainContextList.Add(newCtx);
        }

        /// <summary>
        /// 加入连锁
        /// </summary>
        public void AddChain()
        {
        }

        /// <summary>
        /// 将Perk加入连锁
        /// </summary>
        public void PushPerkTrigger(int perkInfo)
        {
            ChainContextList[0].ChainNodes.Add(new ChainNodeAudiencePerk(perkInfo));
        }

        #region 处理状态


        public void HandleResolve(ResolveNodeBase node)
        {
            switch(node.Type)
            {
                case EnumChainNodeType.Card:
                    {
                        ExcuteUseCard((ChainNodeUseCard)node);
                    }
                    break;
            }
        }

        public void ExcuteUseCard(ChainNodeUseCard cardNode)
        {
            var cardInfo = cardNode.m_cardInstance;

            //先确认分支 之后再后续处理
            List<int> effectToHandle = new List<int>();

            BattleProcessHandler.PushProcess(new BattleProcessPlayCardEffect(cardInfo.InstanceId, 0.5f));

            cardNode.IsResolving = true;
        }

        /// <summary>
        /// 加buff是最多的
        /// </summary>
        /// <param name="effectId"></param>
        private void HandleOneCardEffect(int effectId)
        {
            //// handle 加buff
            //foreach(var pair in Owner.AudienceManager.AudienceContainer)
            //{
            //    pair.Value.AddSatisfaction(4);
            //}
        }


        #endregion

        public CardResolveContext Context = null;

        /// <summary>
        /// 连锁结算现场
        /// </summary>
        public List<CardResolveContext> ChainContextList = new List<CardResolveContext>();

        /// <summary>
        /// 时点触发 玩家攻击前
        /// </summary>
        public event Action<int> EventOnWaitPlayerReact;

        /// <summary>
        /// 时点触发 玩家行动卡前
        /// </summary>
        public event Action<uint> EventOnBeforePlayerAttack;
    }
}

