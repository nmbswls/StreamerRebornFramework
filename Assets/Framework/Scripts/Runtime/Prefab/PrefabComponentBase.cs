using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.Prefab
{
    /// <summary>
    /// 用来辅助自动绑定
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoBindAttribute : System.Attribute
    {
        public enum InitState
        {
            NotInit = 0, //不要初始化
            Active = 1, // 初始为显示
            Inactive = 2, // 初始为隐藏
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
        /// 自动绑定组件
        /// </summary>
        public virtual void BindFields(bool force = false)
        {
            if (BindFieldFinished && !force)
            {
                return;
            }

            //获取对象实例上所有public和非public的成员对象
            var fieldList = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fieldList)
            {
                // 查找AutoBindAttribute，对拥有标签的field进行绑定
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

                // 查找field的绑定对象，并赋值, 这里ctrlname默认使用fieldType.Name，在不使用luaproxy的时候这个用该市一致的
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
        /// 绑定单个对象的实现
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

            // 如果域的类型的higameobject直接赋值为找到的节点
            if (fieldType == typeof(GameObject))
            {
                field = go;
            }
            else if (fieldType.IsSubclassOf(typeof(PrefabComponentBase)))
            {
                // 查找指定类型的组件
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
            // 如果域是一个组件
            else if (fieldType.IsSubclassOf(typeof(Component)))
            {
                // 查找指定类型的组件
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

            // 初始化节点的active状态
            if (initState == AutoBindAttribute.InitState.Active)
                go.SetActive(true);
            else if (initState == AutoBindAttribute.InitState.Inactive)
                go.SetActive(false);

            return field;
        }


        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected virtual void OnBindFiledsCompleted()
        {

        }

        #region 工具

        /// <summary>
        /// 根据路径获取子节点
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject GetChildByPath(string path)
        {
            int index = path.LastIndexOf("../");

            Transform targetTans = null;
            if (index == -1)
            {
                // 拆分出子节点路径
                index = path.IndexOf('/');
                if (index == -1)
                {
                    return gameObject;
                }

                // 取得子节点路径
                string childPath = path.Substring(index + 1);

                // 获取路径指定的节点
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
                // 取得子节点路径
                string childPath = path.Substring(index + 3);
                // 获取路径指定的节点
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
        /// 是否已绑定 确保重复调用
        /// </summary>
        protected bool BindFieldFinished { get; set; }
    }
}

