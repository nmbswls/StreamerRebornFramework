using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace My.Framework.Battle.Actor
{
    public interface IBattleActorHandlerAttribute
    {
        /// <summary>
        /// 创建或更新属性修饰器
        /// </summary>
        /// <param name="modifierId"></param>
        /// <param name="attributeId"></param>
        /// <param name="value"></param>
        /// <param name="setExist"></param>
        /// <returns></returns>
        bool UpdateAttributeModifier(ulong modifierId, int attributeId, long value, bool setExist = true);
    }


    public class BattleActorHandlerAttribute : BattleActorHandlerBase, IBattleActorHandlerAttribute
    {
        public BattleActorHandlerAttribute(IBattleActorOperatorEnv env) : base(env)
        {
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            m_compAttribute = m_env.BattleActorCompAttributeGet();
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            InitAttributes();

            return true;
        }


        #endregion

        /// <summary>
        /// 初始化属性 监听变化
        /// </summary>
        public virtual void InitAttributes()
        {
            foreach (var attribute in m_compAttribute.AttributeContainer)
            {
                attribute.Value.EventOnAttributeAdd += OnAttributeChange;
            }
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public long GetAttributeValue(int attributeId)
        {
            var attribute = GetAttributeData(attributeId);
            if (attribute == null)
            {
                return 0;
            }

            if (!attribute.IsDirty())
            {
                return attribute.Value;
            }
            attribute.RefreshCache();
            return attribute.Value;
        }

        /// <summary>
        /// 创建或更新属性修饰器
        /// </summary>
        /// <param name="modifierId"></param>
        /// <param name="attributeId"></param>
        /// <param name="value"></param>
        /// <param name="setExist"></param>
        /// <returns></returns>
        public bool UpdateAttributeModifier(ulong modifierId, int attributeId, long value, bool setExist = true)
        {
            var attribute = GetAttributeData(attributeId);
            if (attribute == null) return false;
            var existModifier = attribute.GetModiferById(modifierId); 

            // 不存在时 添加
            if (existModifier == null)
            {
                attribute.AddBuffModifier(modifierId);
                return true;
            }

            long oldValue = attribute.Value;
            long newValue = 0;
            if (setExist)
            {
                newValue = value;
            }
            else
            {
                newValue = oldValue + newValue;
            }
            long newValuelCamp = CheckAttributeBound(newValue, attributeId);
            if (oldValue != newValuelCamp)
            {
                attribute.Value = newValuelCamp;
                attribute.SetDirty(true);
            }

            return true;
        }

        /// <summary>
        /// 扣减护盾等可以积攒的属性值
        /// </summary>
        /// <param name="attributeId"></param>
        /// <param name="costVal"></param>
        /// <returns></returns>
        public bool CostAttribute(int attributeId, long costVal)
        {
            var attributeData = GetAttributeData(attributeId);
            if (attributeData == null)
            {
                return false;
            }

            // 消耗不能为负数
            if (costVal <= 0)
            {
                // 扣除异常
                return false;
            }

            var modifierList = attributeData.BuffModifiers;
            long toCost = costVal;
            List<string> costInfos = new List<string>();
            // 实际值
            foreach (var modifier in modifierList)
            {
                if (toCost <= 0)
                {
                    break;
                }
                
                var value = modifier.Value;
                if (value <= 0)
                {
                    continue;
                }

                long nowCost = 0;
                if (value > toCost)
                {
                    value = value - toCost;
                    nowCost = toCost;
                    toCost = 0;
                }
                else
                {
                    nowCost = value;
                    toCost = toCost - value;
                    value = 0;
                }

                costInfos.Add($"node.getId(), node.getSourceId(), (int32_t)nowCost, (int32_t)nValue, node.getSrcEffectId()");

                // 之后清理值为0的modifier
                modifier.Value = value;

                attributeData.SetDirty(true);
            }

            //按照buff加入顺序扣除
            OnCostAttribute(attributeId, costInfos);
            return true;
        }

        /// <summary>
        /// 直接设置属性值
        /// 适用于血量之类的计量类属性
        /// </summary>
        public void SetAttributeValue(int attributeId, long newValue)
        {
            if (newValue < 0)
            {
                newValue = 0;
            }
            var maxValue = GetAttributeMax(attributeId);
            if (newValue > maxValue)
            {
                newValue = maxValue;
            }
            var data = GetAttributeData(attributeId);
            if (data == null) return;
            if (data.Value == newValue)
            {
                return;
            }
            data.Value = newValue;
        }

        #region 内部方法

        /// <summary>
        /// 获取指定属性值
        /// </summary>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        protected BattleActorAttribute GetAttributeData(int attributeId)
        {
            if (m_compAttribute.AttributeContainer.TryGetValue(attributeId, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 针对每种属性进行裁剪
        /// </summary>
        /// <param name="attriValue"></param>
        /// <param name="attriType"></param>
        /// <returns></returns>
        protected virtual long CheckAttributeBound(long attriValue, int attriType)
        {
            long value = attriValue;
            return value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeId"></param>
        /// <param name="addValue"></param>
        protected virtual void OnAttributeChange(int attributeId, long addValue)
        {

        }

        protected virtual void OnCostAttribute(int attributeId, List<string> costInfos)
        {
            foreach (var costInfo in costInfos)
            {
                //if (costAttriNode._left == 0)
                //{
                //    // 效果消失事件
                //    getBuffManager().raiseSubjectEvent<TAttributeUsedTrigger>(TRIGGER_ATTRIBUTE_USED, costAttriNode._id);
                //}

                //if (nType == tps::Attri_Shield)
                //{
                //    SkillableActor* pSkillableActor = dynamic_cast<SkillableActor*>(pActorManager->getActor(costAttriNode._scourceId));
                //    if (pSkillableActor)
                //    {
                //        ActorStatData* pStatData = pSkillableActor->getStatData();
                //        if (pStatData)
                //        {
                //            pStatData->setShieldEffective(pStatData->getShieldEffective() + (uint32_t)(costAttriNode._cost / s_int1000));
                //        }
                //    }
                //}
                //if (nType == tps::Attri_ImmimuteDead)
                //{
                //    raiseEvent(E_ETD_Immortal, costAttriNode._configId);
                //}
            }
        }

        /// <summary>
        /// 获得指定属性对应的最大值
        /// </summary>
        /// <returns></returns>
        protected virtual long GetAttributeMax(int attributeId)
        {
            switch (attributeId)
            {
                case AttributeIdConsts.Hp:
                    return GetAttributeValue(AttributeIdConsts.HpMax);
            }

            return long.MaxValue;
        }

        #endregion

        #region 引用component

        protected BattleActorCompAttribute m_compAttribute;

        #endregion
    }
}
