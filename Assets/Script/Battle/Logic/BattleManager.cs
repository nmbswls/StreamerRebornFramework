using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class BattleManager
    {
        public void Initialize()
        {
            CardManager = new BattleCardManager();
            CardResolveManager = new BattleCardResolveManager();
            AudienceManager = new BattleAudienceManager();


            CardManager.Owner = this;
            CardResolveManager.Owner = this;
            AudienceManager.Owner = this;
        }
        
        public void Tick()
        {
            if (IsWait4Confirm())
            {
                return;
            }
        }

        

        /// <summary>
        ///  
        /// </summary>
        public void BattleStart()
        {
            var battleProcess = GameStatic.GameProcessManager.GetBattleProcess();


            AudienceManager.CreateBattleAudience(1, 0);
            AudienceManager.CreateBattleAudience(1, 0);
            {
                var info = new CardInstanceInfo();
                info.InstanceId = 0;
                info.Config = GameStatic.ConfigDataLoader.GetConfigDataCardBattleInfo(100);
                AddCardFromDeck(info);
            }
            {
                var info = new CardInstanceInfo();
                info.InstanceId = 1;
                info.Config = GameStatic.ConfigDataLoader.GetConfigDataCardBattleInfo(104);
                AddCardFromDeck(info);
            }

        }

        public void Tick(float dTime)
        {
            

        }


        public bool IsWait4Confirm()
        {
            return false;
        }

        

        public void AddCardFromDeck(CardInstanceInfo newCard)
        {
            EventOnAddCard?.Invoke(newCard);
        }


        #region 通知显示层事件事件

        /// <summary>
        /// 加入手牌
        /// </summary>
        public event Action<CardInstanceInfo> EventOnAddCard;

        

        #endregion

        #region 成员变量

        /// <summary>
        /// 卡组列表
        /// </summary>
        public List<long> m_deckCardList = new List<long>();

        /// <summary>
        /// 保存全局卡片信息
        /// </summary>
        public Dictionary<long, CardInstanceInfo> m_cardInstanceDict = new Dictionary<long, CardInstanceInfo>();

        /// <summary>
        /// 观众管理器
        /// </summary>
        public BattleAudienceManager AudienceManager;

        public BattleCardResolveManager CardResolveManager;

        public BattleCardManager CardManager;

        #endregion



    }
}

