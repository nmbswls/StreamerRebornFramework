using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// 包含模型等资源的3d类型ui层
    /// </summary>
    public class UILayer3D : UILayerBase
    {
        /// <summary>
        /// 层类型
        /// </summary>
        public override UILayerType LayerType { get { return UILayerType.ThreeD; } }

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
        protected Camera m_layerCamera;
    }
}

