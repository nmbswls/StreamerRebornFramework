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
    public class BattleSceneActorManagerTorture : BattleSceneActorManagerBase
    {
        public BattleSceneActorManagerTorture(BattleSceneManagerTorture sceneManager) : base(sceneManager)
        {
        }

        #region 继承

        /// <summary>
        /// 增加BattleActor
        /// </summary>
        protected override BattleSceneActorBase ActorCtrlCreate(BattleActor battleActor, bool isBattleEnter = false)
        {
            BattleSceneActorBase actorCtrl = null;
            switch (battleActor.ActorType)
            {
                case BattleActorTypeStartup.Player:
                    // 5.创建Ctrl
                    actorCtrl = CreatePlayer(battleActor, null);
                    break;
                case BattleActorTypeStartup.Enemy:
                    actorCtrl = CreateEnemy(battleActor, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (actorCtrl == null)
            {
                return null;
            }

            // 4.添加进数据结构
            m_sceneActorDict[battleActor.InstId] = actorCtrl;
            m_sceneActorArray.Add(actorCtrl);
            return actorCtrl;
        }

        #endregion

        #region 对外方法


        /// <summary>
        /// 创建Actor
        /// </summary>
        public BattleSceneActorBase CreatePlayer(BattleActor battleActor, Transform parentTrans)
        {
            // 1. 入参合法性判断
            if (battleActor == null)
            {
                Debug.LogError("CreatePlayer, Param battleActor = NULL.");
                return null;
            }

            // 1.获取路径
            string playerAssetPath = "Assets/RuntimeAssets/Battle/Prefabs/Player.prefab";
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
                    $"[Battle][Actor][Create] Failed! <BattleSceneActorHeroController> is NULL.ActorInsId:{battleActor.InstId}");
                return null;
            }

            // 5. 调用ctrl的初始化方法
            ctrl.Initialize(battleActor);
            
            // 设置位置
            ctrl.transform.position = SceneManagerTorture.PlayerRoot.position;

            Debug.Log(
                $"[Battle][Actor][Create] ActorInsId:{battleActor.InstId}, Success.");

            return ctrl;
        }

        /// <summary>
        /// 创建Actor
        /// </summary>
        public BattleSceneActorBase CreateEnemy(BattleActor battleActor, Transform parentTrans)
        {
            // 1. 入参合法性判断
            if (battleActor == null)
            {
                Debug.LogError("[Battle][Actor][Create] Create hero, Param battleActor = NULL.");
                return null;
            }

            // 1.获取路径
            string assetPath = "Assets/RuntimeAssets/Battle/Prefabs/Enemy.prefab";
            GameObject actorAsset = SimpleResourceManager.Instance.LoadAssetSync<GameObject>(assetPath);
            if (actorAsset == null)
            {
                Debug.LogError(
                    $"[Battle][Actor][Create] CreateEnemy Asset is NULL! AssetPath:{assetPath}");
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
                    $"[Battle][Actor][Create] Failed! <BattleSceneActorHeroController> is NULL.ActorInsId:{battleActor.InstId}");
                return null;
            }

            // 5. 调用ctrl的初始化方法
            ctrl.Initialize(battleActor);
            // 设置位置
            ctrl.transform.position = SceneManagerTorture.EnemyRoot.position;

            Debug.Log(
                $"[Battle][Actor][Create] ActorInsId:{battleActor.InstId}, Success.");

            return ctrl;
        }


        #endregion

        protected BattleSceneManagerTorture SceneManagerTorture
        {
            get { return (BattleSceneManagerTorture) m_sceneManager; }
        }

    }

}
