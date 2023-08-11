using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    /// <summary>
    /// Hp改变包
    /// 包括伤害或治疗等
    /// </summary>
    public class HpModifyCtx
    {
        public long ModifyVal;
        
        public int SourceType;
        public uint SourceId;

        public int SourceParam1;
        public int SourceParam2;
    }
    public class BattleActorCompHpState : BattleActorCompBase
    {
        public List<HpModifyCtx> m_hpModifyCtxList = new List<HpModifyCtx>();
    }
}
