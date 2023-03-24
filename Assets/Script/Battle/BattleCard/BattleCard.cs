using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using StreamerReborn.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    public class BattleCard : UIComponentBase
    {
        /// <summary>
        /// Owner
        /// </summary>
        protected BattleCardContainer m_container;

        public CardInstanceInfo InstanceInfo;

        /// <summary>
        /// 对外 类型
        /// </summary>
        /// <returns></returns>
        public int GetCardType()
        {
            if(InstanceInfo == null || InstanceInfo.Config == null)
            {
                return 0;
            }
            return InstanceInfo.Config.CardType;
        }

        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            InitCardAppearence();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void InitCardAppearence()
        {
            //view.NamePicture.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardName/" + ca.CatdImageName);
            //view.BackNamePicture.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardName/" + ca.CatdImageName);

            if (InstanceInfo.Config.CardType == 0)
            {
                //view.PictureCover.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardCover/Geng");
                //view.BackPictureCover.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardCover/Geng");
                //view.Background.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardBackground/Geng");
                //view.BackBackground.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardBackground/Geng");
                //view.TypePicture.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardType/Geng");
                //view.BackTypePicture.sprite = GameMain.GetInstance().GetModule<ResLoader>().LoadResource<Sprite>("CardType/Geng");
                //Color nowColor = Color.white;
                //ColorUtility.TryParseHtmlString(CostColor[2], out nowColor);  //color follow the type
                //view.Cost.color = nowColor;
                //view.BackCost.color = nowColor;
            }
        }

        public void Tick(float dTime)
        {
            switch(m_state)
            {
                case EnumState.Normal:
                    {

                    }
                    break;
                case EnumState.Destroying:
                    {
                        m_timer += dTime;
                        if(m_timer > 0.5f)
                        {
                            m_timer = 0;
                            Recycle();
                        }
                    }
                    break;
            }


            CardRoot.anchoredPosition = m_shakingVector;
        }


        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvent()
        {

        }


        /// <summary>
        /// 使用Card
        /// </summary>
        private void UseCard()
        {
            if (!BattleManager.Instance.CanUseCard(InstanceInfo))
            {
                return;
            }
            // 取消高亮
            SetHighLight(false);
        }

        #region 位置相关

        /// <summary>
        /// 在手牌中的位置
        /// 根据container的排列方式不同，代表不同含义
        /// </summary>
        public float NowPositionInHand;

        /// <summary>
        /// 在手牌中的目标位置
        /// 根据container的排列方式不同，代表不同含义
        /// </summary>
        public float TargetPositionInHand;


        #endregion
        
        /// <summary>
        /// 播放
        /// </summary>
        public void Disappaer()
        {
            m_animator.SetTrigger("Disappear");
            m_state = EnumState.Destroying;
            CardClickArea.blocksRaycasts = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Recycle()
        {
            m_container.RecycleCard(gameObject);
        }

        
        /// <summary>
        /// 设置高亮
        /// </summary>
        public void SetHighLight(bool isHighLight)
        {
            IsHightLight = isHighLight;
            m_container.RefreshHandCardsOrder();
        }

        /// <summary>
        /// pos 脏
        /// </summary>
        public bool PosDirty = false;


        /// <summary>
        /// 是否高亮
        /// </summary>
        public bool IsHightLight = false;


        #region 切换状态

        enum EnumState
        {
            Normal,
            Destroying,
            BackingFromOut,
        }

        /// <summary>
        /// 状态
        /// </summary>
        private EnumState m_state = EnumState.Normal;

        

        /// <summary>
        /// shaking
        /// </summary>
        private Vector2 m_shakingVector = Vector2.zero;

        /// <summary>
        /// timer
        /// </summary>
        private float m_timer;


        #endregion


        #region 绑定区域

        [AutoBind("./")]
        public RectTransform Root;

        [AutoBind("./CardRoot")]
        public RectTransform CardRoot;

        [AutoBind("./CardRoot")]
        public CanvasGroup CardClickArea;

        [AutoBind("./CardRoot/TextDescription")]
        public Text TextDescription;

        [AutoBind("./CardRoot/TextCost")]
        public Text TextCost;

        [AutoBind("./CardRoot/TextName")]
        public Text TextName;

        [AutoBind("./CardRoot/ImageContent")]
        public Image ImageContent;

        [AutoBind("./")]
        public Animator m_animator;

        #endregion
    }

}
