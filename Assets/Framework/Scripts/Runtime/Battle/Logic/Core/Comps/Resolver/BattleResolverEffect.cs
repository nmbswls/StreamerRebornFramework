using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public interface IEffectNodeEnv
    {

    }
    public enum BattleEffectType
    {
        Invalid,
        AddBuff,
        Spawn,
    }
    public abstract class EffectNode
    {
        protected EffectNode(IEffectNodeEnv env)
        {
            m_env = env;
        }

        /// <summary>
        /// 是否准备完毕
        /// </summary>
        /// <returns></returns>
        public virtual bool IsReady()
        {
            return true;
        }

        /// <summary>
        /// 处理标记位
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected IEffectNodeEnv m_env;
        public abstract BattleEffectType EffectType { get; }
    }

    
    public static class BattleEffectNodeFactory
    {
        
        
    }

    public class EffectNodeBuff : EffectNode
    {
        public override BattleEffectType EffectType { get { return BattleEffectType.AddBuff; } }

        public EffectNodeBuff(IEffectNodeEnv env) : base(env)
        {
        }
    }

}
