using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public interface  IBattleActorBuffLastEffecrEnv
    {
        /// <summary>
        /// 获取唯一键
        /// </summary>
        /// <param name="lastEffectId"></param>
        /// <returns></returns>
        ulong GetUniqueInstanceId(int lastEffectId);

        /// <summary>
        /// 施加属性改变节点
        /// </summary>
        /// <param name="modifierId"></param>
        /// <param name="attributeId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool UpdateAttributeModifier(ulong modifierId, int attributeId, long value);
    }
    public abstract class BattleActorBuffLastEffect
    {
        /// <summary>
        /// 获取
        /// </summary>
        public abstract int LastEffectId { get; }

        protected BattleActorBuffLastEffect(IBattleActorBuffLastEffecrEnv env)
        {
            m_env = env;
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnEnd()
        {

        }

        protected IBattleActorBuffLastEffecrEnv m_env;
    }

    /// <summary>
    /// 基础增加属性
    /// </summary>
    public class BattleActorBuffLastCommon : BattleActorBuffLastEffect
    {
        public BattleActorBuffLastCommon(List<int> modAttributes, IBattleActorBuffLastEffecrEnv env) : base(env)
        {
        }

        /// <summary>
        /// 基础属性
        /// </summary>
        public List<int> ModifierList = new List<int>();
        protected int m_effectId;

        /// <summary>
        /// 属性变化恒定为0
        /// </summary>
        public override int LastEffectId
        {
            get { return 0; }
        }

        public override void OnStart()
        {
            var uniqueId = m_env.GetUniqueInstanceId(LastEffectId);

            foreach (var attributeId in ModifierList)
            {
                long nValue = 100;

                m_env.UpdateAttributeModifier(uniqueId, attributeId, nValue);
            }
        }

        
    }

    /// <summary>
    /// 属性加成与生命关联
    /// </summary>
    public class BattleActorBuffLastHpRelate : BattleActorBuffLastEffect
    {
        public int ConfigId;

        public BattleActorBuffLastHpRelate(IBattleActorBuffLastEffecrEnv env) : base(env)
        {
        }

        public override int LastEffectId { get; }
    }
}
