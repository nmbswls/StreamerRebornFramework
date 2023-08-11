using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Rendering;

namespace My.Framework.Battle.Actor
{

    public interface IBattleActorHandlerHpState
    {
        /// <summary>
        /// 添加伤害
        /// </summary>
        void ApplyDamage(long damage);

        /// <summary>
        /// 结算伤害列表
        /// </summary>
        void ApplyHpStateModify();
    }

    /// <summary>
    /// 专门控制hp变化
    /// 其他复杂的属性也可用该模板
    /// </summary>
    public class BattleActorHandlerHpState : BattleActorHandlerBase, IBattleActorHandlerHpState
    {
        public BattleActorHandlerHpState(IBattleActorOperatorEnv env) : base(env)
        {
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            m_compBasic = m_env.BattleActorCompBasicGet();
            m_compHpState = m_env.BattleActorCompHpStateGet();

            m_handlerAttribute = m_env.BattleActorHandlerAttributeGet();
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            return true;
        }


        #endregion

        #region 对外方法

        /// <summary>
        /// 添加伤害
        /// </summary>
        public void ApplyDamage(long damage)
        {
            // 技能多段伤害 需要一次置入多个伤害ctx
            m_compHpState.m_hpModifyCtxList.Add(new HpModifyCtx(){ ModifyVal = -damage});
        }

        protected  class HpStateCalculateCtx
        {
            public long hpMax;
            public long hp;

            public long specialHp;
            public long shieldHp;

            public bool dead;

            public long hpChange;
            public long damage;

            public List<long> ShieldCostRecord = new List<long>();
            public List<long> ExtraHpCostRecord = new List<long>();
        }


        /// <summary>
        /// 结算hp变化量并应用于实际属性
        /// </summary>
        public void ApplyHpStateModify()
        {
            m_cacheDmgRecord.Clear();

            var specialHp = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.SpecialHp);
            var shieldHp = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.ShieldHp);

            //血量变化
            var hpModifyList = m_compHpState.m_hpModifyCtxList;

            bool isLethal = false;
            long hpValue = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.Hp);

            //血量节点
            var hpCalculateCtx = new HpStateCalculateCtx (){ hpMax = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.HpMax), hp = hpValue, specialHp = specialHp, shieldHp = shieldHp };
            bool needExit = false;
            var damageCalcFunc = new Action<HpModifyCtx>((HpModifyCtx modifyCtx) =>
            {
                if (hpCalculateCtx.dead == true)
                {
                    needExit = true;
                }

                if (modifyCtx.ModifyVal == 0)
                {
                    return;
                }
                
                // 无来源的伤害
                if (modifyCtx.SourceId == 0)
                {
                    return;
                }

                DmgsMark dmgsMark = null;
                if (!m_cacheDmgRecord.TryGetValue(modifyCtx.SourceId, out dmgsMark))
                {
                    m_cacheDmgRecord[modifyCtx.SourceId] = new DmgsMark();
                }

                bool bdead = hpCalculateCtx.dead;
                long hpRealMod = 0;

                HpCalculateInternal(hpCalculateCtx, modifyCtx.SourceId, modifyCtx.ModifyVal, out hpRealMod);

                // 正数表示回复
                if (modifyCtx.ModifyVal > 0)
                {
                    dmgsMark.TreatVal += modifyCtx.ModifyVal;
                    dmgsMark.TreatRealVal += hpRealMod;
                }
                else
                {
                    dmgsMark.DamageVal += modifyCtx.ModifyVal;
                    dmgsMark.DamageRealVal += hpRealMod;

                    EventOnCauseDamage?.Invoke(m_env.GetActorId(), modifyCtx.SourceId, modifyCtx.ModifyVal, hpRealMod);
                }
                
                if (bdead == false && hpCalculateCtx.dead == true)
                {
                    dmgsMark.dead = true;
                }
            });

            // 追溯效果
            hpModifyList.ForEach(damageCalcFunc);
            hpModifyList.Clear();

            // 扣除护盾
            if (shieldHp > hpCalculateCtx.shieldHp)
            {
                //扣除护盾
                CostShield(shieldHp - hpCalculateCtx.shieldHp);
            }

            //血量发生变化
            if (hpCalculateCtx.hp != hpValue)
            {
                m_handlerAttribute.SetAttributeValue(AttributeIdConsts.Hp, hpValue);
                //设置血量
                var hpChg = hpCalculateCtx.hp - hpValue;
                if (hpChg > 0)
                {
                    
                }
            }

            var nowHp = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.Hp);

            // 结算需要抛出事件通知的数据
            foreach (var kv in m_cacheDmgRecord)
            {
                ulong sourceId = kv.Key;
                if (sourceId <= 0)
                {
                    continue;
                }
                var dmgsMark = kv.Value;

                // 治疗
                if (dmgsMark.TreatRealVal != 0)
                {
                    // 抛出治疗事件
                    //beTreated(sourceId, dmgsMark);
                }
            }

            CheckHpState();
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 内部计算一个modctx
        /// </summary>
        /// <param name="hpCalculatectx"></param>
        /// <param name="sourceId"></param>
        /// <param name="modVal"></param>
        /// <param name="hpRealMod"></param>
        protected void HpCalculateInternal(HpStateCalculateCtx hpCalculatectx, uint sourceId, long modVal, out long hpRealMod)
        {
            hpRealMod = 0;

            if (modVal > 0)
            {
                hpCalculateTreat(hpCalculatectx, modVal, out var hpRealTreat);
                hpRealMod = hpRealTreat;
            }
            else
            {
                long damage = -modVal;
                long useShield = 0; // 削减的护盾量 正数 
                long useExtraHp = 0; // 削减的额外生命 正数
                long useHp = 0; // 削减的生命值 正数
                hpCalculateDamage(hpCalculatectx, damage, out useHp, out useShield, out useExtraHp);

                if (useShield > 0)
                {
                    hpCalculatectx.ShieldCostRecord.Add(useShield);
                }

                if (useExtraHp > 0)
                {
                    hpCalculatectx.ExtraHpCostRecord.Add(useExtraHp);
                }

                hpRealMod = -useHp;
            }
        }

        /// <summary>
        /// 结算治疗
        /// </summary>
        /// <param name="hpCalculatectx"></param>
        /// <param name="treatHp"></param>
        /// <param name="hpRealTreat"></param>
        protected void hpCalculateTreat(HpStateCalculateCtx hpCalculatectx, long treatHp, out long hpRealTreat)
        {
            hpRealTreat = 0;

            // 对于死亡对象 实际改变量一定为0
            if (treatHp <= 0 || hpCalculatectx.dead == true)
            {
                return;
            }

            long oldHp = hpCalculatectx.hp;
            hpCalculatectx.hp += treatHp;

            if (hpCalculatectx.hp > hpCalculatectx.hpMax)
            {
                hpCalculatectx.hp = hpCalculatectx.hpMax;
            }
            hpRealTreat = hpCalculatectx.hp - oldHp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dmg">正值</param>
        /// <param name="useHp"></param>
        /// <param name="useShield"></param>
        /// <param name="useSpecialHp"></param>
        protected void hpCalculateDamage(HpStateCalculateCtx ctx, long dmg, out long useHp, out long useShield, out long useSpecialHp)
        {
            useHp = 0;
            useShield = 0;
            useSpecialHp = 0;

            // 死亡对象 当不进行强制结算时 跳过结算
            if (ctx.dead)
            {
                return;
            }
            if ((ctx.hp <= 0 && ctx.shieldHp <= 0 && ctx.specialHp <= 0))
            {
                return;
            }

            long leftDmg = dmg;
            // 先扣除额外血量
            if (ctx.specialHp > 0)
            {
                // 足够抵消全部伤害
                if (ctx.shieldHp > leftDmg)
                {
                    useSpecialHp += leftDmg;

                    ctx.specialHp -= leftDmg;
                    leftDmg = 0;
                }
                // 不够抵消全部伤害
                else
                {
                    useSpecialHp += ctx.specialHp;

                    leftDmg = leftDmg - ctx.specialHp;
                    ctx.specialHp = 0;
                }
            }

            if (leftDmg <= 0) return;

            //long dmgHP = 0;
            if (ctx.shieldHp > 0)
            {
                // 足够抵消全部伤害
                if (ctx.shieldHp > leftDmg)
                {
                    useShield += leftDmg;

                    ctx.shieldHp -= leftDmg;
                    leftDmg = 0;
                }
                // 不够抵消全部伤害
                else
                {
                    useShield += ctx.specialHp;

                    leftDmg = leftDmg - ctx.shieldHp;
                    ctx.shieldHp = 0;
                }
            }

            if (ctx.hp > 0)
            {
                if (ctx.hp >= leftDmg)
                {
                    useHp += leftDmg;
                    ctx.hp -= leftDmg;
                    leftDmg = 0;
                }
                else
                {
                    useHp += ctx.hp;

                    leftDmg = leftDmg - ctx.hp;
                    ctx.hp = 0;
                }
            }

            if (ctx.hp <= 0)
            {
                ctx.dead = true;
            }
        }

        /// <summary>
        /// 检查死亡状态
        /// </summary>
        protected void CheckHpState()
        {
            
        }

        /// <summary>
        /// 消耗护盾
        /// </summary>
        /// <param name="useShield"></param>
        protected void CostShield(long useShield)
        {
            if (useShield <= 0)
            {
                return;
            }

            long oldShield = m_handlerAttribute.GetAttributeValue(AttributeIdConsts.ShieldHp);
            if (oldShield <= 0)
            {
                return;
            }

            // 计算实际削减量
            long shieldToCost = 0;
            if (oldShield > useShield)
            {
                shieldToCost = useShield;
            }
            else
            {
                shieldToCost = oldShield;
            }
            m_handlerAttribute.CostAttribute(AttributeIdConsts.ShieldHp, shieldToCost);
        }


        #endregion

        #region 事件

        /// <summary>
        /// 伤害事件
        /// </summary>
        public event Action<uint, uint, long, long> EventOnCauseDamage;

        #endregion

        #region 引用component

        protected BattleActorCompBasic m_compBasic;
        protected BattleActorCompHpState m_compHpState;
        #endregion

        #region 引用Handler

        protected BattleActorHandlerAttribute m_handlerAttribute;

        #endregion

        #region MyRegion


        public class DmgsMark
        {
            public bool dead;

            public long TreatVal; 
            public long TreatRealVal;

            public long DamageVal;
            public long DamageRealVal;
        }
        /// <summary>
        /// 临时管线伤害记录
        /// </summary>
        protected Dictionary<uint, DmgsMark> m_cacheDmgRecord = new Dictionary<uint, DmgsMark>();

        #endregion
    }
}
