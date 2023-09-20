using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{

    public enum BattleEffectType
    {
        Invalid,
        Show,
        AddBuff,
        Spawn,
        ChooseCard,
        Damage,
    }

    public abstract class EffectNode
    {

        protected EffectNode()
        {
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns>是否结束 false表示需要等待</returns>
        public virtual bool HandleEffect()
        {
            return true;
        }

        /// <summary>
        /// 节点是否需要等待表现层结束
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedWaitProcess()
        {
            return false;
        }

        /// <summary>
        /// 处理标记位
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 依赖数据是否准备完毕
        /// </summary>
        public bool IsDataReady = true;

        /// <summary>
        /// 
        /// </summary>
        public abstract BattleEffectType EffectType { get; }

    }
}
