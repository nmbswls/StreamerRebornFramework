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

        #region 点击事件

        /// <summary>
        /// 新游戏
        /// </summary>
        private void OnEventNewGame()
        {
            //清理
            Stop();

            // 加载数据
            GameManager.Instance.GamePlayer.OnLoadGame(null);

            // 通知状态改变
            GameManager.Instance.GameWorld.EnterHall();
        }

        /// <summary>
        /// 新游戏
        /// </summary>
        private void OnEventLoadGame(int savingIdx)
        {
            

            if (!GameManager.Instance.SavingManager.LoadSavingData(savingIdx, out SavingData savingData))
            {
                Debug.LogError("Load Game Fail.");
                return;
            }

            //清理
            Stop();

            // 逻辑层初始化
            GameManager.Instance.GamePlayer.OnLoadGame(savingData);

            // 通知状态改变
            GameManager.Instance.GameWorld.EnterHall();
        }

        protected override void RefreshView()
        {
            //m_compEntryStartup.Show(m_param);
        }


        #endregion

        /// <summary>
        /// 主component
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

