using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.Scene
{
    public class SceneLayerBase : MonoBehaviour
    {
        /// <summary>
        /// ��ʼ��
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
        /// ����
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
        /// ��������
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
        /// ��prefab��ӵ�layer
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
        /// ��ʼ��
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// ����
        /// </summary>
        protected virtual void OnUnInit() { }


        #region �Ƴ�ʱ����״̬

        /// <summary>
        /// �Ƿ��Ǳ�����
        /// </summary>
        /// <returns></returns>
        public bool IsReserve()
        {
            return m_isReserve;
        }

        /// <summary>
        /// �����Ƿ���
        /// </summary>
        /// <param name="value"></param>
        public void SetReserve(bool value)
        {
            m_isReserve = value;
        }

        /// <summary>
        /// �Ƿ񱻱���
        /// �������Ĳ��ᱻ���٣�Ϊ�������Ż�
        /// </summary>
        private bool m_isReserve;

        #endregion

        /// <summary>
        /// ��ǰ�������
        /// </summary>
        public string LayerName { get; private set; }

        /// <summary>
        /// ��ǰ״̬
        /// </summary>
        public LayerState State = LayerState.None;
        public enum LayerState
        {
            None,
            Loading,        // ������
            Unused,         // δʹ��
            InStack,        // ��ջ��
            WaitForFree     // �ȴ��ͷ�
        }


        #region ��ʾ���ȼ�

        public bool IsStayOnTop;
        public int Priority;

        /// <summary>
        /// ���ò�����ȶȣ�ֵԽ��Խ�ϲ�
        /// </summary>
        /// <returns></returns>
        public void SetLayerPriority(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// ��ȡ������ȶȣ�ֵԽ��Խ�ϲ�
        /// </summary>
        /// <returns></returns>
        public int GetLayerPriority()
        {
            return Priority;
        }

        /// <summary>
        /// �ȽϺ���
        /// </summary>
        /// <param name="compareLayer"></param>
        /// <returns></returns>
        public int ComparePriority(SceneLayerBase compareLayer)
        {
            //����ʱ���ڶ�������false�����򷵻�true
            if (IsStayOnTop ^ compareLayer.IsStayOnTop)
                return IsStayOnTop ? 1 : -1;

            // ���ȶ�ԽС��layerԽС
            return Priority.CompareTo(compareLayer.Priority);
        }


        #endregion


        /// <summary>
        /// ��ȡ��ǰ���camera
        /// </summary>
        public virtual Camera LayerCamera { get { return null; } }
        /// <summary>
        /// �Ƿ��ʼ����
        /// </summary>
        private bool m_isInitialized;

        /// <summary>
        /// ���ʲ��prefab���ڵ�
        /// </summary>
        public GameObject LayerPrefabRoot { get; private set; }
    }
}

