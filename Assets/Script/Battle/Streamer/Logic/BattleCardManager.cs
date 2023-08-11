using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    /// <summary>
    /// 管理卡组本身
    /// </summary>
    public class BattleCardManager
    {
        //public BattleManager Owner;
        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dt"></param>
        public void Tick(float dt)
        {
            
        }


        /// <summary>
        /// 等待移除列表
        /// </summary>
        private List<uint> m_cardToRemove = new List<uint>();

        /// <summary>
        /// 是否能使用
        /// </summary>
        /// <returns></returns>
        public bool CanUseCard(CardInstanceInfo instanceInfo)
        {
            return true;
        }


        public bool UseCard(CardInstanceInfo instanceInfo)
        {
            if(!CanUseCard(instanceInfo))
            {
                return false;
            }
            //Owner.CardResolveManager.PutCardInChain();
            return true;
        }

        /// <summary>
        /// 卡片信息
        /// </summary>
        /// <param name="instanceInfo"></param>
        public void RemoveCardToDiscarded(CardInstanceInfo instanceInfo)
        {
            HandCards.Remove(instanceInfo.InstanceId);
            DiscardCards.Add(instanceInfo.InstanceId);

            EventOnRemoveCard?.Invoke(instanceInfo);
        }

        /// <summary>
        /// 移除手牌
        /// </summary>
        public event Action<CardInstanceInfo> EventOnRemoveCard;

        #region 内部变量

        /// <summary>
        /// 所有永久类卡牌信息
        /// </summary>
        public Dictionary<uint, CardInstanceInfo> AllCardDict = new Dictionary<uint, CardInstanceInfo>();

        /// <summary>
        /// 手牌卡表
        /// </summary>
        public List<uint> HandCards = new List<uint>();

        /// <summary>
        /// 弃牌堆
        /// </summary>
        public List<uint> DiscardCards = new List<uint>();

        protected uint m_currentInstId;

        #endregion
    }
}

