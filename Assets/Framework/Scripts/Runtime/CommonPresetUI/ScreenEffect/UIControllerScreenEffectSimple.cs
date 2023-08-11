using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerScreenEffectSimple : UIControllerBase
    {
        public UIControllerScreenEffectSimple(string name) : base(name)
        {
        }

        /// <summary>
        /// 显示message Box
        /// </summary>
        /// <param name="content"></param>
        /// <param name="onConfirm"></param>
        /// <returns></returns>
        public static UIControllerScreenEffectSimple ShowScreenEffect(float enterTime)
        {
            var intent = new UIIntent("ScreenEffect");
            return UIManager.Instance.StartUIController(intent) as UIControllerScreenEffectSimple;
        }

        protected override void OnTick(float dt)
        {
            m_compScreenEffect?.Tick(dt);
        }

        /// <summary>
        /// 不支持多个intent重复进入
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="onEnd"></param>
        public void FadeEnterBlack(float duration, Action onEnd = null)
        {
            m_compScreenEffect.FadeEnterBlack(duration, onEnd);
        }

        /// <summary>
        /// 不支持多个intent重复进入
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="onEnd"></param>
        public void FadeQuitBlack(float duration, Action onEnd = null)
        {
            m_compScreenEffect.FadeQuitBlack(duration, onEnd);
        }
        

        protected override void RefreshView()
        {
            
        }

        protected override void ParseIntent()
        {
            base.ParseIntent();
        }



        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();

            m_compScreenEffect = m_uiCompArray[0] as UIComponentScreenEffectSimple;
            if (m_compScreenEffect != null)
            {
                m_compScreenEffect.m_actionOnEnd += OnScreenEffectEnd;
            }
        }

        private void OnScreenEffectEnd()
        {
            m_compScreenEffect.Close(Stop);
        }

        private UIComponentScreenEffectSimple m_compScreenEffect;


        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "ScreenEffect",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/ScreenEffect.prefab",
            },
        };
        protected override UIComponentDesc[] UIComponentDescArray
        {
            get { return m_uiComponentDescArray; }
        }
        private readonly UIComponentDesc[] m_uiComponentDescArray =
        {
            new UIComponentDesc
            {
                m_attachLayerName = "ScreenEffect",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentScreenEffectSimple).ToString(),
                m_compName = "ScreenEffect"
            },
        };
    }
}


