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
        /// �������� 
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
        /// �Ƿ���ʹ��
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

        #region �¼�

        /// <summary>
        /// ��������
        /// </summary>
        public event Action<CardInstanceInfo> EventOnAddCard;

        /// <summary>
        /// �Ƴ�����
        /// </summary>
        public event Action<CardInstanceInfo> EventOnRemoveCard;

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

        #endregion

    }
}

