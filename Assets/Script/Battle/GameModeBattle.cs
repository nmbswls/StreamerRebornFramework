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

    public interface IBattleProcessHandler
    {
        void PushProcess(BattleProcess process);
    }

    public class BattleProcess
    {
        public bool IsFinish { get; set; }
        public virtual void Tick()
        {

        }

        public virtual void OnFinish()
        {

        }
    }

    public class GameModeBattle : GameModeBase, IBattleProcessHandler
    {
        public const string MainSceneName = "Assets/Scenes/Battle.unity";

        //逻辑层
        public BattleManager MainBattle;
        //private readonly BattleEventListener m_mainBattleListener = new BattleEventListener(); //监听事件

        /// <summary>
        /// 传入参数 加载
        /// </summary>
        public void TryLoadBattleScene(Action onEnd)
        {
            TryStartLoad(MainSceneName, onEnd);
        }

        protected override void Initialize(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            base.Initialize(pipeCtx);
        }

        /// <summary>
        /// 收集需要加载的动态资源的路径
        /// </summary>
        /// <returns></returns>
        protected override List<string> CollectAllDynamicResForLoad(GameModeLoadPipeLineCtxBase pipeCtx)
        {
            return null;
        }

        
        public override void Tick(float dTime)
        {
            base.Tick(dTime);

            while(m_runningProcess.Count > 0)
            {
                var iter = m_runningProcess[0];
                iter.Tick();
                if(iter.IsFinish)
                {
                    iter.OnFinish();
                    m_runningProcess.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }


        /// <summary>
        /// 添加观众事件
        /// </summary>
        public void OnEventAddAudience(BattleAudience newAudience)
        {
            // Find Empty Slot
            var hud = UIControllerBattleHud.GetCurrentHud();
            hud.AddAudience();
        }

        /// <summary>
        /// 播放效果
        /// </summary>
        private List<object> m_effectList = new List<object>();

        public override string MainSceneResPath { get { return "Assets/Scenes/Battle.unity"; } }

        public void PushProcess(BattleProcess process)
        {
            m_runningProcess.Add(process);
        }

        private List<BattleProcess> m_runningProcess = new List<BattleProcess>();
    }

    
}

