using My.Framework.Runtime;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{
    /// <summary>
    /// 
    /// </summary>
    public class MyUIRegister : UIRegisterBase
    {
        /// <summary>
        /// ִ��ע��
        /// </summary>
        public override void DoRegister()
        {
            base.DoRegister();

            UIManager.Instance.RegisterUIController("WorldOverlay", typeof(UIControllerWorldOverlay).ToString(), 4);
            UIManager.Instance.RegisterUIController("SceneHud", typeof(UIControllerSceneHudSimple).ToString(), 2);
        }
    }

    public class GameLauncher : MonoBehaviour
    {
        // Use this for initialization
        IEnumerator Start()
        {
            // �༭��ģʽ������һ��
#if UNITY_EDITOR

#endif
            m_gameManager = GameManager.CreateAndInitGameManager<MyGameManager>();
            if (m_gameManager == null)
            {
                Debug.LogError("CreateAndInitGameManager start fail");
                yield break;
            }

            yield return GameStatic.ResourceManager.InitializeAssetBundle("Default", null);


            bool? ret = null;
            m_gameManager.StartLoadConfigData((lret) => {
                ret = lret;
            }, out var configDataInitLoadCount);
            
            while (ret == null) { yield return null; }

            yield return null;

            // ע��ui��Դ
            GameStatic.UIManager.InitUITaskRegister<MyUIRegister>();

            // ��������ui
            var entryUI = GameStatic.UIManager.StartUIController(new UIIntent("EntryStartup")) as UIControllerEntryStartup;
            if (entryUI == null)
            {
                Debug.LogError("SampleGameEntryUITask start fail");
            }
        }

        void Update()
        {
            if (m_gameManager != null)
            {
                m_gameManager.Tick();
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        protected MyGameManager m_gameManager;
    }
}

