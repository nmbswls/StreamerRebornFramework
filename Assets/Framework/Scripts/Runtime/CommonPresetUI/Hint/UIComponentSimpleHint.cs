using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class UIComponentSimpleHint : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            transform.localPosition = Vector3.zero;
            m_timer = 0;
        }

        /// <summary>
        /// 由脚本驱动
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            m_timer += dt;
            transform.localPosition += dt * 10 * Vector3.up;
        }

        public void SetHintParam(string hintContent, float duration)
        {
            Duration = duration;
            m_hintText.text = hintContent;
        }

        public float Duration;
        protected float m_timer;

        public bool IsFinish()
        {
            return m_timer > Duration;
        }

        #region 绑定区域

        [AutoBind("./HintText")]
        public Text m_hintText;

        #endregion
    }
}

