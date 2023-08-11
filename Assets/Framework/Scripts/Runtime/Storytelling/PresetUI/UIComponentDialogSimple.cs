using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.Storytelling;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime
{

    
    /// <summary>
    /// �����Ի����
    /// </summary>
    public class UIComponentDialogSimple : UIComponentBase
    {
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            m_clickArea.onClick.AddListener(OnTextAreaClick);

            // options ��ʼ��
        }

        public override void Tick(float dt)
        {
            TickPlayText(dt);
        }


        /// <summary>
        /// ��������
        /// </summary>
        protected void TickPlayText(float dt)
        {
            if(!m_isPlaying)
            {
                return;
            }

            if(m_currDialogContent == null || string.IsNullOrEmpty(m_currDialogContent.Contents))
            {
                return;
            }

            // ��������״̬ ����ص�
            if(m_currTextIndex >= m_currDialogContent.Contents.Length)
            {
                m_isPlaying = false;
                m_isWaitingClick = true;
                ShowTextEndHint();
                return;
            }

            // �ۼƲ��Ž���
            m_currTextIndex = m_currTextIndex + dt * 50; // 
            // û�б��ϴ���ʾ����
            if(m_shownMaxTextIdx == (int)m_currTextIndex)
            {
                return;
            }
            int newShownIdx = (int)m_currTextIndex;

            StringBuilder sb = new StringBuilder();
            for(int i = m_shownMaxTextIdx + 1; i <= newShownIdx; i++)
            {
                if(i >= m_currDialogContent.Contents.Length)
                {
                    break;
                }

                // ����������ַ��� ���⴦��
                if(m_currDialogContent.m_specialIndex != null)
                {
                    int findIdx = m_currDialogContent.m_specialIndex.FindIndex(a => a == i);
                    if (findIdx != -1)
                    {
                        var speItem = m_currDialogContent.m_specialItems[findIdx];
                        var hexColor = ColorNameToHex(speItem.m_color);
                        sb.Append("<color=");
                        sb.Append(hexColor);
                        sb.Append(">");
                        sb.Append(speItem.m_content);
                        sb.Append("</color");
                        continue;
                    }
                }
                // ����ֱ�����
                sb.Append(m_currDialogContent.Contents[i]);
            }

            if(sb.Length != 0)
            {
                m_contentStr = m_contentStr + sb.ToString();
                // add the last character to show marker
                m_contentText.text = m_contentStr + " ";
            }
            m_shownMaxTextIdx = newShownIdx;
        }

        /// <summary>
        /// �����������¼�
        /// </summary>
        protected void OnTextAreaClick()
        {
            // ͣ�º�ſɵ��
            if(!m_isWaitingClick)
            {
                return;
            }

            m_cbTextPlayEnd?.Invoke();
            m_cbTextPlayEnd = null;

            m_isWaitingClick = false;
        }


        /// <summary>
        /// ��ʼ����һ���Ի�
        /// ��ͷ��ʼ����
        /// </summary>
        public void StartPlayText(bool isAppend = false)
        {
            m_currTextIndex = 0;
            m_shownMaxTextIdx = -1;
            m_isPlaying = true;
            m_isWaitingClick = false;

            if(!isAppend)
            {
                m_contentStr = string.Empty;
                m_contentText.text = string.Empty;
            } 

            ShowAll();
        }


        /// <summary>
        /// ���ò������� ����ͷ����
        /// </summary>
        public void SetDialogContent(string rawText)
        {
            m_currDialogContent = ParseDialogContent(rawText);
        }

        /// <summary>
        /// ��ǰ״̬
        /// </summary>
        /// <param name="speaker"></param>
        public void SetDialogSpeaker(string speaker)
        {
            m_speakerText.text = speaker;
        }

        /// <summary>
        /// ��ǰ״̬
        /// </summary>
        /// <param name="speaker"></param>
        public void SetDialogCallback(Action callback)
        {
            m_cbTextPlayEnd = callback;
        }

        /// <summary>
        /// ��ʾ���ֽ������hint
        /// </summary>
        public void ShowTextEndHint()
        {
            m_endMarker.gameObject.SetActive(false);
            if(m_isWaitingClick)
            {
                var textInfo = m_contentText.GetTextInfo(m_contentText.text);
                if (textInfo.characterInfo.Length == 0)
                {
                    return;
                }
                var lastCharacterInfo = textInfo.characterInfo[textInfo.characterInfo.Length - 1];
                Vector3 characterPosition = m_contentText.transform.TransformPoint(lastCharacterInfo.bottomLeft);
                m_endMarker.gameObject.SetActive(true);
                m_endMarker.transform.position = characterPosition;
            }
        }

        /// <summary>
        ///  ��ʾ
        /// </summary>
        /// <returns></returns>
        public bool ShowAll()
        {
            m_contentText.gameObject.SetActive(true);
            m_speakerText.gameObject.SetActive(true);
            m_clickArea.gameObject.SetActive(true);

            return true;
        }

        /// <summary>
        /// ���ؽ���
        /// </summary>
        /// <returns></returns>
        public bool HideAll()
        {
            m_contentText.gameObject.SetActive(false);
            m_speakerText.gameObject.SetActive(false);
            m_clickArea.gameObject.SetActive(false);
            m_endMarker.gameObject.SetActive(false);
            m_optionPanel.gameObject.SetActive(false);

            return true;
        }


        public bool IsTextEnd()
        {
            return true;
        }

        /// <summary>
        /// ��ʾѡ��
        /// </summary>
        public void ShowOptions(List<OptionCommand> options)
        {
            m_optionPanel.SetActive(true);
            while(m_optionList.Count < options.Count)
            {
                var newGo = GameObject.Instantiate(m_optionTemplate, m_optionContainer);
                var comp = newGo.GetComponent<UIComponentDialogOption>();
                comp.OptionIdx = m_optionList.Count;
                m_optionList.Add(comp);
            }
            for(int i=0;i<options.Count;i++)
            {
                m_optionList[i].gameObject.SetActive(true);
                m_optionList[i].SetOptionContent(options[i].Option);
            }
            for (int i = options.Count; i < m_optionList.Count; i++)
            {
                m_optionList[i].gameObject.SetActive(false);
            }
            m_isWaitingOption = true;
        }

        #region ����״̬

        /// <summary>
        /// ��������content
        /// </summary>
        public struct DialogContentItem
        {
            public string m_content;
            public string m_color;
        }

        /// <summary>
        /// ��������Ĳ�������
        /// </summary>
        public class DialogContent
        {
            public string Contents;
            public List<int> m_specialIndex;
            public List<DialogContentItem> m_specialItems;
        }

        private static readonly Regex ColorTagRegex = new Regex("<color=#([0-9a-fA-F]{6})>(.*?)</color>");
        private static readonly char SpecialSplitter = '#';

        /// <summary>
        /// ������������
        /// </summary>
        /// <returns></returns>
        protected DialogContent ParseDialogContent(string rawText)
        {
            var ret = new DialogContent();

            MatchCollection colorMatches = ColorTagRegex.Matches(rawText);
            if(colorMatches.Count == 0)
            {
                ret.Contents = rawText;
                return ret;
            }

            List<DialogContentItem> items = new List<DialogContentItem>();
            foreach (Match match in colorMatches)
            {
                items.Add(new DialogContentItem() { m_content  = match.Groups[2].Value, m_color = match.Groups[1].Value });
            }

            string tmpString = rawText;

            // Iterate through all color tag matches in the original text
            for (int i = colorMatches.Count - 1; i >= 0; i--)
            {
                Match match = colorMatches[i];
                string colorCode = match.Groups[1].Value;
                string innerText = match.Groups[2].Value;

                // Record the start and end indices of the color tag
                int startIndex = match.Index;
                int endIndex = match.Index + match.Length;

                // Replace the color tag with the inner text
                tmpString = tmpString.Substring(0, startIndex) + SpecialSplitter + tmpString.Substring(endIndex);
            }

            List<int> itemIndex = new List<int>();
            
            for (int i=0;i<tmpString.Length;i++)
            {
                if(tmpString[i] == SpecialSplitter)
                {
                    itemIndex.Add(i);
                }
            }

            ret.Contents = tmpString;
            ret.m_specialIndex = itemIndex;
            ret.m_specialItems = items;

            return ret;
        }

        /// <summary>
        /// ��ǰdialog����
        /// </summary>
        protected DialogContent m_currDialogContent;

        /// <summary>
        /// ��ǰ���ֲ����±�
        /// �Ե�ǰ�ı�����
        /// </summary>
        protected float m_currTextIndex;

        /// <summary>
        /// �Ѿ���ʾ������±�
        /// �Ե�ǰ�ı�����
        /// -1��ʾû����ʾ��
        /// </summary>
        protected int m_shownMaxTextIdx;

        /// <summary>
        /// ���ֲ��Ž���
        /// </summary>
        protected Action m_cbTextPlayEnd;

        /// <summary>
        /// ���������string
        /// </summary>
        protected string m_contentStr;

        /// <summary>
        /// ���λ �Ƿ����ڲ�������
        /// </summary>
        protected bool m_isPlaying;

        /// <summary>
        /// ���λ ������� ���ڵȴ�ȷ��
        /// ���� �Ƿ�ϲ�Ϊ״̬����
        /// </summary>
        protected bool m_isWaitingClick;

        /// <summary>
        /// �Ƿ��ڵȴ�ѡ��
        /// </summary>
        protected bool m_isWaitingOption;

        #endregion

        #region �¼�



        #endregion

        #region �ڲ�����

        /// <summary>
        /// ������ɫ���� ��ȡʵ��ʮ��������ɫ
        /// </summary>
        /// <param name="colorType"></param>
        /// <returns></returns>
        protected string ColorNameToHex(string colorType)
        {
            switch(colorType)
            {
                case "red":
                    return "red";
            }
            return "#000000";
        }

        #endregion

        #region ѡ�

        /// <summary>
        /// ѡ���б�
        /// </summary>
        protected List<UIComponentDialogOption> m_optionList = new List<UIComponentDialogOption>();

        #endregion


        #region ������

        [AutoBind("./Speaker/SpeakerText")]
        public Text m_speakerText;

        [AutoBind("./ContentText")]
        public TextMeshProUGUI m_contentText;

        [AutoBind("./ClickArea")]
        public Button m_clickArea;

        [AutoBind("./EndMarker")]
        public Image m_endMarker;

        [AutoBind("./OptionPanel")]
        public GameObject m_optionPanel;

        [AutoBind("./OptionPanel/Options")]
        public Transform m_optionContainer;

        [AutoBind("./OptionPanel/OptionTemplate")]
        public UIComponentDialogOption m_optionTemplate;

        #endregion
    }
}


