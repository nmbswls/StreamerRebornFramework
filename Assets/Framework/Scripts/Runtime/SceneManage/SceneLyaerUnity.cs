using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace My.Framework.Runtime.Scene
{
    /// <summary>
    /// ����һ��unityscene��layer
    /// </summary>
    public class SceneLayerUnity : SceneLayer3D
    {
        /// <summary>
        /// ����scene
        /// </summary>
        /// <param name="scene"></param>
        public void SetScene(UnityEngine.SceneManagement.Scene scene)
        {
            Scene = scene;
            UnitySceneRootObjs.AddRange(scene.GetRootGameObjects());
        }

        /// <summary>
        /// ��ȡ���Ӧ��camera
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
        private Camera m_layerCamera;

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public UnityEngine.SceneManagement.Scene Scene;

        /// <summary>
        /// �������ڵ��б�
        /// </summary>
        public List<UnityEngine.GameObject> UnitySceneRootObjs = new List<GameObject>();
    }
}
