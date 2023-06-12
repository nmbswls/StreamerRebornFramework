using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// ����ģ�͵���Դ��3d����ui��
    /// </summary>
    public class UILayer3D : UILayerBase
    {
        /// <summary>
        /// ������
        /// </summary>
        public override UILayerType LayerType { get { return UILayerType.ThreeD; } }

        /// <summary>
        /// ��ȡ���Ӧ��camera
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

