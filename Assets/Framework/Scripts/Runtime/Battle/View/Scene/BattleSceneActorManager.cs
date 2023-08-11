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
    public class BattleSceneActorManager : IBattleActorEventListener
    {
        public BattleSceneActorManager(GameObject root)
        {
            SceneActorParent = root.transform;
            SceneActorPoolParent = new GameObject("SceneActorPool");
            SceneActorPoolParent.transform.SetParent(SceneActorParent);
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
            if (m_sceneActorDict.ContainsKey(logicActor.ActorId))
                return m_sceneActorDict[logicActor.ActorId];

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

            var actorCtrl = ActorCtrlCreate(logicActor, SceneActorParent);

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
        protected virtual BattleSceneActorBase ActorCtrlCreate(BattleActor battleActor, bool isBattleEnter = false)
        {

            BattleSceneActorBase actorCtrl = null;
            switch (battleActor.CompBasic.ActorType)
            {
                case 1:
                    // 5.创建Ctrl
                    actorCtrl = CreatePlayer(battleActor, null);
                    break;
                case 2:
                    actorCtrl = CreatePlayer(battleActor, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (actorCtrl == null)
            {
                return null;
            }

            // 4.添加进数据结构
            m_sceneActorDict[battleActor.ActorId] = actorCtrl;
            m_sceneActorArray.Add(actorCtrl);
            return actorCtrl;
        }

        /// <summary>
        /// 创建Actor
        /// </summary>
        public static BattleSceneActorBase CreatePlayer(BattleActor battleActor, Transform parentTrans)
        {
            // 1. 入参合法性判断
            if (battleActor == null)
            {
                Debug.LogError("[Battle][Actor][Create] Create hero, Param battleActor = NULL.");
                return null;
            }

            // 1.获取路径
            string playerAssetPath = "test/battle/player.asset";
            GameObject actorAsset = SimpleResourceManager.Instance.LoadAssetSync<GameObject>(playerAssetPath);
            if (actorAsset == null)
            {
                Debug.LogError(
                    $"[Battle][Actor][Create] HeroPrefabInstantiate::Actor Asset is NULL! playerAssetPath:{playerAssetPath}");
                return null;
            }

            // 3.实例化asset并设置parent Trans
            // 初始坐标的Y值设置成-5 是防止人物进场显影的时候穿帮
            var actorGo = GameObject.Instantiate(actorAsset, new Vector3(0, -5, 0), Quaternion.identity) as GameObject;

            // 4.指定asset的父对象
            if (parentTrans != null)
            {
                actorGo.transform.SetParent(parentTrans, false);
            }

            if (actorGo == null)
            {
                return null;
            }

            // 4. 创建并获取ctrl
            //PrefabControllerCreater.CreateAllControllers(actorGo);
            var ctrl = actorGo.GetComponent<BattleSceneActorBase>();
            if (ctrl == null)
            {
                Debug.LogError(
                    $"[Battle][Actor][Create] Failed! <BattleSceneActorHeroController> is NULL.ActorInsId:{battleActor.ActorId}");
                return null;
            }

            // 5. 调用ctrl的初始化方法
            ctrl.Initialize(battleActor);

            Debug.Log(
                $"[Battle][Actor][Create] ActorInsId:{battleActor.ActorId}, Success.");

            return ctrl;
        }

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

        /// <summary> 父节点 </summary>
        private Transform SceneActorParent { get; set; }
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
