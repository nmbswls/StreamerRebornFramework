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

        // 抛出表现
    }

    public class ChainNodeUseCard : ChainNodeBase
    {
        public CardInstanceInfo m_cardInstance;
        public uint Target;

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

    public class ChainContext
    {
        public ChainNodeBase HeadNode;
        public bool m_needBreak;
        public bool m_isWaiting; // 能否挪到上层

        public void Clear()
        {
            m_needBreak = false;
            m_isWaiting = false;
        }
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
            HandleChain();
        }


        /// <summary>
        /// 将卡加入连锁
        /// </summary>
        public void PutCardInChain()
        {
            ChainNodeBase newNode = new ChainNodeUseCard();
            ChainHeadNodes.Add(newNode);
        }

        /// <summary>
        /// 将Perk加入连锁
        /// </summary>
        public void PushPerk(int perkInfo)
        {
            var newNode = new ChainNodeAudiencePerk(perkInfo);
            ChainHeadNodes.Add(newNode);
        }


        private void HandleChain()
        {
            if(ChainHeadNodes.Count != 0)
            {
                var iter = ChainHeadNodes[0];
                m_currCxt.Clear();
                while (iter != null)
                {
                    switch(iter.Type)
                    {
                        case EnumChainNodeType.Card:
                            {
                                var cardNode = (ChainNodeUseCard)iter;
                                ExcuteUseCard(cardNode);
                            }
                            break;
                        case EnumChainNodeType.AudienceSatisfied:
                            {

                            }
                            break;
                    }

                    iter = iter.Next;
                }

                iter = ChainHeadNodes[0];

                // 处理完毕后执行路径上的清理
                while (iter != null)
                {
                    switch (iter.Type)
                    {
                        case EnumChainNodeType.Card:
                            {
                                var cardNode = (ChainNodeUseCard)iter;
                                Owner.CardManager.RemoveCardToDiscarded(cardNode.m_cardInstance);
                            }
                            break;
                    }

                    iter = iter.Next;
                }
            }
        }

        #region 处理状态

        public void ExcuteUseCard(ChainNodeUseCard cardNode)
        {
            var cardInfo = cardNode.m_cardInstance;

            //先确认分支 之后再后续处理
            List<int> effectToHandle = new List<int>();

            foreach (var effect in effectToHandle)
            {
                HandleOneCardEffect(effect);
            }
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

        /// <summary>
        /// 连锁结算现场
        /// </summary>
        private ChainContext m_currCxt;

        #endregion


        public List<ChainNodeBase> ChainHeadNodes = new List<ChainNodeBase>();
    }
}

