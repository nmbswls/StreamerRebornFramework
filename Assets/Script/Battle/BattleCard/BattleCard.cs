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
        /// ���� ����
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
        /// ��ɰ󶨻ص�
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
        /// ע���¼�
        /// </summary>
        private void RegisterEvent()
        {

        }


        /// <summary>
        /// ʹ��Card
        /// </summary>
        private void UseCard()
        {
            if (!BattleManager.Instance.CanUseCard(InstanceInfo))
            {
                return;
            }
            // ȡ������
            SetHighLight(false);
        }

        #region λ�����

        /// <summary>
        /// �������е�λ��
        /// ����container�����з�ʽ��ͬ������ͬ����
        /// </summary>
        public float NowPositionInHand;

        /// <summary>
        /// �������е�Ŀ��λ��
        /// ����container�����з�ʽ��ͬ������ͬ����
        /// </summary>
        public float TargetPositionInHand;


        #endregion
        
        /// <summary>
        /// ����
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
        /// ���ø���
        /// </summary>
        public void SetHighLight(bool isHighLight)
        {
            IsHightLight = isHighLight;
            m_container.RefreshHandCardsOrder();
        }

        /// <summary>
        /// pos ��
        /// </summary>
        public bool PosDirty = false;


        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool IsHightLight = false;


        #region �л�״̬

        enum EnumState
        {
            Normal,
            Destroying,
            BackingFromOut,
        }

        /// <summary>
        /// ״̬
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


        #region ������

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
