using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StreamerReborn
{
    public class BattleCardContainer : UIComponentBase
    {
        [Serializable]
        public class BattleCardContainerSettings
        {
            /// <summary>
            /// �����Ų�ʱ �������ҿ���Բ���϶�Ӧ�Ƕ� angle
            /// </summary>
            public float LayoutDegree = 8;

            public float CardMoveSpdDegree = 5f;
            public float CardMoveSpd = 3000f;

            /// <summary>
            /// Ĭ�ϼ��
            /// </summary>
            public float DefaultInterval = 0.1f;

            public float DefaultIntervalDis = 220f;
        }

        enum EnumHandCardsLayoutMode
        {
            Line,
            Arcc,
        }

        private EnumHandCardsLayoutMode HandCardsLayoutMode = EnumHandCardsLayoutMode.Line; // ����ģʽ 0 ���� 1 ˮƽ

        private GameObject m_cardPrefab;

        /// <summary>
        /// ����
        /// </summary>
        public BattleCardContainerSettings Settings = new BattleCardContainerSettings();


        #region ˽�г�Ա

        public float m_width;

        #region �����Ų�����

        /// <summary>
        /// �뾶
        /// </summary>
        public float m_arcRadius;

        #endregion


        #endregion



        /// <summary>
        /// ��ɰ󶨻ص�
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            m_width = Root.rect.width;

            if(HandCardsLayoutMode == EnumHandCardsLayoutMode.Arcc)
            {
                m_arcRadius = m_width * 0.5f / Mathf.Sin(Settings.LayoutDegree * 0.5f * Mathf.Deg2Rad);
            }

            RegisterEvent();
        }


        /// <summary>
        /// ע���¼�
        /// </summary>
        private void RegisterEvent()
        {
            //BattleManager.Instance.EventOnAddCard += OnAddCard;
            //BattleManager.Instance.EventOnAddCard += OnRemoveHandCard;
        }

        /// <summary>
        /// ��ע���¼�
        /// </summary>
        private void UnRegisterEvent()
        {
            //BattleManager.Instance.EventOnAddCard -= OnAddCard;
            //BattleManager.Instance.EventOnAddCard += OnRemoveHandCard;
        }

        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dTime"></param>
        public override void Tick(float dTime)
        {
            TickHands(dTime);
        }

        /// <summary>
        /// �ŵ����Ƶ�λ��
        /// </summary>
        /// <param name="card"></param>
        public void PutToInitPos(BattleCard card)
        {
            card.PosDirty = true;
            card.TargetPositionInHand = 1;
        }

        #region ��ʾ֪ͨ�¼�

        /// <summary>
        /// �����ֿ�
        /// </summary>
        /// <param name="instanceInfo"></param>
        /// <returns></returns>
        public void OnAddCard(CardInstanceInfo instanceInfo)
        {
            //GameStatic.ResourceManager.LoadAssetSync<Ga>
            GameObject cardGo = GameObject.Instantiate(m_cardPrefab, NormalRoot);
            if (cardGo == null)
            {
                return;
            }
            var card = cardGo.GetComponent<BattleCard>();
            card.BindFields();
            //card.Init(cardInfo, this);
            HandCards.Add(card);
            
            // ����1 ��������
            if(card.GetCardType() == 1)
            {
                card.NowPositionInHand = 0;
            }
            // ����2 ���Ҳ����
            else
            {
                card.NowPositionInHand = 1;
            }

            // �������� ����1���� ����2 ����
            ReArrangeHandCards();

            

            // ����λ��
            AdjustHandCards();
        }


        /// <summary>
        /// �Ƴ��ֿ��� ����
        /// </summary>
        /// <param name="cardHandIdx"></param>
        public void OnRemoveHandCard(CardInstanceInfo instanceInfo)
        {
            int indexInHand = -1;
            for(int i=0;i<HandCards.Count;i++)
            {
                if(HandCards[i].InstanceInfo.InstanceId == instanceInfo.InstanceId)
                {
                    indexInHand = i;
                    break;
                }
            }
            if(indexInHand == -1)
            {
                Debug.LogError($"OnRemoveHandCard Card Already Removed {instanceInfo.InstanceId}");
                return;
            }
            HandCards[indexInHand].Disappaer();
            // �Ƿ���л���
            HandCards.RemoveAt(indexInHand);
            AdjustHandCards();
        }


        #endregion


        public void RecycleCard(GameObject go)
        {
            //GameObject.Destroy(go);
            if(m_cachedBattleCard.Count > 10)
            {
                GameObject.Destroy(go);
                return;
            }
            go.SetActive(false);
            go.transform.SetParent(Pool);
            m_cachedBattleCard.Enqueue(go);
        }

        /// <summary>
        /// ��������λ��
        /// </summary>
        private void AdjustHandCards()
        {
            // ���㵱ǰ���������µ��Ų����
            float interval = Settings.DefaultInterval;
            if (HandCards.Count >= 10)
            {
                interval = 1 / HandCards.Count;
            }

            // ��ʼλ��
            float startPosition = 0.5f - ((HandCards.Count - 1) * 0.5f * interval);

            // �������ֿ��е��Ų�λ��
            for (int i = 0; i < HandCards.Count; i++)
            {
                float targetPosition = startPosition + i * interval;
                HandCards[i].TargetPositionInHand = targetPosition;
                HandCards[i].PosDirty = true;
            }
            RefreshHandCardsOrder();
        }


        /// <summary>
        /// ˢ��ǰ���ڵ�˳��
        /// </summary>
        public void RefreshHandCardsOrder()
        {
            // ��������˳��
            for (int i = 0; i < HandCards.Count; i++)
            {
                var card = HandCards[i];
                if(card.IsHightLight)
                {
                    card.transform.parent = HightLightRoot;
                }
                else
                {
                    card.transform.parent = NormalRoot;
                }
                card.transform.SetSiblingIndex(i);
            }
        }

        

        #region �ڲ�����

        /// <summary>
        /// tick����
        /// </summary>
        /// <param name="dTime"></param>
        protected void TickHands(float dTime)
        {
            for (int i = 0; i < HandCards.Count; i++)
            {
                var card = HandCards[i];
                card.Tick(dTime);

                // λ���ޱ仯�Ĳ�������
                if (!card.PosDirty || Mathf.Abs(card.TargetPositionInHand - card.NowPositionInHand) <= 1e-6)
                {
                    card.PosDirty = false;
                    continue;
                }

                // ���
                if (HandCardsLayoutMode == EnumHandCardsLayoutMode.Arcc)
                {
                    
                    card.NowPositionInHand += Mathf.Sign(card.TargetPositionInHand - card.NowPositionInHand) * dTime * 1;
                    if (card.NowPositionInHand < 0) card.NowPositionInHand = 0;
                    if(card.NowPositionInHand > 1) card.NowPositionInHand = 1;
                    float degree = (Settings.LayoutDegree) * card.NowPositionInHand - Settings.LayoutDegree * 0.5f;
                    Vector2 pos = LocalPositionGetByDrgreeInHand(card.NowPositionInHand);

                    card.Root.localEulerAngles = new Vector3(0, 0, -degree);
                }
                else
                {
                    
                }
            }

            CheckHandCardHighlight();
        }


        
        /// <summary>
        /// ���߹�
        /// </summary>
        protected void CheckHandCardHighlight()
        {
            bool allHandReach = true;
            for (int i = 0; i < HandCards.Count; i++)
            {
                if(HandCards[i].PosDirty)
                {
                    allHandReach = false;
                }
                HandCards[i].IsHightLight = false;
            }
            // �����Ƶ���ָ��λ��ʱ�ſɸ���
            if(!allHandReach)
            {
                return;
            }

            var currBattleCard = GetMouseOverBattleCard();
            if(currBattleCard != null)
            {
                currBattleCard.IsHightLight = true;

                RefreshHandCardsOrder();
            }

        }

        #endregion

        #region �ڲ�����

        /// <summary>
        /// ͨ�������еĽǶȼ��㿨������λ�� 
        /// </summary>
        /// <param name="position">0-1֮��Ĺ�һ��λ��</param>
        /// <returns></returns>
        protected Vector2 LocalPositionGetByDrgreeInHand(float position)
        {
            float degree = (Settings.LayoutDegree) * position - Settings.LayoutDegree * 0.5f;

            return new Vector2(Mathf.Sin(degree * Mathf.Deg2Rad) * m_arcRadius, Mathf.Cos(degree * Mathf.Deg2Rad) * m_arcRadius - m_arcRadius);
        }

        /// <summary>
        /// ��ȡ��������Ŀ���
        /// </summary>
        /// <returns></returns>
        protected BattleCard GetMouseOverBattleCard()
        {
            EventSystem uiEventSystem = EventSystem.current;
            if(uiEventSystem == null)
            {
                return null;
            }
            PointerEventData eventData = new PointerEventData(uiEventSystem);
            eventData.position = Input.mousePosition;
            uiEventSystem.RaycastAll(eventData, m_cacheRaycastList);
            if(m_cacheRaycastList.Count > 0)
            {
                foreach(var obj in m_cacheRaycastList)
                {
                    if(obj.gameObject.name == "Card")
                    {
                        return obj.gameObject.GetComponent<BattleCard>();
                    }
                }
            }
            return null;
        }
        
        private List<RaycastResult> m_cacheRaycastList = new List<RaycastResult>();

        /// <summary>
        /// �������� 1�������� 2��������
        /// </summary>
        /// <returns></returns>
        protected void ReArrangeHandCards()
        {
            var newList = new List<BattleCard>();
            int leftCardCount = 0;
            for(int i=0;i<HandCards.Count;i++)
            {
                if(HandCards[i].GetCardType() == 1)
                {
                    newList.Insert(leftCardCount, HandCards[i]);
                    leftCardCount += 1;
                }
                else
                {
                    newList.Add(HandCards[i]);
                }
            }
            HandCards = newList;
        }



        #endregion

        /// <summary>
        /// ������
        /// </summary>
        public List<BattleCard> HandCards = new List<BattleCard>();



        #region ����

        /// <summary>
        /// ����
        /// </summary>
        private Queue<GameObject> m_cachedBattleCard = new Queue<GameObject>();

        #endregion

        #region ������

        [AutoBind("./")]
        public RectTransform Root;

        [AutoBind("./Cards")]
        public RectTransform NormalRoot;

        [AutoBind("./CardsHightLight")]
        public RectTransform HightLightRoot;

        [AutoBind("./Pool")]
        public RectTransform Pool;

        #endregion
    }
}

