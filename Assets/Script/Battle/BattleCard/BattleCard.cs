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
    public class BattleCard : UIComponentBase
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
                case EnumPositionState.InHand:
                    {

                    }
                    break;
                case EnumPositionState.Backing:
                    {
                        Root.localEulerAngles = m_container.LocalEularGetByPositionInHand(TargetPositionInHand);

                        Vector2 targetPos = m_container.LocalPositionGetByDrgreeInHand(TargetPositionInHand);
                        Root.anchoredPosition = Vector2.Lerp(Root.anchoredPosition, targetPos, 0.05f);
                        if(Mathf.Abs((targetPos - Root.anchoredPosition).magnitude) < 1e-2)
                        {
                            Root.anchoredPosition = targetPos;
                            NowPositionInHand = TargetPositionInHand;
                            m_state = EnumPositionState.InHand;
                        }
                    }
                    break;
            }


            CardRoot.anchoredPosition = m_shakingVector;

            if(Input.GetKeyDown(KeyCode.S))
            {
                wholeDissolveController.StartDissolve();
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
            if (!BattleManager.Instance.CanUseCard(InstanceInfo))
            {
                return;
            }
            // ȡ������
            SetHighLight(false);
        }

        #region λ�����

        /// <summary>
        /// ���������ʼ��
        /// </summary>
        /// <param name="positionWorld"></param>
        public void InitFromOutside(Vector3 positionWorld)
        {
            m_state = EnumPositionState.Backing;

            Root.position = positionWorld;
            Root.localPosition = new Vector3(Root.localPosition.x, Root.localPosition.y, 0);
        }


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
            //m_animator.SetTrigger("Disappear");
            //m_state = EnumPositionState.Destroying;
            CardClickArea.blocksRaycasts = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Recycle()
        {
            m_container.RecycleCard(gameObject);

            wholeDissolveController.DissolveEnd();
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

        public enum EnumPositionState
        {
            InHand,
            Poping, // ���ڸ���
            InPreview,
            Backing,
        }

        public EnumPositionState PositionState { get { return m_state; } }
        /// <summary>
        /// λ��״̬
        /// </summary>
        private EnumPositionState m_state = EnumPositionState.InHand;

        

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
