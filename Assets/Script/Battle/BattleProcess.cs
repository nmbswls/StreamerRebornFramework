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

        }

        /// <summary>
        /// �ռ���Ҫ���صĶ�̬��Դ��·��
        /// </summary>
        /// <returns></returns>
        protected override List<string> CollectAllDynamicResForLoad(GameProcessLoadPipeLineCtxBase pipeCtx)
        {
            return null;
        }

        

        public override string MainSceneResPath { get { return "Assets/Scenes/Battle.unity"; } }
    }
}

