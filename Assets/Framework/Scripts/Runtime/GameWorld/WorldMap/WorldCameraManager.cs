using My.Framework.Runtime.Director;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 相机管理器
    /// </summary>
    public class WorldCameraManager
    {

        public bool Initialize(Transform virtualCameraRoot, Camera camera)
        {

            m_virtualCameraRoot = virtualCameraRoot;

            m_sceneCamera = new CameraWrapper(camera, m_virtualCameraRoot.gameObject);
            m_sceneCamera.Initialize();
            m_sceneCamera.Enable();

            m_isInit = true;
            return m_isInit;
        }

        /// <summary>
        /// 注销清理操作
        /// </summary>
        public void UnInitialize()
        {
            if (m_isInit == false)
            {
                return;
            }
            m_isInit = false;
            m_virtualCameraRoot = null;
            m_sceneCamera?.UnInitialize();
        }

        protected bool m_isInit;

        /// <summary>
        /// 封装相机
        /// </summary>
        protected CameraWrapper m_sceneCamera;

        public CameraWrapper SceneCamera
        {
            get { return m_sceneCamera; }
        }

        public Transform m_virtualCameraRoot;
    }
}
