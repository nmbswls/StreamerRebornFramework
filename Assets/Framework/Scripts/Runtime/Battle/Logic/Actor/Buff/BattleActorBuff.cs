using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Battle.Actor
{
    public class FakeBuffEnv : IBuffViewEnv
    {
        public void PushProcess(BattleShowProcess process, Action onProcessEnd = null)
        {
            onProcessEnd?.Invoke();
        }
    }
    public interface IBuffViewEnv
    {
        /// <summary>
        /// 决定在哪里异步
        /// </summary>
        /// <param name="process"></param>
        /// <param name="onProcessEnd"></param>
        void PushProcess(BattleShowProcess process, Action onProcessEnd = null);
    }

    public interface IBattleActorBuffEnv
    {
        /// <summary>
        /// 获取持有者
        /// </summary>
        /// <returns></returns>
        IBattleActor GetOwner();

        /// <summary>
        /// 获取结算器
        /// </summary>
        /// <returns></returns>
        IBattleLogicResolver GetResolver();
    }


    public class BattleActorBuff : IBattleActorBuffTriggerEnv, IBattleActorBuffLastEffecrEnv
    {
        public BattleActorBuff(int buffId, IBattleActorBuffEnv env)
        {
            m_buffId = buffId;
            m_env = env;

            m_config = FakeBattleActorBuffConfig.CreateFake();

            foreach (var triggerConf in m_config.BuffTriggerConfigs)
            {
                // 注册监听器
                var trigger = new BattleActorBuffTrigger(triggerConf.TriggerId, this);
                m_triggerList.Add(trigger);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            
        }

        public uint InstanceId;

        public int BuffId
        {
            get { return m_buffId; }
        }
        protected int m_buffId;


        /// <summary>
        /// 获取buf拥有者
        /// </summary>
        /// <returns></returns>
        public IBattleActor GetOwner()
        {
            return m_env.GetOwner();
        }

        /// <summary>
        /// 添加或修改回调
        /// </summary>
        public void OnAddOrModified()
        {
            // 是否可重复触发
            foreach (var action in m_config.BuffActionConfigs)
            {
                ApplyBuffAction(action);
            }
            
            ApplyBuffLast();
        }

        #region 一次性效果 

        /// <summary>
        /// 结算一次性效果
        /// </summary>
        public void ApplyBuffAction(FakeBattleActorBuffActionConfig actionConfig)
        {
            Debug.Log("ApplyBuffAction");
            switch (actionConfig.ActionType)
            {
                case "Damage":
                {
                    m_env.GetResolver().CurrContext.AddEffectNode(new EffectNodeDamage());
                    
                }
                    break;
            }
            
        }
        #endregion

        #region 持续效果

        /// <summary>
        /// 施加持续效果
        /// </summary>
        public void ApplyBuffLast()
        {
            // 初始化
            if (m_lastEffectList.Count == 0)
            {
                // 创建属性变化器
                var attributeEffect = new BattleActorBuffLastCommon(m_config.AttributeModifers, this);
                m_lastEffectList.Add(attributeEffect);

                // 创建其他复杂效果
                foreach (var lastConf in m_config.BuffLastConfigs)
                {
                    var lastEffect = CreateBuffLastEffect(lastConf);
                    m_lastEffectList.Add(lastEffect);
                }
            }

            foreach (var lastEffect in m_lastEffectList)
            {
                lastEffect.OnStart();
            }
        }

        /// <summary>
        /// 内嵌工厂类
        /// </summary>
        /// <returns></returns>
        protected BattleActorBuffLastEffect CreateBuffLastEffect(FakeBattleActorBuffLastConfig config)
        {
            switch (config.LastType)
            {
                case 1:
                    return new BattleActorBuffLastHpRelate(this);
            }

            return null;
        }

        #endregion

        #region 触发

        /// <summary>
        /// 执行触发
        /// </summary>
        /// <param name="triggerType"></param>
        public virtual void OnTrigger(EnumBuffTriggerType triggerType, params object[] paramList)
        {
            // 处理触发
            foreach (var trigger in m_triggerList)
            {
                if (!trigger.CheckTrigger(triggerType, paramList))
                {
                    continue;
                }

                // 开启新效果

                var ctx = m_env.GetResolver().OpenResolveCtx(EnumTriggereSourceType.BuffTriggered);
                //trigger.TriggerActionList maybe addbuff or damage
                ctx.AddEffectNode(new EffectNodeAddBuff(){ SourceActorId = m_env.GetOwner().ActorId, TargetActorId = m_env.GetOwner().ActorId, BuffId = 400});
            }
        }

        #endregion

        #region  IBattleActorBuffLastEffecrEnv

        /// <summary>
        /// 获取唯一id
        /// </summary>
        /// <param name="lastEffectId"></param>
        /// <returns></returns>
        public ulong GetUniqueInstanceId(int lastEffectId)
        {
            ulong unique = InstanceId;
            unique <<= 32;
            unique = (unique | (uint) lastEffectId);
            return unique;
        }

        /// <summary>
        /// 更新属性装饰
        /// </summary>
        /// <param name="modifierId"></param>
        /// <param name="attributeId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateAttributeModifier(ulong modifierId, int attributeId, long value)
        {
            return GetOwner().UpdateAttributeModifier(modifierId, attributeId, value);
        }

        #endregion

        /// <summary>
        /// 层数改变事件
        /// </summary>
        public event Action<uint, int, int> EventOnLayerChange;


        /// <summary>
        /// 实例id
        /// </summary>
        protected uint m_instanceId;

        /// <summary>
        /// TODO 修改为正式资源
        /// </summary>
        protected FakeBattleActorBuffConfig m_config;

        /// <summary>
        /// 
        /// </summary>
        protected List<BattleActorBuffLastEffect> m_lastEffectList = new List<BattleActorBuffLastEffect>();
        protected List<BattleActorBuffTrigger> m_triggerList = new List<BattleActorBuffTrigger>();

        /// <summary>
        /// 当前buf的层数 对于普通buf是0层
        /// </summary>
        protected int m_currLayerNum;

        protected IBattleActorBuffEnv m_env;

        

    }
}
