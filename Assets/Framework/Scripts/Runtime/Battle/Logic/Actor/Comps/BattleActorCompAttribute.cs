using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace My.Framework.Battle.Actor
{

    public class AttributeCalculater
    {
        public static long GetAttributeListValue(List<AttributeModifier> modifierList, AttributeCombineRule combineRule)
        {
            long result = 0;
            switch (combineRule)
            {
                case AttributeCombineRule.Add:
                {
                    modifierList.ForEach((elem) => { result += elem.Value;});
                }
                break;
                // 1-x的乘法逻辑 基本用于减伤
                case AttributeCombineRule.OneMinusMulti:
                {
                    result = 10000;
                    modifierList.ForEach((elem)=>
                    {
                        var value = elem.Value;
                        if (value <= 0 || value >= 10000)
                        {
                            return;
                        }
                        var dvalue = (double)(10000 - value) / (double)10000.0f;
                        result = (long)(result * dvalue);
                    });
                }
                break;
            }

            return result;
        }
    }



    /// <summary>
    /// 属性修改器
    /// </summary>
    public class AttributeModifier
    {
        /// <summary>
        /// 来源信息
        /// </summary>
        public int SourcrType;
        public int SourceParam;

        public ulong ModifierId;

        public long Value;
    }

    

    /// <summary>
    /// 属性上层环境
    /// </summary>
    public interface IBattleActorAttributeEnv
    {
        
    }

    public class BattleActorAttribute
    {

        public BattleActorAttribute(IBattleActorAttributeEnv env)
        {
            m_env = env;
        }

        public int AttributeId;
        public long BaseVal;
        public List<AttributeModifier> BuffModifiers = new List<AttributeModifier>();

        protected IBattleActorAttributeEnv m_env;


        /// <summary>
        /// 查找modifier
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public AttributeModifier GetModiferById(ulong instanceId)
        {
            var result = BuffModifiers.Find((x) => x.ModifierId == instanceId);
            return result;
        }

        public void AddBuffModifier(ulong instanceId)
        {
            var newAttribute = new AttributeModifier();
            BuffModifiers.Add(newAttribute);
        }

        public void RemoveBuffModifier(ulong instanceId)
        {

        }

        public void SetDirty(bool isDirty)
        {
            m_dirty = isDirty;
        }

        /// <summary>
        /// 获取dirty状态
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return m_dirty;
        }

        /// <summary>
        /// 重新计算缓存值
        /// </summary>
        public void RefreshCache()
        {
            AttributeCombineRule defaultRule = AttributeCombineRule.Add;
            m_buffCache = AttributeCalculater.GetAttributeListValue(BuffModifiers, defaultRule);

            long newValue = 0;
            bool checkMax = true;
            switch (defaultRule)
            {
                case AttributeCombineRule.Add:
                    {
                        newValue = BaseVal + m_buffCache;
                    }
                    break;
            }

            if (checkMax)
            {
                
            }

            // 可追溯属性 如攻击防御
            if (IsTraceable(AttributeId))
            {
                if (newValue != Value)
                {
                    long oldValue = Value;
                    Value = newValue;
                    EventOnAttributeChange?.Invoke(AttributeId, newValue, oldValue);
                }
            }
            // 不可追溯属性 如血量蓝量 处理后改变value并清空modifier
            else
            {
                BuffModifiers.Clear();
                EventOnAttributeAdd?.Invoke(AttributeId, newValue);
            }
            m_dirty = false;
        }

        protected bool m_dirty; // dirty

        /// <summary>
        /// buff带来的cache
        /// </summary>
        protected long m_buffCache;

        /// <summary>
        /// 最终值
        /// </summary>
        public long Value;

        #region 工具

        /// <summary>
        /// 是否可追溯
        /// </summary>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public bool IsTraceable(int attributeId)
        {
            bool bTraceable = true;
            return bTraceable;
        }

        #endregion

        /// <summary>
        /// 属性值改变事件 p1 属性id p2 新值 p3 旧值
        /// </summary>
        public Action<int, long, long> EventOnAttributeChange;

        /// <summary>
        /// 属性值变化事件 p1 属性id p2 改变量
        /// </summary>
        public Action<int, long> EventOnAttributeAdd;

    }
    public class BattleActorCompAttribute : BattleActorCompBase
    {
        public Dictionary<int, BattleActorAttribute> AttributeContainer = new Dictionary<int, BattleActorAttribute>();
    }
}
