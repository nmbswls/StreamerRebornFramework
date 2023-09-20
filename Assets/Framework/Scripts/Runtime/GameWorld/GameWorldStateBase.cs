using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace My.Framework.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameWorldStateEnvBase
    {
        /// <summary>
        /// 执行协程
        /// </summary>
        /// <param name="corutine"></param>
        void StartCorutine(IEnumerator corutine);

        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="isStopPre"></param>
        void ChangeState(int newState, bool isStopPre = true);
    }

    /// <summary>
    /// scene 管理器
    /// </summary>
    public class SceneHandlerHall : SceneHandlerBase
    {
        public Transform m_dynamicRoot;
    }
    

    /// <summary>
    /// 状态定义
    /// </summary>
    public class GameWorldStateTypeDefineBase
    {
        public const int None = 0;
        public const int SimpleHall = 1;
        public const int SimpleMap = 2;
    }

}
