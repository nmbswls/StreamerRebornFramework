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


        #region ֪ͨ��ʾ���¼��¼�

        /// <summary>
        /// ��������
        /// </summary>
        public event Action<CardInstanceInfo> EventOnAddCard;

        

        #endregion

        #region ��Ա����

        /// <summary>
        /// �����б�
        /// </summary>
        public List<long> m_deckCardList = new List<long>();

        /// <summary>
        /// ����ȫ�ֿ�Ƭ��Ϣ
        /// </summary>
        public Dictionary<long, CardInstanceInfo> m_cardInstanceDict = new Dictionary<long, CardInstanceInfo>();

        /// <summary>
        /// ���ڹ�����
        /// </summary>
        public BattleAudienceManager AudienceManager;

        public BattleCardResolveManager CardResolveManager;

        public BattleCardManager CardManager;

        #endregion



    }
}

