using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 场景管理基础类 
    /// </summary>
    public class SceneHandlerBase : MonoBehaviour
    {
        /// <summary>
        /// 包含的unity场景对象
        /// </summary>
        public Scene UnityScene;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// 是否保留
        /// 保留的场景在切换时必定不销毁
        /// </summary>
        public virtual bool IsReserve { get { return false; } }

        /// <summary>
        /// 主场景根节点
        /// </summary>
        public GameObject MainRootGameObject;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 激活显示
        /// </summary>
        public void SetActive()
        {

        }

        /// <summary>
        /// 挂起场景
        /// 该弄掉的动态actor全弄掉
        /// </summary>
        public void Suspend()
        {

        }
    }
}
