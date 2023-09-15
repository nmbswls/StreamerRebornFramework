using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 可继承的通用战斗表现
    /// </summary>
    public class UIControllerBattlePerform : UIControllerBase
    {
        public UIControllerBattlePerform(string name) : base(name)
        {
        }

        protected override void RefreshView()
        {
        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            if (m_uiCompArray.Length > 0 && m_compPerformMain == null)
            {
                m_compPerformMain = m_uiCompArray[0] as UIComponentBattlePerformUI;
            }
            
            if (m_uiCompArray.Length > 1 && m_compPerformAnnounce == null)
            {
                m_compPerformAnnounce = m_uiCompArray[1] as UIComponentBattlePerformAnnounce;
            }
        }

        #region 对外方法

        /// <summary>
        /// 跳字
        /// </summary>
        /// <param name="originPos"></param>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        public void ShowHintText(Vector3 originPos, string text, Action callback = null)
        {
            Debug.Log($"ShowHintText of {text}");
        }

        /// <summary> 显示Buff宣告 技能宣告 </summary>
        public void ShowAnnounce(Vector3 originPos, string text, Action callback = null)
        {
            //var actor = BattleSceneActorManager.Instance.GetSceneActor(actorId);
            if (m_compPerformAnnounce != null)
                m_compPerformAnnounce.ShowAnnounce(originPos, text, callback);
        }

        #endregion

        #region 唯一类型ui

        public static void StopUITask()
        {
            UIManager.Instance.StopUIController("BattlePerform");
            Instance = null;
        }
        public static void StartUITask(Action<bool> onEnd)
        {
            Instance = UIManager.Instance.StartUIController(new UIIntent("BattlePerform"), onPipelineEnd: onEnd) as UIControllerBattlePerform;
        }
        public static UIControllerBattlePerform Instance { get; private set; }

        #endregion

        #region 绑定

        private UIComponentBattlePerformUI m_compPerformMain;
        private UIComponentBattlePerformAnnounce m_compPerformAnnounce;



        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "BattlePerformUI",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/BattlePerformUI.prefab",
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
                m_attachLayerName = "BattlePerformUI",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBattlePerformUI).ToString(),
                m_compName = "BattlePerformUI"
            },
            new UIComponentDesc
            {
                m_attachLayerName = "BattlePerformUI",
                m_attachPath = "./AnnouncementRoot",
                m_compTypeName = typeof(UIComponentBattlePerformAnnounce).ToString(),
                m_compName = "BattlePerformAnnounce"
            },
        };

        #endregion
    }
}
