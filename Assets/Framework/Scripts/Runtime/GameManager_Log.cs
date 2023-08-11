using My.Framework.Battle;
using My.Framework.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime
{
    public partial class GameManager : ILogicLogger
    {

        #region ILogicLogger

        public void LogDebug(string className, string methodName, string content)
        {
            Debug.Log($"[{className}][{methodName}]:content");
        }

        public void LogError(string className, string methodName, string content)
        {
            Debug.LogError($"[{className}][{methodName}]:content");
        }


        #endregion


        #region 日志

        /// <summary>
        /// 获取通用逻辑日志打印器
        /// </summary>
        /// <returns></returns>
        public ILogicLogger LogicLoggerGet()
        {
            return this;
        }

        #endregion

    }
}
