using My.Framework.Runtime;
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
        public bool IsStart;
        public bool IsFinish;

        public virtual void OnStart()
        {

        }
        public virtual void Tick(float dTime)
        {

        }

        public virtual void OnFinish()
        {

        }
    }

    public class BattleProcessPlayCardEffect : BattleProcess
    {
        public BattleProcessPlayCardEffect(uint cardInstanceId, float duration):base()
        {
            m_cardInstanceId = cardInstanceId;
            m_duration = duration;
        }

        uint m_cardInstanceId = 0;
        float m_duration = 0;
        float m_timer = 0;

        UIComponentBattleCard m_battleCard;

        public override void OnStart()
        {
            //
            var hud = UIControllerBattleHud.GetCurrentHud();
            var cardComp = hud.m_compHud.m_cardContainer.GetCardUIComponent(m_cardInstanceId);

            if(cardComp == null)
            {
                return;
            }
            m_battleCard = cardComp;
            m_battleCard.ShowDissolve();
        }

        public override void Tick(float dTime)
        {
            m_timer += dTime;
            if (m_timer > m_duration)
            {
                IsFinish = true;
            }
        }

        public virtual void OnFinish()
        {
            m_battleCard.Disappaer();
        }
    }

    //public class GameModeBattle : GameModeBase, IBattleProcessHandler
    //{
    //    public const string MainSceneName = "Assets/Scenes/Battle.unity";

    //    //逻辑层
    //    public BattleManager MainBattle;
    //    //private readonly BattleEventListener m_mainBattleListener = new BattleEventListener(); //监听事件

    //    /// <summary>
    //    /// 传入参数 加载
    //    /// </summary>
    //    public void TryLoadBattleScene(Action onEnd)
    //    {
    //        TryStartLoad(MainSceneName, onEnd);
    //    }

    //    protected override void Initialize(GameWorldSceneLoadingCtxBase pipeCtx)
    //    {
    //        base.Initialize(pipeCtx);
    //    }

    //    /// <summary>
    //    /// 收集需要加载的动态资源的路径
    //    /// </summary>
    //    /// <returns></returns>
    //    protected override List<string> CollectAllDynamicResForLoad(GameWorldSceneLoadingCtxBase pipeCtx)
    //    {
    //        return null;
    //    }

        
    //    public override void Tick(float dTime)
    //    {
    //        base.Tick(dTime);

    //        TickEffectProcess(dTime);
    //    }

    //    protected void TickEffectProcess(float dTime)
    //    {
    //        if (m_runningProcess.Count > 0)
    //        {
    //            var iter = m_runningProcess[0];
    //            if(!iter.IsStart)
    //            {
    //                iter.OnStart();
    //                iter.IsStart = true;
    //            }
    //            iter.Tick(dTime);
    //            if (iter.IsFinish)
    //            {
    //                iter.OnFinish();
    //                m_runningProcess.RemoveAt(0);
    //            }
    //        }
    //    }


    //    /// <summary>
    //    /// 添加观众事件
    //    /// </summary>
    //    public void OnEventAddAudience(BattleAudience newAudience)
    //    {
    //        // Find Empty Slot
    //        var hud = UIControllerBattleHud.GetCurrentHud();
    //        hud.AddAudience();
    //    }

    //    /// <summary>
    //    /// 播放效果
    //    /// </summary>
    //    private List<object> m_effectList = new List<object>();

    //    public override string MainSceneResPath { get { return "Assets/Scenes/Battle.unity"; } }

    //    public void PushProcess(BattleProcess process)
    //    {
    //        m_runningProcess.Add(process);
    //    }

    //    private List<BattleProcess> m_runningProcess = new List<BattleProcess>();
    //}

    
}

