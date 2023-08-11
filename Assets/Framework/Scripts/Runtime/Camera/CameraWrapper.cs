using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime
{
    public class CameraWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainCamera">主摄像机</param>
        /// <param name="go">虚拟摄像机根节点</param>
        public CameraWrapper(Camera mainCamera, GameObject go)
        {
            m_mainCamera = mainCamera;
            if (go != null)
            {
                m_root = go;
                m_virtualCamera = m_root.GetComponentInChildren<CinemachineVirtualCamera>(true);
            }
            else
                Debug.LogError("ErrorMsg: CinemachineVirtualCamera == null");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            if (m_virtualCamera != null)
            {
                m_virtualCamera.Priority = VirtualCameraPriority;
            }
            else
            {
                Debug.Log("ErrorMsg: m_virtualCamera == null");
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        public virtual void UnInitialize()
        {
            Disable();
        }

        #region Implementation of ICameraMode

        /// <summary>
        /// Tick 驱动
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Tick(float dt)
        {
        }

        /// <summary>
        /// 激活
        /// </summary>
        public virtual void Enable()
        {
            if (m_virtualCamera != null)
            {
                m_virtualCamera.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("ErrorMsg: m_virtualCamera == null");
            }
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public virtual void Disable()
        {
            if (m_virtualCamera != null)
            {
                m_virtualCamera.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("ErrorMsg: m_virtualCamera == null");
            }
        }

        public virtual void Reset() { }

        #endregion

        public Vector3 Position
        {
            get
            {
                return m_virtualCamera.transform.position;
            }
        }

        /// <summary>
        /// 虚拟相机的根
        /// </summary>
        protected GameObject m_root;

        /// <summary>
        /// 主相机
        /// </summary>
        private Camera m_mainCamera;

        public Camera MainCamera
        {
            get
            {
                if (m_mainCamera == null)
                {
                    m_mainCamera = Camera.main;
                }
                return m_mainCamera;
            }
        }

        private CinemachineBrain m_brain;
        public CinemachineBrain Brain
        {
            get
            {
                if (m_brain == null)
                    m_brain = MainCamera.GetComponent<CinemachineBrain>();
                return m_brain;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fFar"></param>
        public virtual void SetVirtualCameraFar(float fFar)
        {
            if (m_virtualCamera != null)
                m_virtualCamera.m_Lens.FarClipPlane = fFar;
        }

        /// <summary>
        /// 虚拟摄像机
        /// </summary>
        protected CinemachineVirtualCamera m_virtualCamera;

        protected virtual Int32 VirtualCameraPriority
        {
            get { return 11; }
        }
    }
}
