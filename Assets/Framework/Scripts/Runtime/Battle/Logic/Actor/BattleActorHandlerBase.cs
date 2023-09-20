using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Battle.Actor
{
    public interface IBattleActorOperatorEnv : IBattleActorCompProvider, IBattleActorHandlerProvider
    {
        /// <summary>
        /// 添加事件listener
        /// </summary>
        /// <param name="listener"></param>
        void AddListener(ILogicEventListener listener);

        /// <summary>
        /// 移除事件listener
        /// </summary>
        /// <param name="listener"></param>
        void RemoveListener(ILogicEventListener listener);

        /// <summary>
        /// 将事件push到pipe中
        /// </summary>
        /// <param name="logicEvent"></param>
        void PushEvent(LogicEvent logicEvent);

        #region 推送表现

        void PushProcessAndFlush(BattleShowProcess process);

        #endregion

        /// <summary>
        /// 获取actor
        /// </summary>
        /// <returns></returns>
        uint GetActorId();

        /// <summary>
        /// 获取结算器
        /// </summary>
        /// <returns></returns>
        IBattleLogicResolver GetResolver();
    }

    public abstract class BattleActorHandlerBase
    {

        protected BattleActorHandlerBase(IBattleActorOperatorEnv env)
        {
            m_env = env;
        }

        #region 生命周期

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

        #endregion

        protected IBattleActorOperatorEnv m_env;
    }
}
