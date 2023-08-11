using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine.UI;
using StreamerReborn.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using My.Framework.Runtime;
using My.Framework.Runtime.Director;
using StreamerReborn;

namespace My.Runtime
{
    /// <summary>
    /// Actor头顶冒泡
    /// </summary>
    public class UIComponentActorBubble : UIComponentBase
    {
        public ActorControllerBase bindTarget;

        private Camera m_parentUICamera;
        private RectTransform m_canvasRectTransform;

        public void Initialize(ActorControllerBase bingTarget)
        {
            this.bindTarget = bingTarget;

            if (m_parentUICamera == null)
            {
                m_parentUICamera = UIManager.Instance.GetUICamera(gameObject);
            }
            if(m_canvasRectTransform == null)
            {
                m_canvasRectTransform = UIManager.Instance.GetCanvas(gameObject).GetComponent<RectTransform>();
            }
            SetBubbleBGEnable(false);
            SetBubbleContentEnable(false);
        }


        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            UpdateRectTransform();

            if(m_auto)
            {
                UpdateShowStatus(dt);
            }
        }

        /// <summary>
        /// 显示气泡
        /// </summary>
        public void ShowBubble(string bubbleStyle, float duration, Action onEnd)
        {
            // 激活显示
            m_auto = true;
            m_animator.enabled = true;

            // TODO 根据bubble类型拼接不同override animation controller
            switch (bubbleStyle)
            {
                case "1":
                    {
                        m_bubbleContent.sprite = sprites[0];
                        break;
                    }
                case "2":
                    {
                        m_bubbleContent.sprite = sprites[1];
                        break;
                    }
                default:
                {
                        m_bubbleContent.sprite = null;
                    break;
                }

            }

            m_currBubbleStyle = bubbleStyle;
            m_currDuration = duration;
            m_timer = 0;

            m_animator.SetTrigger("ShowUp");
            // 显示气泡
            SetBubbleContentEnable(true);
            SetBubbleBGEnable(true);

            m_eventOnDisappear = onEnd;
        }

        /// <summary>
        /// 隐藏bubble
        /// </summary>
        /// <param name="OnAnimEnd"></param>
        public void HideBubble()
        {
            m_auto = false;
            m_flagDisappear = false;

            m_animator.enabled = false;
            m_timer = 0;

            m_eventOnDisappear?.Invoke();
            m_eventOnDisappear = null;

            // 回收
            {
                var overlayUI = UIControllerWorldOverlay.GetCurrent();
                overlayUI?.ReleaseActorBubble(this);
            }
        }


        /// <summary>
        /// 是否结束
        /// </summary>
        /// <returns></returns>
        public bool IsShowEnd()
        {
            return m_timer >= m_currDuration;
        }


        #region 动画事件

        /// <summary>
        /// 动画事件回调
        /// </summary>
        /// <param name="eventName"></param>
        protected void OnAnimationEvent(string eventName)
        {
            switch(eventName)
            {
                case "DisappearEnd":
                    {
                        // 仅在自动模式下进行动画回调
                        if(!m_auto)
                        {
                            return;
                        }
                        HideBubble();
                    }
                    break;
            }
        }

        /// <summary>
        /// 结束回调事件
        /// </summary>
        protected Action m_eventOnDisappear;

        #endregion


        #region 对外接口

        public void SetBubbleBGEnable(bool enabled)
        {
            m_bubbleBG.gameObject.SetActive(enabled);
        }

        public void SetBubbleContentEnable(bool enabled)
        {
            m_bubbleContent.gameObject.SetActive(enabled);
        }

        #endregion


        /// <summary>
        /// 获取当前激活的相机
        /// </summary>
        /// <returns></returns>
        protected CameraWrapper GetCurrActivateCamera()
        {
            if(DirectorManager.Instance.m_currCutscene != null)
            {
                return DirectorManager.Instance.m_currCutscene.m_cameraWrapper;
            }
            var currState = GameManager.Instance.GameWorld.GetCurrState();
            if (currState.StateType != GameWorldStateTypeDefineBase.SimpleHall)
            {
                return null;
            }
            return ((GameWorldStateSimpleHall)currState).CameraManager.SceneCamera;
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        private void UpdateRectTransform()
        {
            if (bindTarget == null || !bindTarget.enabled)
            {
                return;
            }

            var sceneCamera = GetCurrActivateCamera();
            if (sceneCamera == null || m_parentUICamera == null)
            {
                return;
            }
                
            var viewport = sceneCamera.MainCamera.WorldToViewportPoint(bindTarget.m_statUIRoot.position);

            viewport.z = m_canvasRectTransform.anchoredPosition3D.z;
            Vector3 worldPos = m_parentUICamera.ViewportToWorldPoint(viewport);

            if (Vector3.SqrMagnitude(transform.position - worldPos) > 1e-5f)
            {
                transform.position = worldPos;
                transform.localScale = new Vector3(1, 1, 1);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
            }
        }

        /// <summary>
        /// 更新显示状态
        /// </summary>
        private void UpdateShowStatus(float dt)
        {

            m_timer += dt;

            if(m_timer >= m_currDuration && !m_flagDisappear)
            {
                m_flagDisappear = true;
                m_animator.SetTrigger("Disappear");
                SetBubbleContentEnable(false);
            }
        }

        protected bool m_auto = false; //是否自动进行 timeline形式需要支持拖拽 由上层进行驱动
        protected float m_timer;
        protected bool m_flagDisappear;

        public string m_currBubbleStyle;
        public float m_currDuration;

        public static float m_hidingTime = 0.3f;

        public Sprite[] sprites;

        #region 绑定组件

        [AutoBind("./BubbleContent")]
        public Image m_bubbleContent;

        [AutoBind("./BubbleBG")]
        public Image m_bubbleBG;

        [AutoBind(".")]
        public Animator m_animator;

        #endregion
    }
}
