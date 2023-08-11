using System;
using System.Collections;
using System.Collections.Generic;
using My.Framework.Runtime.Saving;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerEntryStartup : UIControllerBase
    {
        public UIControllerEntryStartup(string name) : base(name)
        {
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compEntryStartup = m_uiCompArray[0] as UIComponentEntryStartup;

            m_compEntryStartup.EventOnNewGame += OnEventNewGame;
            m_compEntryStartup.EventOnLoadSaving += OnEventLoadGame;
        }

        protected override void OnTick(float dt)
        {
        }

        #region ����¼�

        /// <summary>
        /// ����Ϸ
        /// </summary>
        private void OnEventNewGame()
        {
            //����
            Stop();

            // ��������
            GameManager.Instance.GamePlayer.OnLoadGame(null);

            // ֪ͨ״̬�ı�
            GameManager.Instance.GameWorld.EnterHall();
        }

        /// <summary>
        /// ����Ϸ
        /// </summary>
        private void OnEventLoadGame(int savingIdx)
        {
            

            if (!GameManager.Instance.SavingManager.LoadSavingData(savingIdx, out SavingData savingData))
            {
                Debug.LogError("Load Game Fail.");
                return;
            }

            //����
            Stop();

            // �߼����ʼ��
            GameManager.Instance.GamePlayer.OnLoadGame(savingData);

            // ֪ͨ״̬�ı�
            GameManager.Instance.GameWorld.EnterHall();
        }

        protected override void RefreshView()
        {
            //m_compEntryStartup.Show(m_param);
        }


        #endregion

        /// <summary>
        /// ��component
        /// </summary>
        protected UIComponentEntryStartup m_compEntryStartup;

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "MainEntry",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/EntryStartup.prefab",
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
                m_attachLayerName = "MainEntry",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentEntryStartup).ToString(),
                m_compName = "MainEntry"
            },
        };
    }
}

