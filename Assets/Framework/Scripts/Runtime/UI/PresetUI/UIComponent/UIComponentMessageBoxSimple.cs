using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    public class MessageBoxParam
    {
        public string Content;
        public Action actionOnBtn1Clicked;
        public Action actionOnBtn2Clicked;
        public Action actionOnBtn3Clicked;
    }
    public class UIComponentMessageBoxSimple : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            m_cancelBtn.onClick.AddListener(OnCancelBtnClicked);
            m_yesBtn.onClick.AddListener(OnYesBtnClicked);
            m_noBtn.onClick.AddListener(OnNoBtnClicked);
        }

        public void Show(MessageBoxParam param)
        {
            m_contentText.text = param.Content;
            m_actionOnYesBtnClicked = param.actionOnBtn1Clicked;
            m_actionOnNoBtnClicked = param.actionOnBtn2Clicked;
            m_actionOnCancelBtnClicked = param.actionOnBtn3Clicked;

            m_canCancel = true;
            m_canInteract = true;
        }

        public void Close(Action actionOnClose)
        {
            //tween end
            actionOnClose?.Invoke();
        }

        private void OnCancelBtnClicked()
        {
            if (!m_canCancel)
            {
                return;
            }
            if (!m_canInteract)
            {
                return;
            }
            m_canInteract = false;

            m_actionOnCancelBtnClicked?.Invoke();
            m_actionOnCancelBtnClicked = null;
            HandleEnd();
        }

        private void OnYesBtnClicked()
        {
            if (!m_canInteract)
            {
                return;
            }
            m_canInteract = false;
            m_actionOnYesBtnClicked?.Invoke();
            m_actionOnYesBtnClicked = null;
            HandleEnd();
        }

        private void OnNoBtnClicked()
        {
            if (!m_canInteract)
            {
                return;
            }
            m_canInteract = false;
            m_actionOnNoBtnClicked?.Invoke();
            m_actionOnNoBtnClicked = null;
            
            HandleEnd();
        }

        private void HandleEnd()
        {
            m_actionOnYesBtnClicked = null;
            m_actionOnNoBtnClicked = null;
            m_actionOnCancelBtnClicked = null;
            if (actionOnEnd != null)
            {
                actionOnEnd();
            }
        }

        public Action actionOnEnd;

        private bool m_canInteract;
        private bool m_canCancel;

        private Action m_actionOnYesBtnClicked;
        private Action m_actionOnNoBtnClicked;
        private Action m_actionOnCancelBtnClicked;

        #region °ó¶¨ÇøÓò

        [AutoBind("./BlackBGButton")] 
        public Button m_cancelBtn;
        [AutoBind("./Panel/ConfirmButton")]
        public Button m_yesBtn;
        [AutoBind("./Panel/CancelButton")]
        public Button m_noBtn;
        [AutoBind("./Panel/Content/Text_Value")]
        public Text m_contentText;
        #endregion
    }
}

