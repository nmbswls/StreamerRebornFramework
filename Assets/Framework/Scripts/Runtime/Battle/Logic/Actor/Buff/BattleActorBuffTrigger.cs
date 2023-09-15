using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public enum EnumBuffTriggerType
    {
        Invalid,
        BattleStart,
        AddBuff,
        CauseDamage,
        OwnerLayerReach,
        BeforeCast,
        TurnStart,
        TurnEnd,
    }

    public interface IBattleActorBuffTriggerEnv
    {
        
    }

    /// <summary>
    /// 简单类型buff trigger
    /// </summary>
    public class BattleActorBuffTrigger
    {

        public BattleActorBuffTrigger(int triggerId, IBattleActorBuffTriggerEnv env)
        {

        }

        #region Fake Config

        public EnumBuffTriggerType TriggerType;

        public List<int> TriggerActionList = new List<int>();

        #endregion

        /// <summary>
        /// 尝试触发
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckTrigger(EnumBuffTriggerType triggerType, params object[] paramList)
        {
            if (!CheckCommon())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 通用校验
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckCommon()
        {
            return true;
        }

        /// <summary>
        /// 触发回调
        /// </summary>
        protected virtual void OnTrigger()
        {
            //Owner.CompResolver.TriggerResolve
        }

        #region 事件转接

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckTriggerOnBattleStart()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckTriggerOnCauseDamage(uint targetId, uint sourceId, long dmg, long realDmg)
        {
            return true;
        }

        #endregion
    }
}
