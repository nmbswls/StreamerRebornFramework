using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 普通单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T m_instance;

        public static T Default
        {
            get { return Instance; }
        }

        public static T Instance
        {
            get
            {
                CreateInstance();
                return m_instance;
            }
        }

        public static bool HasInstance
        {
            get { return m_instance != null; }
        }

        public static void CreateInstance()
        {
            if (m_instance == null)
            {
                m_instance = new T();
                m_instance.Init();
            }
        }


        public static void DestroyInstance()
        {
            if (m_instance != null)
            {
                m_instance.UnInit();
                m_instance = null;
            }
        }

        protected virtual void Init()
        {

        }

        protected virtual void UnInit()
        {

        }
    }

    /// <summary>
    /// Mono单例 保证DontUnload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        #region Fields

        /// <summary>
        /// The instance.
        /// </summary>
        private static T instance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}
