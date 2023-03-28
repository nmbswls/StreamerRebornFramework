using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Scene
{
    public class SceneLayerBase : MonoBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (m_isInitialized)
            {
                return;
            }
            m_isInitialized = true;
            OnInit();
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void UnInit()
        {
            if (!m_isInitialized)
            {
                return;
            }
            m_isInitialized = false;
            OnUnInit();
        }

        /// <summary>
        /// 设置名称
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(LayerName))
            {
                LayerName = name;
            }
            else
            {
                throw new Exception(string.Format("SceneLayerBase.SetName={0} to layer already have name={1}", name, LayerName));
            }
        }

        /// <summary>
        /// 将prefab添加到layer
        /// </summary>
        /// <param name="layerPrefabGo"></param>
        public void AttachGameObject(GameObject go)
        {
            if (go == null)
                return;
            OnAttachGameObject(go);
            LayerPrefabRoot = go;
        }

        protected virtual void OnAttachGameObject(GameObject go)
        {
            Vector3 pos = go.transform.localPosition;
            Vector3 scale = go.transform.localScale;
            Quaternion rotation = go.transform.localRotation;
            go.transform.SetParent(transform, true);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localRotation = rotation;
        }

        public void SetLayerPrefabRoot(GameObject go)
        {
            LayerPrefabRoot = go;
        }

        

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 回收
        /// </summary>
        protected virtual void OnUnInit() { }


        #region 移除时保留状态

        /// <summary>
        /// 是否是保留的
        /// </summary>
        /// <returns></returns>
        public bool IsReserve()
        {
            return m_isReserve;
        }

        /// <summary>
        /// 设置是否保留
        /// </summary>
        /// <param name="value"></param>
        public void SetReserve(bool value)
        {
            m_isReserve = value;
        }

        /// <summary>
        /// 是否被保留
        /// 被保留的不会被销毁，为了性能优化
        /// </summary>
        private bool m_isReserve;

        #endregion

        /// <summary>
        /// 当前层的名称
        /// </summary>
        public string LayerName { get; private set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public LayerState State = LayerState.None;
        public enum LayerState
        {
            None,
            Loading,        // 加载中
            Unused,         // 未使用
            InStack,        // 在栈中
            WaitForFree     // 等待释放
        }


        #region 显示优先级

        public bool IsStayOnTop;
        public int Priority;

        /// <summary>
        /// 设置层的优先度，值越大，越上层
        /// </summary>
        /// <returns></returns>
        public void SetLayerPriority(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// 获取层的优先度，值越大，越上层
        /// </summary>
        /// <returns></returns>
        public int GetLayerPriority()
        {
            return Priority;
        }

        /// <summary>
        /// 比较函数
        /// </summary>
        /// <param name="compareLayer"></param>
        /// <returns></returns>
        public int ComparePriority(SceneLayerBase compareLayer)
        {
            //相异时，在顶部返回false，否则返回true
            if (IsStayOnTop ^ compareLayer.IsStayOnTop)
                return IsStayOnTop ? 1 : -1;

            // 优先度越小的layer越小
            return Priority.CompareTo(compareLayer.Priority);
        }


        #endregion


        /// <summary>
        /// 获取当前层的camera
        /// </summary>
        public virtual Camera LayerCamera { get { return null; } }
        /// <summary>
        /// 是否初始化过
        /// </summary>
        private bool m_isInitialized;

        /// <summary>
        /// 访问层的prefab根节点
        /// </summary>
        public GameObject LayerPrefabRoot { get; private set; }
    }
}

