using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// 结算源类型
    /// </summary>
    public enum EnumTriggereSourceType
    {
        Invalid = 0,
        BeforeAttack,
        Attack,
        AfterAttack,
        BattleStart,
        TurnStart,
        TurnEnd,
    }

    /// <summary>
    /// 触发节点
    /// </summary>
    public abstract class TriggerNode
    {
        public abstract EnumTriggereSourceType SourceType { get; }
    }

    public class TriggerNodeBeforeAttack : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.BeforeAttack; }}

        public uint AttackerId;
        public List<uint> TargetList = new List<uint>();
    }

    public class TriggerNodeAttack : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.Attack; } }

        public uint AttackerId;
        public List<uint> TargetList = new List<uint>();
    }

    public class TriggerNodeAfterAttack : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.AfterAttack; } }

        public uint AttackerId;
        public List<uint> TargetList = new List<uint>();
    }

    public class TriggerNodeBattleStart : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.BattleStart; } }

        public uint AttackerId;
        public List<uint> TargetList = new List<uint>();
    }


    public class TriggerNodeTurnStart : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.TurnStart; } }

        /// <summary>
        /// 回合数
        /// </summary>
        public int TurnNumber;

        /// <summary>
        /// 阵营Id
        /// </summary>
        public int ControllerId;
    }

    public class TriggerNodeTurnEnd : TriggerNode
    {
        public override EnumTriggereSourceType SourceType { get { return EnumTriggereSourceType.TurnEnd; } }

        /// <summary>
        /// 回合数
        /// </summary>
        public int TurnNumber;

        /// <summary>
        /// 阵营Id
        /// </summary>
        public int ControllerId;
    }

}
