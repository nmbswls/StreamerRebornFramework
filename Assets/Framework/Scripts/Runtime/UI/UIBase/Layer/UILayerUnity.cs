using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 包含一个unityscene的layer
    /// </summary>
    public class UILayerUnity : UILayerBase
    {
        /// <summary>
        /// 层类型
        /// </summary>
        public override UILayerType LayerType { get { return UILayerType.Scene; } }

        /// <summary>
        /// 设置scene
        /// </summary>
        /// <param name="scene"></param>
        public void SetScene(UnityEngine.SceneManagement.Scene scene)
        {
            Scene = scene;
            UnitySceneRootObjs.AddRange(scene.GetRootGameObjects());
        }

        /// <summary>
        /// 获取层对应的camera
        /// </summary>
        public override Camera LayerCamera
        {
            get
            {
                if (m_layerCamera == null)
                {
                    foreach (var go in UnitySceneRootObjs)
                    {
                        var cameras = go.GetComponentsInChildren<Camera>(true);
                        if (cameras.Length == 1)
                        {
                            m_layerCamera = cameras[0];
                        }
                        else
                        {
                            foreach (var cam in cameras)
                            {
                                if (cam.gameObject.name == "LayerCamera")
                                {
                                    m_layerCamera = cam;
                                    return m_layerCamera;
                                }
                            }
                        }
                    }
                }
                return m_layerCamera;
            }
        }
        /// <summary>
        /// 当前场景
        /// </summary>
        public UnityEngine.SceneManagement.Scene Scene;

        /// <summary>
        /// 场景根节点列表
        /// </summary>
        public List<UnityEngine.GameObject> UnitySceneRootObjs = new List<GameObject>();

        /// <summary>
        /// 相机
        /// </summary>
        protected Camera m_layerCamera;
    }
}

