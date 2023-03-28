using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class BattleManager : MonoBehaviour
    {
        private static BattleManager m_instance;
        public static BattleManager Instance { get{ return m_instance; } }


        private void Awake()
        {
            if(m_instance == null)
            {
                m_instance = this;
            }
        }
        /// <summary>
        /// 仅供测试 
        /// </summary>
        public void BattleStart()
        {
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

        /// <summary>
        /// 是否能使用
        /// </summary>
        /// <returns></returns>
        public bool CanUseCard(CardInstanceInfo instanceInfo)
        {
            return true;
        }

        public void AddCardFromDeck(CardInstanceInfo newCard)
        {
            EventOnAddCard?.Invoke(newCard);
        }

        #region 事件

        /// <summary>
        /// 加入手牌
        /// </summary>
        public event Action<CardInstanceInfo> EventOnAddCard;

        /// <summary>
        /// 移除手牌
        /// </summary>
        public event Action<CardInstanceInfo> EventOnRemoveCard;

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

        #endregion

    }
}

