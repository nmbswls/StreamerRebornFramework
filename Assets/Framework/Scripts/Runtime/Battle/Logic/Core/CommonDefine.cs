using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle
{
    /// <summary>
    /// 战斗开启信息
    /// </summary>
    [Serializable]
    public class BattleLaunchInfo
    {
        /// <summary>
        /// BattleLaunchReason
        /// </summary>
        public int m_launchReason;

        /// <summary>
        /// BattleLaunchReason参数
        /// </summary>
        public int m_reasonParam;

        /// <summary>
        /// 场景bufInsId列表
        /// </summary>
        public List<uint> m_externBufList;
    }

    /// <summary>
    /// 战斗额外初始化信息
    /// </summary>
    public class BattleInitInfoBase
    {
        /// <summary>
        /// 是否需要进战表现
        /// </summary>
        public bool m_needEnterBattlePerform;

        /// <summary>
        /// 入场信息
        /// </summary>
        public List<uint> m_acterList = new List<uint>();


        #region Overrides of Object

        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// 战斗结算信息
    /// </summary>
    public class BattleResultInfo
    {
        public bool IsWin;
    }

    public class BattleFinishRule
    {
        public string RuleType;
        public int param;
    }

    public class FakeBattleConfig
    {
        public List<BattleFinishRule> BattleFinishRule = new List<BattleFinishRule>();

        public static FakeBattleConfig GetFake()
        {
            var config = new FakeBattleConfig();
            config.BattleFinishRule.Add(new BattleFinishRule(){ RuleType = "EnemyDie" });
            config.BattleFinishRule.Add(new BattleFinishRule() { RuleType = "Turn" });
            return config;
        }
    }

    public class FakeBattleActorBuffConfig
    {
        public List<FakeBattleActorBuffActionConfig> BuffActionConfigs = new List<FakeBattleActorBuffActionConfig>();
        public List<FakeBattleActorBuffLastConfig> BuffLastConfigs = new List<FakeBattleActorBuffLastConfig>();
        public List<FakeBattleActorBuffTriggerConfig> BuffTriggerConfigs = new List<FakeBattleActorBuffTriggerConfig>();

        public List<int> AttributeModifers = new List<int>();
        public static FakeBattleActorBuffConfig CreateFake()
        {
            var ret = new FakeBattleActorBuffConfig();
            ret.BuffTriggerConfigs.Add(new FakeBattleActorBuffTriggerConfig(){ TriggerId  = 100});
            return ret;
        }
    }

    public class FakeBattleActorBuffActionConfig
    {
        public int ConfigId;
        public string ActionType = "Damage";
    }

    public class FakeBattleActorBuffLastConfig
    {
        public int LastId;
        public int LastType;
    }

    public class FakeBattleActorBuffTriggerConfig
    {
        public int TriggerId;
    }

    /// <summary>
    /// 最基础的
    /// </summary>
    public static class BattleActorTypeStartup
    {
        public const int Player = 1;
        public const int Enemy = 2;
    }
}
