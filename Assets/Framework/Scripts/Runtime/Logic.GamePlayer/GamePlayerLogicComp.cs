using My.Framework.Runtime.Common;
using My.Framework.Runtime.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    /// <summary>
    /// 容器
    /// </summary>
    public interface IGamePlayerCompOwnerBase
    {
        /// <summary>
        /// 获取通用逻辑日志打印器
        /// </summary>
        /// <returns></returns>
        ILogicLogger LogicLoggerGet();

        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <returns></returns>
        IDataContainerHolder GetDataProvider();
    }

    public abstract class GamePlayerCompBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="owner"></param>
        protected GamePlayerCompBase(IGamePlayerCompOwnerBase owner)
        {
            m_owner = owner;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool Initlialize()
        {
            return true;
        }

        /// <summary>
        /// 卸载
        /// </summary>
        /// <returns></returns>
        public virtual bool UnInitialize()
        {
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool PostInitlialize()
        {
            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="currTime"></param>
        public virtual void Tick(uint currTime)
        {
        }


        #region 内部变量

        /// <summary>
        /// 组件宿主
        /// </summary>
        protected IGamePlayerCompOwnerBase m_owner;

        #endregion
    }
}
