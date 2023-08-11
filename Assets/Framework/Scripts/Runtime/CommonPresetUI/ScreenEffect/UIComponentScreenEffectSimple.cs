using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{

    public class UIComponentScreenEffectSimple : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
        }

        public void ShowBlackMask(float preTime)
        {
            
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);


            if(m_isOut || m_isIn)
            {
                m_timer += dt;

                if(m_isIn)
                {
                    var progress = Mathf.Clamp(m_timer / m_duration, 0, 1);
                    m_rootMask.alpha = Mathf.Lerp(0, 1, progress);
                }
                else
                {
                    var progress = Mathf.Clamp(m_timer / m_duration, 0, 1);
                    m_rootMask.alpha = Mathf.Lerp(1, 0, progress);
                }

                if (m_timer > m_duration)
                {
                    m_actionOnEnd?.Invoke();
                    m_isOut = false;
                    m_isIn = false;
                }
            }
        }

        #region ×´Ì¬

        private float m_timer;
        private bool m_isIn;

        private bool m_isOut;
        private float m_duration;
        #endregion

        public void FadeEnterBlack(float duration, Action onEnd)
        {
            m_isIn = true;
            m_isOut = false;
            if(duration < 0)
            {
                m_duration = 0.3f;
            }
            else
            {
                m_duration = duration;
            }
            m_rootMask.alpha = 0;
            m_timer = 0;
            m_actionOnEnd = onEnd;
        }

        public void FadeQuitBlack(float duration, Action onEnd)
        {
            m_isIn = false;
            m_isOut = true;
            if (duration < 0)
            {
                m_duration = 0.3f;
            }
            else
            {
                m_duration = duration;
            }
            m_rootMask.alpha = 1;
            m_timer = 0;
            m_actionOnEnd = onEnd;
        }

        public void Close(Action actionOnClose)
        {
            //tween end
            actionOnClose?.Invoke();
        }


        private void HandleEnd()
        {
            if (m_actionOnEnd != null)
            {
                m_actionOnEnd();
            }
        }


        public Action m_actionOnEnd;

        #region °ó¶¨ÇøÓò

        [AutoBind(".")]
        public CanvasGroup m_rootMask;
        
        #endregion
    }
}

