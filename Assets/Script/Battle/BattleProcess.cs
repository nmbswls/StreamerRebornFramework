using My.Framework.Runtime;
using My.Framework.Runtime.Scene;
using StreamerReborn;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamerReborn
{
    public class BattleProcess : GameProcessBase
    {
        public const string MainSceneName = "Assets/Scenes/Battle.unity";

        //�߼���
        public BattleManager MainBattle;
        //private readonly BattleEventListener m_mainBattleListener = new BattleEventListener(); //�����¼�

        /// <summary>
        /// ������� ����
        /// </summary>
        public void TryLoadBattleScene(Action onEnd)
        {
            TryStartLoad(MainSceneName, onEnd);
        }

        protected override void Initialize(GameProcessLoadPipeLineCtxBase pipeCtx)
        {
            base.Initialize(pipeCtx);
        }

        /// <summary>
        /// �ռ���Ҫ���صĶ�̬��Դ��·��
        /// </summary>
        /// <returns></returns>
        protected override List<string> CollectAllDynamicResForLoad(GameProcessLoadPipeLineCtxBase pipeCtx)
        {
            return null;
        }

        
        public override void Tick(float dTime)
        {
            base.Tick(dTime);
        }


        /// <summary>
        /// ��ӹ����¼�
        /// </summary>
        public void OnEventAddAudience(BattleAudience newAudience)
        {
            // Find Empty Slot
            var hud = UIControllerBattleHud.GetCurrentHud();
            hud.AddAudience();
        }

        /// <summary>
        /// ����Ч��
        /// </summary>
        private List<object> m_effectList = new List<object>();

        public override string MainSceneResPath { get { return "Assets/Scenes/Battle.unity"; } }
    }
}

