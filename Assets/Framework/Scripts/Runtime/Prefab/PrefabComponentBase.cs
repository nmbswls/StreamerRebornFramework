using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.Prefab
{
    /// <summary>
    /// ���������Զ���
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoBindAttribute : System.Attribute
    {
        public enum InitState
        {
            NotInit = 0, //��Ҫ��ʼ��
            Active = 1, // ��ʼΪ��ʾ
            Inactive = 2, // ��ʼΪ����
        }

        public AutoBindAttribute(string path, InitState initState = InitState.NotInit, bool optional = false)
        {
            m_path = path;
            m_initState = initState;
            m_optional = optional;
        }


        public readonly string m_path;
        public readonly InitState m_initState;
        public readonly bool m_optional;
    }

    public class PrefabComponentBase : MonoBehaviour
    {
        /// <summary>
        /// �Զ������
        /// </summary>
        public virtual void BindFields(bool force = false)
        {
            if (BindFieldFinished && !force)
            {
                return;
            }

            //��ȡ����ʵ��������public�ͷ�public�ĳ�Ա����
            var fieldList = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fieldList)
            {
                // ����AutoBindAttribute����ӵ�б�ǩ��field���а�
                var attrList = field.GetCustomAttributes(typeof(AutoBindAttribute), false);
                if (attrList.Length == 0)
                {
                    continue;
                }
                AutoBindAttribute bindAttr = null;
                foreach (var attr in attrList)
                {
                    if (attr is AutoBindAttribute)
                    {
                        bindAttr = attr as AutoBindAttribute;
                        break;
                    }
                }
                if (bindAttr == null)
                {
                    continue;
                }
                string path = bindAttr.m_path;
                bool optional = bindAttr.m_optional;
                var fieldType = field.FieldType;

                // ����field�İ󶨶��󣬲���ֵ, ����ctrlnameĬ��ʹ��fieldType.Name���ڲ�ʹ��luaproxy��ʱ������ø���һ�µ�
                var fieldValue = BindFieldImpl(fieldType, path, bindAttr.m_initState, field.Name, fieldType.Name, optional);
                if (fieldValue != null)
                {
                    field.SetValue(this, fieldValue);
                }
            }

            BindFieldFinished = true;

            OnBindFiledsCompleted();
        }

        /// <summary>
        /// �󶨵��������ʵ��
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="path"></param>
        /// <param name="initState"></param>
        /// <param name="fieldName"></param>
        /// <param name="ctrlName"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        protected virtual UnityEngine.Object BindFieldImpl(Type fieldType, string path, AutoBindAttribute.InitState initState, string fieldName, string ctrlName = null, bool optional = false)
        {

            UnityEngine.Object field = null;

            GameObject go;

            go = GetChildByPath(path);

            if (go == null)
            {
                if (!optional)
                {
                    Debug.LogError(string.Format("BindFields fail can not found child {0} in {1} uictrl:{2}", path, gameObject, GetType().Name), gameObject);
                }
                return null;
            }

            // ���������͵�higameobjectֱ�Ӹ�ֵΪ�ҵ��Ľڵ�
            if (fieldType == typeof(GameObject))
            {
                field = go;
            }
            else if (fieldType.IsSubclassOf(typeof(PrefabComponentBase)))
            {
                // ����ָ�����͵����
                var comp = go.GetComponent(fieldType) as PrefabComponentBase;
                if (comp == null)
                {
                    if (!optional)
                    {
                        Debug.LogError(string.Format("BindFields fail can not found comp in child {0} {1}", path, fieldType.Name));
                    }
                    return null;
                }
                field = comp;
            }
            // �������һ�����
            else if (fieldType.IsSubclassOf(typeof(Component)))
            {
                // ����ָ�����͵����
                var comp = go.GetComponent(fieldType);
                if (comp == null)
                {
                    if (!optional)
                    {
                        Debug.LogError(string.Format("BindFields fail can not found comp in child {0} {1} {2}", path, fieldType.Name, name), gameObject);
                    }

                    return null;
                }
                field = comp;
            }

            // ��ʼ���ڵ��active״̬
            if (initState == AutoBindAttribute.InitState.Active)
                go.SetActive(true);
            else if (initState == AutoBindAttribute.InitState.Inactive)
                go.SetActive(false);

            return field;
        }


        /// <summary>
        /// ��ɰ󶨻ص�
        /// </summary>
        protected virtual void OnBindFiledsCompleted()
        {

        }

        #region ����

        /// <summary>
        /// ����·����ȡ�ӽڵ�
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject GetChildByPath(string path)
        {
            int index = path.LastIndexOf("../");

            Transform targetTans = null;
            if (index == -1)
            {
                // ��ֳ��ӽڵ�·��
                index = path.IndexOf('/');
                if (index == -1)
                {
                    return gameObject;
                }

                // ȡ���ӽڵ�·��
                string childPath = path.Substring(index + 1);

                // ��ȡ·��ָ���Ľڵ�
                targetTans = transform.Find(childPath);
            }
            else
            {
                Transform proot = transform;
                int deep = index / 3;
                while (deep >= 0)
                {
                    deep--;
                    proot = proot.parent;
                }
                // ȡ���ӽڵ�·��
                string childPath = path.Substring(index + 3);
                // ��ȡ·��ָ���Ľڵ�
                targetTans = proot.Find(childPath);
            }

            if (targetTans == null)
            {
                return null;
            }
            return targetTans.gameObject;
        }

        #endregion


        /// <summary>
        /// �Ƿ��Ѱ� ȷ���ظ�����
        /// </summary>
        protected bool BindFieldFinished { get; set; }
    }
}

