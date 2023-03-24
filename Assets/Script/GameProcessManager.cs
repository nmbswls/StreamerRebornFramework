using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class GameProcessManager
    {
        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dTime"></param>
        public void Tick(float dTime)
        {
            foreach(var process in m_processList)
            {
                process.Tick(dTime);
            }
        }


        private event Action EventOnLoadingEnd;

        public IEnumerator EnterBattleCoroutine(Action onEnd)
        {
            EventOnLoadingEnd = onEnd;
            yield return LoadUI();
            yield return LoadProcess();
            yield return LoadAfterUI();
        }

        /// <summary>
        /// 加载ui
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadUI()
        {
            var loadingCtrl = GameStatic.UIManager.FindUIControllerByName("Loading") as UIControllerLoading;
            if (loadingCtrl == null || loadingCtrl.State != UIControllerBase.UIState.Running)
            {
                loadingCtrl = UIControllerLoading.ShowLoadingUI(0, "loading tips");
                while (!loadingCtrl.IsOpen)
                {
                    yield return 0;
                }
            }
        }

        /// <summary>
        /// 加载process
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadProcess()
        {
            float timeOut = Time.realtimeSinceStartup;

            var battleProcess = new BattleProcess() ;
            m_processList.Add(battleProcess);
            bool isFinish = false;
            battleProcess.TryLoadBattleScene(() => {
                Debug.LogError("TryLoadBattleScene Finish");
                isFinish = true;
            });

            // 超时
            while (!isFinish)
            {
                yield return null;
            }

            //var hudCtrl = UIControllerHud.Create();
            //while (!hudCtrl.IsLayerInStack())
            //    yield return 0;


            // 加入管理
            
        }

        private IEnumerator LoadAfterUI()
        {
            UIControllerLoading.StopLoadingUI();

            yield return 1;
            EventOnLoadingEnd?.Invoke();
            EventOnLoadingEnd = null;
        }


        List<BattleProcess> m_processList = new List<BattleProcess>();

        #region 临时


        #endregion
    }
}
