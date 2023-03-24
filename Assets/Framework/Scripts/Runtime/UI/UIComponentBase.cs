using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{

    public class UIComponentBase : PrefabComponentBase
    {
        public string UIName { get; set; }

        /// <summary>
        /// 清理操作
        /// </summary>
        public virtual void Clear()
        {
            m_buttonClickListenerDict?.Clear();
            m_toggleValueChangedListenerDict?.Clear();
        }

        /// <summary>
        /// 可能会被Hotfix，所以override一个空实现
        /// </summary>
        /// <param name="uiName"></param>
        public virtual void Initlize(string uiName)
        {
        }

        /// <summary>
        /// 由脚本驱动
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Tick(float dt)
        {

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
        protected override UnityEngine.Object BindFieldImpl(Type fieldType, string path, AutoBindAttribute.InitState initState, string fieldName, string ctrlName = null, bool optional = false)
        {
            UnityEngine.Object field = null;
            GameObject go = GetChildByPath(path);
            // 获取子节点
            if (go == null)
            {
                if (!optional)
                    Debug.LogError(string.Format("BindFields fail can not found child {0} uictrl:{1}", path, this.UIName), gameObject);
                return null;
            }

            // 本子类需要特殊处理ui相关的组件
            if (fieldType.IsSubclassOf(typeof(Selectable)))
            {
                // 查找指定类型的组件
                Component comp = go.GetComponent(fieldType);
                if (comp == null)
                {
                    if (!optional)
                        Debug.LogError(string.Format("BindFields fail can not found comp in child path:{0} compType:{1} ctrlName:{2}", path, fieldType.Name, this.UIName), gameObject);
                    return null;
                }
                field = comp;

                // 事件注册处理
                switch (fieldType.Name)
                {
                    case "Button":
                        {
                            Button button = comp as Button;
                            if (button != null)
                            {
                                button.onClick.AddListener(() => { OnButtonClick(button, fieldName); });
                            }
                        }
                        break;
                    case "Toggle":
                        {
                            Toggle toggle = comp as Toggle;
                            if (toggle != null)
                            {
                                toggle.onValueChanged.AddListener((value) =>
                                {
                                    OnToggleValueChanged(toggle, fieldName, value);
                                });
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // 如果域的类型的higameobject直接赋值为找到的节点
                field = base.BindFieldImpl(fieldType, path, initState, fieldName, ctrlName, optional);
            }

            // 初始化节点的active状态
            if (initState == AutoBindAttribute.InitState.Active)
                go.SetActive(true);
            else if (initState == AutoBindAttribute.InitState.Inactive)
                go.SetActive(false);

            return field;
        }

        /// <summary>
        /// 当某个被绑定的按钮点击
        /// </summary>
        /// <param name="button"></param>
        /// <param name="fieldName"></param>
        // ReSharper disable once UnusedParameter.Local
        protected void OnButtonClick(Button button, string fieldName)
        {
            if (m_buttonClickListenerDict == null)
            {
                return;
            }

            if (m_buttonClickListenerDict.TryGetValue(fieldName, out var action))
            {
                if (action != null)
                {
                    action(this);
                }
            }
        }

        /// <summary> 
        /// 添加按钮点击事件的监听
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="action"></param>
        public void SetButtonClickListener(string fieldName, Action<UIComponentBase> action)
        {
            if (m_buttonClickListenerDict == null)
            {
                m_buttonClickListenerDict = new Dictionary<string, Action<UIComponentBase>>();
            }
            m_buttonClickListenerDict[fieldName] = action;
        }

        /// <summary>
        /// Toggle点击事件处理
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        private void OnToggleValueChanged(Toggle toggle, string fieldName, bool value)
        {
            if (m_toggleValueChangedListenerDict == null)
            {
                return;
            }

            if (m_toggleValueChangedListenerDict.TryGetValue(fieldName, out var action))
            {
                if (action != null)
                {
                    action(this, value);
                }
            }
        }

        

        // 监听事件
        protected Dictionary<string, Action<UIComponentBase>> m_buttonClickListenerDict;
        protected Dictionary<string, Action<UIComponentBase, bool>> m_toggleValueChangedListenerDict;

        
    }
}

