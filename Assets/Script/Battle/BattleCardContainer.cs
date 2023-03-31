using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static StreamerReborn.UIComponentBattleCard;

namespace StreamerReborn
{
    public class BattleCardContainer : UIComponentBase
    {
        [Serializable]
        public class BattleCardContainerSettings
        {
            /// <summary>
            /// 弧形排布时 最左最右卡在圆弦上对应角度 angle
            /// </summary>
            public float LayoutDegree = 8;

            public float CardMoveSpdDegree = 5f;
            public float CardMoveSpd = 3000f;

            /// <summary>
            /// 默认间隔
            /// </summary>
            public float DefaultInterval = 0.16f;

            public float DefaultIntervalDis = 220f;
        }

        enum EnumHandCardsLayoutMode
        {
            Arcc,
            Line,
        }

        private EnumHandCardsLayoutMode HandCardsLayoutMode = EnumHandCardsLayoutMode.Arcc; // 排列模式 0 弧形 1 水平

        private GameObject m_cardPrefab;

        /// <summary>
        /// 设置
        /// </summary>
        public BattleCardContainerSettings Settings = new BattleCardContainerSettings();


        #region 私有成员

        public float m_width;

        #region 弧形排布数据

        /// <summary>
        /// 半径
        /// </summary>
        public float m_arcRadius;

        #endregion


        #endregion

        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            m_width = NormalRoot.rect.width;

            if(HandCardsLayoutMode == EnumHandCardsLayoutMode.Arcc)
            {
                m_arcRadius = m_width * 0.5f / Mathf.Sin(Settings.LayoutDegree * 0.5f * Mathf.Deg2Rad);
            }

            m_cardPrefab = GameStatic.ResourceManager.LoadAssetSync<GameObject>("Assets/RuntimeAssets/UI/UIPrefab/Battle/BattleCard.prefab");
            RegisterEvent();


            BtnPreviewConfirm.onClick.AddListener(delegate() { 
            
            
            });

            BtnPreviewCancel.onClick.AddListener(delegate () {

                if(PreviewCards.Count == 0)
                {
                    return;
                }

                for(int i= PreviewCards.Count - 1;i>=0;i--)
                {
                    var card = PreviewCards[i];
                    card.MakeStateTransition(EnumPositionState.InHand);

                    HandCards.Remove(card);
                    PreviewCards.Remove(card);

                    HandCards.Add(card);
                    card.Root.SetParent(NormalRoot, true);

                    AdjustHandCards();
                }
                

            });
        }


        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvent()
        {
            BattleManager.Instance.EventOnAddCard += OnAddCard;
            BattleManager.Instance.EventOnRemoveCard += OnRemoveHandCard;
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        private void UnRegisterEvent()
        {
            BattleManager.Instance.EventOnAddCard -= OnAddCard;
            BattleManager.Instance.EventOnRemoveCard -= OnRemoveHandCard;
        }

        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dTime"></param>
        public override void Tick(float dTime)
        {
            TickHands(dTime);

            TickPreview(dTime);

            
            if(Input.GetKeyDown(KeyCode.A))
            {
                var info = new CardInstanceInfo();
                info.InstanceId = 1;
                info.Config = GameStatic.ConfigDataLoader.GetConfigDataCardBattleInfo(104);
                OnAddCard(info);
            }
        }

        /// <summary>
        /// 正在选择目标
        /// </summary>
        /// <returns></returns>
        public bool IsPreviewChooseTarget()
        {
            if(PreviewCards.Count == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 放到来牌的位置
        /// </summary>
        /// <param name="card"></param>
        public void PutToInitPos(UIComponentBattleCard card)
        {
            card.PosDirty = true;
            card.TargetPositionInHand = 1;
        }


        #region 移动卡片

        /// <summary>
        /// 将卡片移到preview区
        /// </summary>
        /// <param name="card"></param>
        public void MoveCard2Preview(UIComponentBattleCard card)
        {
            HandCards.Remove(card); 
            PreviewCards.Remove(card);

            PreviewCards.Add(card);
            card.Root.SetParent(UseCardPreviewRoot, true);
        }

        #endregion


        /// <summary>
        /// 使用卡片
        /// </summary>
        public void UseCard()
        {
            if(m_isInReactMode)
            {
                // 上层调用 check react

                // add node

                //process
                ResolveManager.AddChain();
            }
            else
            {

            }
        }

        public void EndReact()
        {
            ResolveManager.endReact(); //check是否在等待且是在react 如果是则放弃
        }


        #region 显示通知事件

        /// <summary>
        /// 加入手卡
        /// </summary>
        /// <param name="instanceInfo"></param>
        /// <returns></returns>
        public void OnAddCard(CardInstanceInfo instanceInfo)
        {
            //GameStatic.ResourceManager.LoadAssetSync<Ga>
            GameObject cardGo = GameObject.Instantiate(m_cardPrefab, Root);
            if (cardGo == null)
            {
                return;
            }
            var card = cardGo.GetComponent<UIComponentBattleCard>();
            card.name = card.name.Replace("(Clone)","");
            card.Init(instanceInfo, this);
            card.BindFields();
            HandCards.Add(card);

            // 类型1 从左侧进牌
            if (card.GetCardType() == 1)
            {
                card.Root.position = BornPosLeft.position;
            }
            // 类型2 从右侧进牌
            else
            {
                card.Root.position = BornPosRight.position;
            }
            card.PositionState = EnumPositionState.CardDeck;

            card.MakeStateTransition(EnumPositionState.InHand);

            // 整理手牌 类型1在左 类型2 在右
            ReArrangeHandCards();

            // 调整位置
            AdjustHandCards();
        }


        /// <summary>
        /// 移除手卡中 表现
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
            AdjustHandCards();
        }

        public void EventOnWaitPlayerReact()
        {
            // 进入反应状态
            m_isInReactMode = true;

            // 更新ui
            // handle 使用卡片api
            // 此时check 反应

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
        /// 调整手牌位置
        /// </summary>
        private void AdjustHandCards()
        {
            // 计算当前手牌数量下的排布情况
            float interval = Settings.DefaultInterval;
            if (HandCards.Count > 6)
            {
                interval = 1.0f / HandCards.Count;
            }

            // 起始位置
            float startPosition = 0.5f - ((HandCards.Count - 1) * 0.5f * interval);

            // 计算在手卡中的排布位置
            for (int i = 0; i < HandCards.Count; i++)
            {
                float targetPosition = startPosition + i * interval;
                HandCards[i].TargetPositionInHand = targetPosition;
                HandCards[i].PosDirty = true;
            }
            RefreshHandCardsOrder();
        }


        /// <summary>
        /// 刷新前后遮挡顺序
        /// </summary>
        public void RefreshHandCardsOrder()
        {
            // 更新手牌顺序
            for (int i = 0; i < HandCards.Count; i++)
            {
                var card = HandCards[i];
                if(card.IsHightLight)
                {
                    card.transform.SetParent(HightLightRoot);
                }
                else
                {
                    card.transform.SetParent(NormalRoot);
                }
                card.transform.SetSiblingIndex(i);
            }
        }

        #region 内部方法

        /// <summary>
        /// tick手牌
        /// </summary>
        /// <param name="dTime"></param>
        protected void TickHands(float dTime)
        {
            for (int i = 0; i < HandCards.Count; i++)
            {
                var card = HandCards[i];
                card.Tick(dTime);
            }

            CheckHandCardHighlight();
        }

        /// <summary>
        /// tick预览区
        /// </summary>
        /// <param name="dTime"></param>
        protected void TickPreview(float dTime)
        {
            for (int i = 0; i < PreviewCards.Count; i++)
            {
                var card = PreviewCards[i];
                card.Tick(dTime);
            }

            // 更新
            if (PreviewCards.Count > 0)
            {
                BtnPreviewCancel.gameObject.SetActive(true);
                BtnPreviewConfirm.gameObject.SetActive(true);
            }
            else
            {
                BtnPreviewCancel.gameObject.SetActive(false);
                BtnPreviewConfirm.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 检查高光
        /// </summary>
        protected void CheckHandCardHighlight()
        {
            bool allHandReach = true;
            for (int i = 0; i < HandCards.Count; i++)
            {
                // 只有当手牌稳定时才可以
                var diff = HandCards[i].TargetPositionInHand - HandCards[i].NowPositionInHand;
                if (Mathf.Abs(diff) > 1e-2)
                {
                    allHandReach = false;
                }
                HandCards[i].IsHightLight = false;
            }
            // 所有牌到达指定位置时才可高亮
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

        #region 内部工具


        /// <summary>
        /// 通过手牌中的角度计算卡牌角度
        /// </summary>
        /// <param name="position">0-1之间的归一化位置</param>
        /// <returns></returns>
        public Vector3 LocalEularGetByPositionInHand(float position)
        {
            float degree = (Settings.LayoutDegree) * position - Settings.LayoutDegree * 0.5f;

            return new Vector3(0,0,-degree);
        }

        /// <summary>
        /// 通过手牌中的角度计算卡牌所处位置 
        /// </summary>
        /// <param name="position">0-1之间的归一化位置</param>
        /// <returns></returns>
        public Vector2 LocalPositionGetByDrgreeInHand(float position)
        {
            float degree = (Settings.LayoutDegree) * position - Settings.LayoutDegree * 0.5f;

            return new Vector2(Mathf.Sin(degree * Mathf.Deg2Rad) * m_arcRadius, Mathf.Cos(degree * Mathf.Deg2Rad) * m_arcRadius - m_arcRadius);
        }

        /// <summary>
        /// 获取鼠标悬浮的卡牌
        /// </summary>
        /// <returns></returns>
        protected UIComponentBattleCard GetMouseOverBattleCard()
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
                    if(obj.gameObject.name == "CardRoot")
                    {
                        return obj.gameObject.GetComponentInParent<UIComponentBattleCard>();
                    }
                }
            }
            return null;
        }
        
        private List<RaycastResult> m_cacheRaycastList = new List<RaycastResult>();

        /// <summary>
        /// 手牌排序 1类型在左 2类型在右
        /// </summary>
        /// <returns></returns>
        protected void ReArrangeHandCards()
        {
            var newList = new List<UIComponentBattleCard>();
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
        /// 预览中卡组
        /// </summary>
        public List<UIComponentBattleCard> PreviewCards = new List<UIComponentBattleCard>();

        /// <summary>
        /// 手牌组
        /// </summary>
        public List<UIComponentBattleCard> HandCards = new List<UIComponentBattleCard>();

        /// <summary>
        /// 是否在反应模式
        /// </summary>
        public bool m_isInReactMode;

        #region 缓存

        /// <summary>
        /// 缓存
        /// </summary>
        private Queue<GameObject> m_cachedBattleCard = new Queue<GameObject>();

        #endregion

        #region 绑定区域

        [AutoBind("./")]
        public RectTransform Root;

        [AutoBind("./Cards")]
        public RectTransform NormalRoot;

        [AutoBind("./CardsHightLight")]
        public RectTransform HightLightRoot;

        [AutoBind("./Pool")]
        public RectTransform Pool;

        [AutoBind("./UseCardPreview/Holder")]
        public RectTransform UseCardPreviewRoot;

        [AutoBind("./UseCardPreview/BtnPreviewCancel")]
        public Button BtnPreviewCancel;

        [AutoBind("./UseCardPreview/BtnPreviewConfirm")]
        public Button BtnPreviewConfirm;

        [AutoBind("./BornPosLeft")]
        public RectTransform BornPosLeft;

        [AutoBind("./BornPosRight")]
        public RectTransform BornPosRight;

        #endregion
    }
}

