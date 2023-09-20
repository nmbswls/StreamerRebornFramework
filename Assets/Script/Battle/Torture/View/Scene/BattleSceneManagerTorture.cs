using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Runtime.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Battle
{
    public class BattleSceneManagerTorture : BattleSceneManagerBase
    {

        #region 内部方法

        /// <summary>
        /// 创建scene 
        /// </summary>
        /// <returns></returns>
        protected override BattleSceneActorManagerBase CreateBattleSceneActorManager()
        {
            var retManager = new BattleSceneActorManagerTorture(this);
            return retManager;
        }

        /// <summary>
        /// 绑定场景节点
        /// </summary>
        protected override void BindSceneRoot(Transform sceneRoot)
        {
            base.BindSceneRoot(sceneRoot);

            PlayerRoot = m_namedPointRoot.Find("PlayerMain");
            EnemyRoot = m_namedPointRoot.Find("EnemyMain");
        }


        #endregion

        #region 场景结构

        public Transform PlayerRoot;
        public Transform EnemyRoot;

        #endregion

    }
}
