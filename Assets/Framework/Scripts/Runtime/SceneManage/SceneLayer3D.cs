using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Scene
{
    public class SceneLayer3D : SceneLayerBase
    {
        /// <summary>
        /// 获取层对应的camera
        /// </summary>
        public override Camera LayerCamera
        {
            get
            {
                if (m_layerCamera == null)
                {
                    var cameras = GetComponentsInChildren<Camera>(true);
                    if (cameras.Length == 1)
                    {
                        m_layerCamera = cameras[0];
                    }
                    else if (cameras.Length > 0)
                    {
                        foreach (var cam in cameras)
                        {
                            if (cam.gameObject.name == "LayerCamera")
                            {
                                m_layerCamera = cam;
                                break;
                            }
                        }

                        if (m_layerCamera == null)
                            m_layerCamera = cameras[0];
                    }
                }
                return m_layerCamera;
            }
        }
        private Camera m_layerCamera;
    }
}

