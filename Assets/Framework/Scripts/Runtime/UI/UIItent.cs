using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIIntent
    {
        public UIIntent(string uiName, string targetMode = "")
        {
            TargetUIName = uiName;
            TargetMode = targetMode;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetParam(string key, object value)
        {
            if (value == null)
            {
                object o;
                if (TryGetParam(key, out o))
                    m_params.Remove(key);
            }
            else
                m_params[key] = value;
        }

        public bool TryGetParam(string key, out object value)
        {
            return m_params.TryGetValue(key, out value);
        }

        public bool TryGetParam<T>(string key, out T value)
        {
            value = default(T);
            if (!m_params.TryGetValue(key, out object rawVal))
            {
                return false;
            }
            value = (T)rawVal;
            return true;
        }

        private void SetParam<T>(string key, T value) where T : struct, IEquatable<T>
        {
            object o;
            if (!TryGetParam(key, out o) || !((T)o).Equals(value))
                m_params[key] = value;
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetClassParam<T>(string key) where T : class
        {
            object o;
            if (TryGetParam(key, out o))
                return o as T;
            return default(T);
        }

        /// <summary>
        /// 想要开启的task目标
        /// </summary>
        public string TargetUIName { get; private set; }
        /// <summary>
        /// 需要开启的模式
        /// </summary>
        public string TargetMode { get; set; }

        private readonly Dictionary<string, object> m_params = new Dictionary<string, object>();
    }

    public class UIIntentReturnable : UIIntent
    {
        public UIIntentReturnable(UIIntent prevTaskIntent, string targetTaskName, string targetMode = null)
            : base(targetTaskName, targetMode)
        {
            PrevUIIntent = prevTaskIntent;

            uiStack.Clear();
            CollapseRetIntent(uiStack);
        }

        static List<string> uiStack = new List<string>();

        public void CollapseRetIntent(List<string> taskstack)
        {
            taskstack.Add(TargetUIName);
            if (taskstack.Count > 10)
                return; //保护

            int maxcheck = 10;
            while (PrevUIIntent != null && maxcheck > 0)
            {
                int retidx = taskstack.IndexOf(PrevUIIntent.TargetUIName);

                if (retidx >= 0)
                {
                    var retpreintent = (PrevUIIntent as UIIntentReturnable);
                    if (retpreintent != null)
                        PrevUIIntent = retpreintent.PrevUIIntent;
                    else
                        PrevUIIntent = null;
                }
                else
                    break;
                maxcheck--;
            }

            if (PrevUIIntent != null)
            {
                var retpreintent = (PrevUIIntent as UIIntentReturnable);
                if (retpreintent != null)
                {
                    retpreintent.CollapseRetIntent(taskstack);
                }
            }
        }

        public UIIntent PrevUIIntent { set; get; }
    }
}

