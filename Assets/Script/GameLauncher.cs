using My.Framework.Runtime;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{
    public class GameLauncher : MonoBehaviour
    {
        // Use this for initialization
        IEnumerator Start()
        {
            // 编辑器模式加上这一句
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
            GameStatic.ConfigDataLoader.TryLoadInitConfig(
                (lret) => { 
                    ret = lret;
                }
                );
            while (ret == null) { yield return null; }

            yield return null;

            GameStatic.UIManager.RegisterUIController("EntryStartup", typeof(UIControllerEntryStartup).ToString(), 0);

            GameStatic.UIManager.RegisterUIController("Loading", typeof(UIControllerLoading).ToString(), 0);
            GameStatic.UIManager.RegisterUIController("MessageBox", typeof(UIControllerMessageBoxSimple).ToString(), 0);

            // 启动进门ui
            var entryUI = GameStatic.UIManager.StartUIController(new UIIntent("EntryStartup")) as UIControllerEntryStartup;
            if (entryUI == null)
            {
                Debug.LogError("SampleGameEntryUITask start fail");
            }
            entryUI.EventOnEnter += GameStatic.MyGameManager.EnterBattle;
        }

        void Update()
        {
            if (m_gameManager != null)
            {
                m_gameManager.Tick();
            }
        }

        /// <summary>
        /// 管理器
        /// </summary>
        protected MyGameManager m_gameManager;
    }
}

