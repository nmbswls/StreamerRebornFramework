using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 组件持有
    /// </summary>
    public interface IBattleLogicCompOwnerBase
    {
        /// <summary>
        /// 获取静态基础信息组件
        /// </summary>
        /// <returns></returns>
        T CompGet<T>(string compName) where T : BattleLogicCompBase;

        /// <summary>
        /// 获取战斗
        /// </summary>
        /// <returns></returns>
        FakeBattleConfig ConfigGet();
    }
    
    /// <summary>
    /// 战斗组件基类
    /// </summary>
    public abstract class BattleLogicCompBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="owner"></param>
        protected BattleLogicCompBase(IBattleLogicCompOwnerBase owner)
        {
            m_owner = owner;
        }

        /// <summary>
        /// 获取组件名
        /// </summary>
        /// <returns></returns>
        public abstract string CompName { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool PostInitialize()
        {
            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Tick(float dt)
        {
        }

        #region 通用事件

        /// <summary>
        /// 战斗开始
        /// </summary>
        public virtual void BattleStart()
        {

        }

        #endregion

        #region 内部变量

        /// <summary>
        /// 组件宿主
        /// </summary>
        protected IBattleLogicCompOwnerBase m_owner;

        #endregion
    }
}
