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

            BattleAudienceManager = new BattleAudienceManager();

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

            BattleAudienceManager?.Tick(dTime);
        }


        public override string MainSceneResPath { get { return "Assets/Scenes/Battle.unity"; } }


        /// <summary>
        /// ���ڹ�����
        /// </summary>
        public BattleAudienceManager BattleAudienceManager;
    }
}

