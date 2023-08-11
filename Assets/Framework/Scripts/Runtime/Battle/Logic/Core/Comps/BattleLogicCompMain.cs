using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using UnityEditor.PackageManager;

namespace My.Framework.Battle.Logic
{
    public interface IBattleLogicCompMain
    {
        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        BattleController GetBattleControllerById(int controllerId);
    }

    public class BattleLogicCompMain : BattleLogicCompBase, IBattleLogicCompMain
    {
        public BattleLogicCompMain(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName { get; }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!base.Initialize())
            {
                return false;
            }

            m_compActorContainer = m_owner.CompGet<BattleLogicCompActorContainer>(GamePlayerCompNames.ProcessManager);
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            if (!base.PostInitialize())
            {
                return false;
            }

            return true;
        }

        #endregion


        #region 对外方法

        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        public BattleController GetBattleControllerById(int controllerId)
        {
            return m_controllers[controllerId];
        }

        /// <summary>
        /// 召唤actor
        /// </summary>
        /// <param name="actorConfigId"></param>
        /// <param name="actorType"></param>
        /// <param name="campId"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool SummonActor(int actorConfigId, int actorType, int campId, out BattleActor actor)
        {
            actor = m_compActorContainer.CreateActor(actorType, actorConfigId, BattleActorSourceType.Summon, campId);
            if (actor == null)
            {
                return false;
            }
            
            //actor.Init4EnterBattle();
            return true;
        }


        #endregion

        #region 组件

        /// <summary>
        /// actor
        /// </summary>
        protected BattleLogicCompActorContainer m_compActorContainer;

        /// <summary>
        /// 战斗控制器列表
        /// </summary>
        public List<BattleController> m_controllers = new List<BattleController>();

        #endregion
    }
}
