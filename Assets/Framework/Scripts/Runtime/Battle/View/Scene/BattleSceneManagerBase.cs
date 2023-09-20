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
    public abstract class BattleSceneManagerBase
    {
        protected Scene? BindScene;

        protected bool m_isStarted = false;
        protected event Action EventOnLoadSceneEnd;

        public virtual void Initialize(Scene scene)
        {
            this.BindScene = scene;
            var rootObjs = BindScene.Value.GetRootGameObjects();

            var sceneRoot = rootObjs[0].transform;
            BindSceneRoot(sceneRoot);
            
            m_sceneActorManager = CreateBattleSceneActorManager();


            LoadBattleSceneObjects();

            m_isStarted = true;
            //m_sceneColorHandler = new SceneColorHandler();
            //m_sceneEclipseHandler = new SceneEclipseHandler();
            //m_screenShiningHandler = new ScreenShiningHandler();
        }

        /// <summary>
        /// 加载战斗场景所需物体
        /// </summary>
        public void LoadBattleSceneObjects()
        {
            if (m_sceneObjContainer == null)
            {
                GameObject root = CreateGameObject("SceneObjContainer");
                m_sceneObjContainer = root.transform;
            }
            if (m_sceneObjContainer != null)
            {
                
            }
        }

        public void RestartBattle()
        {
            CleanBattle();
        }

        public void CleanBattle()
        {
            if (m_sceneObjContainer != null)
            {
                //for (int i = 0; i < m_sceneObjContainer.sceneObjList.Count; ++i)
                //{
                //    m_sceneObjContainer.sceneObjList[i].ResetState();
                //}
            }

        }

        #region 内部方法

        /// <summary>
        /// 创建scene 
        /// </summary>
        /// <returns></returns>
        protected abstract BattleSceneActorManagerBase CreateBattleSceneActorManager();

        /// <summary>
        /// 绑定场景节点
        /// </summary>
        protected virtual void BindSceneRoot(Transform sceneRoot)
        {
            m_sceneActorRootTransform = sceneRoot.Find("ActorRoot");
            m_namedPointRoot = sceneRoot.Find("NamedPointRoot");
        }


        public GameObject GetSceneObj(string name)
        {
            if (m_sceneObjContainer != null)
            {
                for (int i = 0; i < m_sceneObjContainer.childCount; i++)
                {
                    var view = m_sceneObjContainer.GetChild(i);
                    if (view != null && view.name == name)
                    {
                        return view.gameObject;
                    }
                }
            }
            return null;
        }


        protected GameObject CreateGameObject(String name, bool addMapDic = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            if (!addMapDic) return go;

            m_mapObjectList.Add(go);
            return go;
        }

        #endregion

        /// <summary>
        /// 静态物体缓存列表
        /// </summary>
        protected readonly List<GameObject> m_mapObjectList = new List<GameObject>();

        /// <summary>
        /// 当前战斗场景
        /// </summary>
        public UnityEngine.SceneManagement.Scene Scene;

        /// <summary>
        /// 场景actor管理器
        /// </summary>
        protected BattleSceneActorManagerBase m_sceneActorManager;
        public BattleSceneActorManagerBase ActorManager
        {
            get { return m_sceneActorManager; }
        }

        // 组织根节点
        public Transform m_sceneObjContainer;
        public Transform m_sceneActorRootTransform;

        /// <summary>
        /// 命名点根节点
        /// </summary>
        public Transform m_namedPointRoot;

    }
}
