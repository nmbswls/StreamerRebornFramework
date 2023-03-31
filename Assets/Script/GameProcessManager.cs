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

        /// <summary>
        /// ��ʱ����
        /// </summary>
        /// <returns></returns>
        public BattleProcess GetBattleProcess()
        {
            foreach (var process in m_processList)
            {
                if(process is BattleProcess)
                {
                    return process;
                }
            }
            return null;
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
        /// ����ui
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
        /// ����process
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadProcess()
        {
            float timeOut = Time.realtimeSinceStartup;

            var battleProcess = new BattleProcess() ;
            m_processList.Add(battleProcess);
            bool isFinish = false;
            battleProcess.TryLoadBattleScene(() => {
                Debug.Log("TryLoadBattleScene Finish");
                isFinish = true;
            });

            // ��ʱ
            while (!isFinish)
            {
                yield return null;
            }
            yield return null;

            var hudCtrl = UIControllerBattleHud.Create();
            while (!hudCtrl.IsLayerInStack())
                yield return 0;


            // �������

        }

        private IEnumerator LoadAfterUI()
        {
            UIControllerLoading.StopLoadingUI();

            yield return 1;
            EventOnLoadingEnd?.Invoke();
            EventOnLoadingEnd = null;
        }


        List<BattleProcess> m_processList = new List<BattleProcess>();

        #region ��ʱ


        #endregion
    }
}
