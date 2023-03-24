using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class UIComponentLoading : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
        }

        public void HideLoadingUI(Action onHideEnd)
        {
            // show tween 
            onHideEnd?.Invoke();
        }

        /// <summary>
        /// œ‘ æcontent
        /// </summary>
        /// <param name="content"></param>
        public void ShowLoadingUI(string content, Action<bool> onShowEnd)
        {
            m_loadingText.text = content;

            // show tween 
            onShowEnd?.Invoke(true);
        }

        #region ∞Û∂®«¯”Ú

        [AutoBind("./LoadingText")]
        public Text m_loadingText;

        #endregion
    }
}

