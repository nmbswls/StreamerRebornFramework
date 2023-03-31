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

    public class ChainNodeBase
    {
        public virtual EnumChainNodeType Type
        {
            get { return EnumChainNodeType.Invalid; }
        }
        public ChainNodeBase Next;

        public ChainNodeBase()
        {
            
        }
    }

    public class ChainNodeUseCard : ChainNodeBase
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
        // 抛出表现
    }

    public class ChainNodeAudiencePerk : ChainNodeBase
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
    /// 连锁构造现场 构造完成后才能执行
    /// </summary>
    public class ChainBuildContext
    {
        public int StartPointId;

        public List<ChainNodeBase> ChainNodes = new List<ChainNodeBase>();

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
        public BattleManager Owner;


        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dt"></param>
        public void Tick(float dt)
        {
            TicBuildChain();
        }


        /// <summary>
        /// 开启新连锁 主动使用/结算中被动效果触发
        /// </summary>
        public void CreateUseCardCtx()
        {
            var newCtx = new ChainBuildContext();
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


        public void endReact()
        {
            ChainContextList[0].m_isWaiting = false;
        }

        /// <summary>
        /// 构建连锁
        /// </summary>
        private void TicBuildChain()
        {
            if(ChainContextList.Count == 0)
            {
                return;
            }
            var chainCtx = ChainContextList[0];
            // 正在等待响应
            if (chainCtx.m_isWaiting)
            {
                return;
            }

            switch(chainCtx.StartPointId)
            {
                case 0: // 玩家攻击前
                    {
                        chainCtx.m_isWaiting = true;
                        var cardNode = (ChainNodeUseCard)chainCtx.ChainNodes[0];
                        EventOnBeforePlayerAttack?.Invoke(cardNode.m_cardInstance.InstanceId);
                    }
                    break;
            }

        }

        #region 处理状态

        public void ExcuteUseCard(ChainNodeUseCard cardNode)
        {
            var cardInfo = cardNode.m_cardInstance;

            //先确认分支 之后再后续处理
            List<int> effectToHandle = new List<int>();

            //m_currCxt.m_isWaiting = true;
        }

        /// <summary>
        /// 加buff是最多的
        /// </summary>
        /// <param name="effectId"></param>
        private void HandleOneCardEffect(int effectId)
        {
            // handle 加buff
            foreach(var pair in Owner.AudienceManager.AudienceContainer)
            {
                pair.Value.AddSatisfaction(4);
            }
        }


        #endregion

        /// <summary>
        /// 连锁结算现场
        /// </summary>
        public List<ChainBuildContext> ChainContextList = new List<ChainBuildContext>();

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

