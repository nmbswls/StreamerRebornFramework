using My.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Battle.Logic;
using UnityEngine;

namespace My.Framework.Battle.Actor
{

    public interface IBattleActorEnvBase
    {
        /// <summary>
        /// 推送process
        /// </summary>
        /// <returns></returns>
        void PushProcessAndFlush(BattleShowProcess process);

        /// <summary>
        /// 获取当前结算现场
        /// </summary>
        /// <returns></returns>
        BattleResolveContext GetResolveContext();
    }

    public interface IBattleActorCompProvider
    {
        BattleActorCompBasic BattleActorCompBasicGet();
        BattleActorCompAttribute BattleActorCompAttributeGet();
        BattleActorCompBuff BattleActorCompBuffGet();
        BattleActorCompSkill BattleActorCompSkillGet();
        BattleActorCompHpState BattleActorCompHpStateGet();
    }

    public interface IBattleActorHandlerProvider
    {
        BattleActorHandlerBuff BattleActorHandlerBuffGet();
        BattleActorHandlerAttribute BattleActorHandlerAttributeGet();

        BattleActorHandlerSkill BattleActorHandlerSkillGet();
    }

    public interface IBattleActor:
        IBattleActorHandlerHpState, 
        IBattleActorHandlerAttribute,
        IBattleActorHandlerSkill,
        IBattleActorHandlerBuff
    {

    }

    public partial class BattleActor : IBattleActor, IBattleActorCompProvider, IBattleActorOperatorEnv
    {
        protected BattleActor(IBattleActorEnvBase env)
        {
            m_env = env;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="attributePreset"></param>
        /// <returns></returns>
        public virtual bool Init(uint actorId, BattleActorSourceType sourceType, object initInfo, Dictionary<int, long> attributePreset = null)
        {
            m_actorId = actorId;
            m_sourceType = sourceType;

            // 创建组件
            CreateComps();
            // 创建service
            CreateHandlers();

            RegEvent4Fire();

            // 初始化
            if (!OnInit()) return false;

            // 后初始化
            if (!OnPostInit()) return false;

            RegEvent4Fire();

            return true;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        protected bool OnInit()
        {
            if (!InitHandlers())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 后置初始化
        /// </summary>
        public bool OnPostInit()
        {
            // 后置初始化
            if (!PostInitOperators())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建component
        /// </summary>
        protected virtual void CreateComps()
        {
            CompBasic = new BattleActorCompBasic();
            CompAttribute = new BattleActorCompAttribute();
            CompSkill = new BattleActorCompSkill();
            CompBuff = new BattleActorCompBuff();
            CompHpState = new BattleActorCompHpState();

            m_componentList.Add(CompBasic);
            m_componentList.Add(CompAttribute);
            m_componentList.Add(CompSkill);
            m_componentList.Add(CompBuff);
            m_componentList.Add(CompHpState);
            
        }

        /// <summary>
        /// 创建service
        /// </summary>
        protected virtual void CreateHandlers()
        {
            m_handlerAttribute = new BattleActorHandlerAttribute(this);

            m_handlerList.Add(m_handlerAttribute);
        }


        /// <summary>
        /// 初始化Comp
        /// </summary>
        /// <returns></returns>
        protected bool InitHandlers()
        {
            foreach (var handler in m_handlerList)
            {
                if (handler != null && !handler.Initialize())
                {
                    throw new Exception(handler + "init fail");
                }
            }

            return true;
        }

        /// <summary>
        /// 后初始化Operators
        /// </summary>
        /// <returns></returns>
        protected bool PostInitOperators()
        {
            foreach (var handler in m_handlerList)
            {
                if (handler != null && !handler.PostInitialize())
                {
                    throw new Exception(handler + "post init fail");
                }
            }

            return true;
        }

        #region 外部事件

        public void OnBattleStart()
        {
            var logicEvent = new LogicEventDefault(EventIDConsts.BattleStart);
            m_pipe.PushEvent(logicEvent);
        }

        public void OnTurnStart()
        {

        }

        public void OnTurnEnd()
        {

        }

        #endregion

        #region 逻辑转发

        

        #endregion


        #region Component

        /// <summary>
        /// component列表
        /// </summary>
        protected List<BattleActorCompBase> m_componentList = new List<BattleActorCompBase>();

        public BattleActorCompBasic CompBasic;
        public BattleActorCompAttribute CompAttribute;
        public BattleActorCompBuff CompBuff;
        public BattleActorCompSkill CompSkill;
        public BattleActorCompHpState CompHpState;

        BattleActorCompBasic IBattleActorCompProvider.BattleActorCompBasicGet()
        {
            return CompBasic;
        }

        BattleActorCompAttribute IBattleActorCompProvider.BattleActorCompAttributeGet()
        {
            return CompAttribute;
        }

        public BattleActorCompBuff BattleActorCompBuffGet()
        {
            return CompBuff;
        }

        public BattleActorCompSkill BattleActorCompSkillGet()
        {
            return CompSkill;
        }

        public BattleActorCompHpState BattleActorCompHpStateGet()
        {
            return CompHpState;
        }

        #endregion

        #region IBattleActorOperatorEnv 实现

        public void PushProcessAndFlush(BattleShowProcess process)
        {
            m_env.PushProcessAndFlush(process);
        }

        public uint GetActorId()
        {
            return m_actorId;
        }

        #endregion

        #region 获取
        public BattleActorHandlerBuff BattleActorHandlerBuffGet()
        {
            return m_handlerBuff;
        }

        public BattleActorHandlerAttribute BattleActorHandlerAttributeGet()
        {
            return m_handlerAttribute;
        }

        public BattleActorHandlerSkill BattleActorHandlerSkillGet()
        {
            return m_handlerSkill;
        }

        #endregion

        #region Opperator

        /// <summary>
        /// operator列表
        /// 由于不是严格的ecs 命名不适用service
        /// </summary>
        protected List<BattleActorHandlerBase> m_handlerList = new List<BattleActorHandlerBase>();

        protected BattleActorHandlerAttribute m_handlerAttribute;
        protected BattleActorHandlerBuff m_handlerBuff;
        protected BattleActorHandlerSkill m_handlerSkill;
        protected BattleActorHandlerHpState m_handlerHpState;

        #endregion

        #region 内部变量

        /// <summary>
        /// actorId
        /// </summary>
        protected uint m_actorId;
        public uint ActorId
        {
            get { return m_actorId; }
        }

        /// <summary>
        /// 来源
        /// </summary>
        protected BattleActorSourceType m_sourceType;

        /// <summary>
        /// 上层环境
        /// </summary>
        public IBattleActorEnvBase m_env;

        #endregion

        #region 逻辑转发

        public void ApplyDamage(long damage)
        {
            m_handlerHpState.ApplyDamage(damage);
        }

        public void ApplyHpStateModify()
        {
            m_handlerHpState.ApplyHpStateModify();
        }

        public bool UpdateAttributeModifier(ulong modifierId, int attributeId, long value, bool setExist = true)
        {
            return m_handlerAttribute.UpdateAttributeModifier(modifierId, attributeId, value, setExist);
        }

        #endregion

        public bool CanUseSkill(int skillId)
        {
            return m_handlerSkill.CanUseSkill(skillId);
        }

        public bool UseSkill(int skillId)
        {
            return m_handlerSkill.UseSkill(skillId);
        }

        #region Implementation of IBattleActorHandlerBuff

        /// <summary>
        /// 增加buf
        /// </summary>
        public void AddBuff(int buffId, int layer = 1)
        {
            m_handlerBuff.AddBuff(buffId, layer);
        }

        #endregion
    }
}
