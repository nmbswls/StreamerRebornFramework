using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Battle.Logic;
using My.Framework.Runtime.Resource;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// actor管理器
    /// </summary>
    public abstract class BattleSceneActorManagerBase : IBattleActorEventListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneManager"></param>
        protected BattleSceneActorManagerBase(BattleSceneManagerBase sceneManager)
        {
            m_sceneManager = sceneManager;

            SceneActorRoot = sceneManager.m_sceneActorRootTransform;
            SceneActorPoolParent = new GameObject("SceneActorPool");
            SceneActorPoolParent.transform.SetParent(SceneActorRoot);
            //RegisterEvent();
        }

        /// <summary> 初始化Actor对象 </summary>
        public void Initialize(BattleLogic battleLogic)
        {
            var allActors = battleLogic.CompActorContainer.m_battleActorList;
            foreach (var actorLogic in allActors)
            {
                AddSceneActor(actorLogic);
            }
        }


        /// <summary> 创建一个新角色入场景 </summary>
        public BattleSceneActorBase AddSceneActor(BattleActor logicActor)
        {
            if (m_sceneActorDict.ContainsKey(logicActor.InstId))
                return m_sceneActorDict[logicActor.InstId];

            int viewId = 100;

            // 尝试获取指定prefab
            GameObject actor = SpawnSceneActor(viewId);
            Transform actorTransform;
            if (actor != null)
            {
                // 先去池子里找
                actorTransform = actor.transform;
            }
            else
            {
                //Int32 charId = battleActorInfo.CharacterId;
                //Int32 skinId = battleActorInfo.CharacterShowId;

                //CharacterQualityType characterQuality = CharacterQualityType.Default;
                //if (battleActorInfo.GetActorType() == SceneActorType.Character)
                //{
                //    characterQuality = SceneActorUtility.GetCharacterQualityType(BattleManager.Instance.BattleQualitySetting, LoadCharacterSourceType.Battle);
                //}
                //actorTransform = SceneActorUtility.CreateBattleSceneActorTransform(charId, skinId, SceneActorParent, characterQuality);
            }

            var actorCtrl = ActorCtrlCreate(logicActor, SceneActorRoot);

            
            //初始化角色信息 加载基础武器特效 更新角色可变材质容器
            actorCtrl.Initialize(logicActor);

            return actorCtrl;
        }

        #region 内部方法

        private GameObject SpawnSceneActor(int characterShowId)
        {
            //CharacterPoolItem pool;
            //GetCharacterPoolItem(characterShowId, out pool, true);
            //GameObject prefab = pool.GetPrefab();
            //if (prefab != null)
            //{
            //    prefab.SetParent(SceneActorParent.gameObject);
            //    prefab.SetActiveSafely(true);
            //}

            //if (!BattleStaticVar.isSkippingBattlePerformance)
            //{
            //    TryDeleteCharacterPool();
            //}
            //return prefab;
            return null;
        }


        /// <summary>
        /// 增加BattleActor
        /// </summary>
        protected abstract BattleSceneActorBase ActorCtrlCreate(BattleActor battleActor, bool isBattleEnter = false);

        #endregion

        #region 逻辑层事件监听

        public void OnAddBuff(BattleActor actor, BattleActorBuff buff)
        {
        }

        public void OnCauseDamage(uint targetId, uint sourceId, long dmg, long realDmg)
        {
            UIControllerBattlePerform.Instance.ShowHintText(Vector3.zero, $"OnCauseDamage {sourceId}");
        }

        #endregion

        #region 内部变量

        /// <summary>
        /// 场景管理器
        /// </summary>
        protected BattleSceneManagerBase m_sceneManager;

        /// <summary>
        /// 
        /// </summary>
        protected Transform SceneActorRoot { get; set; }

       
        private GameObject SceneActorPoolParent { get; set; }


        /// <summary> 战场中单位</summary>
        protected readonly Dictionary<uint, BattleSceneActorBase> m_sceneActorDict = new Dictionary<uint, BattleSceneActorBase>();

        /// <summary>
        /// 顺序排列数组
        /// </summary>
        protected List<BattleSceneActorBase> m_sceneActorArray = new List<BattleSceneActorBase>();

        #endregion

        

    }
}
