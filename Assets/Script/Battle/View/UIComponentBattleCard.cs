using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using My.Framework.Runtime.UIExtention;
using StreamerReborn.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    public class UIComponentBattleCard : UIComponentBase
    {
        /// <summary>
        /// Owner
        /// </summary>
        protected BattleCardContainer m_container;

        public CardInstanceInfo InstanceInfo;


        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="instanceInfo"></param>
        /// <param name="owner"></param>
        public void Init(CardInstanceInfo instanceInfo, BattleCardContainer owner)
        {
            this.InstanceInfo = instanceInfo;
            this.m_container = owner;
        }


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
        /// ��Ҫռ������λ��
        /// </summary>
        /// <returns></returns>
        public bool NeedBlockInHand()
        {
            if(m_state == EnumPositionState.InHand)
            {
                return true;
            }
            if(m_currentTransitionInfo != null)
            {
                if(m_currentTransitionInfo.m_toState == EnumPositionState.InHand)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ��ɰ󶨻ص�
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            InitCardAppearence();

            CardClickArea.onClick.AddListener(OnClick);


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
            // �����л�״̬
            TickStateTransition();
            // ����λ��
            TickPosition(dTime);

            CardRoot.anchoredPosition = m_shakingVector;

            if(Input.GetKeyDown(KeyCode.S))
            {
                wholeDissolveController.StartDissolve();
            }
        }

        /// <summary>
        /// Tick λ��
        /// </summary>
        /// <param name="dTime"></param>
        protected void TickPosition(float dTime)
        {
            
            switch (m_state)
            {
                case EnumPositionState.InHand:
                    {
                        if (m_currentTransitionInfo != null) break;

                        NowPositionInHand = Mathf.Lerp(NowPositionInHand, TargetPositionInHand, 0.05f);


                        if (NowPositionInHand < 0) NowPositionInHand = 0;
                        if (NowPositionInHand > 1) NowPositionInHand = 1;
                        var localEular = m_container.LocalEularGetByPositionInHand(NowPositionInHand);
                        Vector2 targetPosHorizon = m_container.LocalPositionGetByDrgreeInHand(NowPositionInHand);

                        if (IsHightLight)
                        {
                            HighlightRate = Mathf.Lerp(HighlightRate, 1, 0.05f);
                            if (HighlightRate > 1)
                            {
                                HighlightRate = 1;
                            }
                        }
                        else
                        {
                            HighlightRate = Mathf.Lerp(HighlightRate, 0, 0.1f);
                        }

                        targetPosHorizon += Vector2.up * 100f * HighlightRate;
                        Root.anchoredPosition = targetPosHorizon;
                        CardRoot.localScale = Vector3.one * ((1 - 0.7f) * HighlightRate + 0.7f);

                        if (IsHightLight)
                        {
                            Root.localEulerAngles = Vector3.zero;
                        }
                        else
                        {
                            Root.localEulerAngles = localEular;
                        }
                    }
                    break;
            }
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
            if (!BattleManager.Instance.CardManager.CanUseCard(InstanceInfo))
            {
                return;
            }
            // ȡ������
            SetHighLight(false);
        }

        /// <summary>
        /// ���
        /// </summary>
        protected void OnClick()
        {
            wholeDissolveController.StartDissolve();

            if (m_state != EnumPositionState.InHand)
            {
                return;
            }
            m_container.MoveCard2Preview(this);
            IsHightLight = false;
            MakeStateTransition(EnumPositionState.InPreview);
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


        /// <summary>
        /// ��ǰ������ 
        /// </summary>
        public float HighlightRate = 0;


        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool IsHightLight = false;


        #endregion

        /// <summary>
        /// ����
        /// </summary>
        public void Disappaer()
        {
            //m_animator.SetTrigger("Disappear");
            //m_state = EnumPositionState.Destroying;
            //CardClickArea.blocksRaycasts = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Recycle()
        {
            m_container.RecycleCard(gameObject);

            wholeDissolveController.DissolveEnd();
            m_currentTransitionInfo = null;
            m_state = EnumPositionState.CardDeck;
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




        #region �л�״̬

        public enum EnumPositionState
        {
            InHand,
            CardDeck, // ��ʼ״̬
            InPreview,
        }

        public EnumPositionState PositionState { 
            get { return m_state; }
            set { m_state = value; }
        }
        /// <summary>
        /// λ��״̬
        /// </summary>
        private EnumPositionState m_state = EnumPositionState.InHand;

        /// <summary>
        /// ������ת
        /// </summary>
        public void MakeStateTransition(EnumPositionState toState)
        {
            // ��ȫ���ܾ�
            if(m_currentTransitionInfo != null)
            {
                return;
            }
            var info = new TransitionInfo() { m_fromState = m_state, m_toState = toState };
            m_currentTransitionInfo = info;
        }

        class TransitionInfo
        {
            public EnumPositionState m_fromState;
            public EnumPositionState m_toState;
        }

        private TransitionInfo m_currentTransitionInfo;

        protected void TickStateTransition()
        {
            if (m_currentTransitionInfo == null) return;

            switch(m_currentTransitionInfo.m_toState)
            {
                case EnumPositionState.InHand:
                    {
                        Root.localEulerAngles = m_container.LocalEularGetByPositionInHand(TargetPositionInHand);

                        CardRoot.localScale = Vector3.one * 0.7f;

                        Vector2 targetPos = m_container.LocalPositionGetByDrgreeInHand(TargetPositionInHand);
                        Root.anchoredPosition = Vector2.Lerp(Root.anchoredPosition, targetPos, 0.05f);
                        if (Mathf.Abs((targetPos - Root.anchoredPosition).magnitude) < 1e-2)
                        {
                            EndStateTransition();
                            Root.anchoredPosition = targetPos;
                        }
                    }
                    break;
                case EnumPositionState.InPreview:
                    {
                        Root.localEulerAngles = Vector3.zero;

                        Vector2 targetPos = Vector2.zero;
                        Root.anchoredPosition = Vector2.Lerp(Root.anchoredPosition, targetPos, 0.05f);
                        if (Mathf.Abs((targetPos - Root.anchoredPosition).magnitude) < 1e-2)
                        {
                            Root.anchoredPosition = targetPos;
                            EndStateTransition();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// ������ת
        /// </summary>
        protected void EndStateTransition()
        {
            if(m_currentTransitionInfo == null)
            {
                Debug.LogError("EndStateTransition error m_currentTransitionInfo not exists.");
                return;
            }
            NowPositionInHand = TargetPositionInHand;
            m_state = m_currentTransitionInfo.m_toState;

            m_currentTransitionInfo = null;
        }

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
        public Button CardClickArea;

        [AutoBind("./CardRoot/TextDescription")]
        public TextMeshProUGUI TextDescription;

        [AutoBind("./CardRoot/TextCost")]
        public Text TextCost;

        [AutoBind("./CardRoot/TextName")]
        public Text TextName;

        [AutoBind("./CardRoot/ImageContent")]
        public Image ImageContent;

        [AutoBind("./CardRoot/ImageCover")]
        public Image ImageCover;

        [AutoBind("./CardRoot/ImageBG")]
        public Image ImageBG;

        //[AutoBind("./")]
        //public Animator m_animator;

        [AutoBind("./CardRoot")]
        public WholeDissolveController wholeDissolveController;

        #endregion
    }

}
