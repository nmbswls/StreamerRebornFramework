using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Common
{
    /// <summary>
    /// 通用逻辑日志接口
    /// </summary>
    public interface ILogicLogger
    {
        /// <summary>
        /// 打印Debug日志
        /// </summary>
        /// <param name="className">打印日志的class名字</param>
        /// <param name="methodName">打印日志的method名字</param>
        /// <param name="content">日志内容</param>
        void LogDebug(string className, string methodName, string content);

        /// <summary>
        /// 打印Error日志
        /// </summary>
        /// <param name="className">打印日志的class名字</param>
        /// <param name="methodName">打印日志的method名字</param>
        /// <param name="content">日志内容</param>
        void LogError(string className, string methodName, string content);
    }
}
