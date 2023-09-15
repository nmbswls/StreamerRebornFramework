using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    public class EffectNodeAddBuff : EffectNode
    {
        public override BattleEffectType EffectType { get { return BattleEffectType.AddBuff; } }

        public EffectNodeAddBuff() : base()
        {

        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns>是否结束 false表示需要等待</returns>
        public override bool HandleEffect()
        {
            uint actorId = 1;
            //var actor = m_compActorContainer.GetActor(actorId);
            //actor.AddBuff(1);
            return true;
        }

        /// <summary>
        /// 施加者actorId
        /// </summary>
        public uint SourceActorId = 0;

        /// <summary>
        /// 
        /// </summary>
        public uint TargetActorId = 0;

        /// <summary>
        /// 添加buffid
        /// </summary>
        public int BuffId = 0;
    }

}
