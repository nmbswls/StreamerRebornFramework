using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using UnityEngine;

namespace My.Framework.Battle.Logic
{

    public interface IBattleLogicCompActorContainer
    {
        /// <summary>
        /// 新建actor
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="actorType"></param>
        /// <param name="actorId"></param>
        /// <returns></returns>
        BattleActor CreateActor(int actorType, int configId, BattleActorSourceType sourceType, int campId, uint actorId = 0);

        /// <summary>
        /// 获取指定actor
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        BattleActor GetActor(uint actorId);

        /// <summary>
        /// 获取某一阵营所有actor
        /// </summary>
        /// <param name="campId"></param>
        /// <returns></returns>
        IEnumerable<BattleActor> GetActorsByCamp(int campId);
    }


    public class BattleLogicCompActorContainer : BattleLogicCompBase, IBattleLogicCompActorContainer, IBattleActorEnvBase, IBattleActorEventListener
    {

        public BattleLogicCompActorContainer(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName
        {
            get { return GamePlayerCompNames.ActorManager; }
        }


        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!base.Initialize())
            {
                return false;
            }

            m_compMain = m_owner.CompGet<BattleLogicCompMain>(GamePlayerCompNames.Main);
            m_compProcessManager = m_owner.CompGet<BattleLogicCompProcessManager>(GamePlayerCompNames.ProcessManager);
            return true;
        }

        /// <summary>
        /// 后期初始化
        /// </summary>
        /// <returns></returns>
        public override bool PostInitialize()
        {
            if (!base.PostInitialize())
            {
                return false;
            }

            m_compMain.EventOnBattleStateStarting += OnBattleStarting;

            // 构造角色
            //foreach (var setup in sceneConfInfo.CharActorSetups)
            //{
            //    if (IsCharActorNeedCreateInInit(setup) && !IsDeadJudgeByHpInherit(setup))
            //    {
            //        if (CreateCharActor(BattleActorSourceType.Init, setup) == null)
            //        {
            //            DebugUtility.LogError(string.Format("CreateCharActor Fail Setup X:{0}, Y:{1}, ActorId:{2}",
            //                setup.X, setup.Y, setup.ActorId));
            //            return false;
            //        }
            //    }
            //}
            CreateActor(BattleActorTypeStartup.Player, 0, BattleActorSourceType.Init, 1);
            CreateActor(BattleActorTypeStartup.Enemy, 100, BattleActorSourceType.Init, 2);

            return true;
        }

        /// <summary>
        /// 战斗状态切换 - 战前表现
        /// </summary>
        protected void OnBattleStarting()
        {
            foreach (var battleActor in m_battleActorList)
            {
                battleActor.OnTrigger(EnumBuffTriggerType.BattleStart);
            }
        }

        #endregion

        /// <summary>
        /// 新建actor
        /// </summary>
        /// <param name="actorType"></param>
        /// <param name="configId"></param>
        /// <param name="sourceType"></param>
        /// <param name="campId"></param>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public BattleActor CreateActor(int actorType, int configId, BattleActorSourceType sourceType, int campId, uint actorId = 0)
        {
            // 检查actorId 是否会被占用
            if (actorId != 0 && GetActor(actorId) != null)
            {
                Debug.LogWarning(string.Format("CreateActor Fail actorId:{0} already exist", actorId));
                return null;
            }

            var actor = CreateActorInternal(actorType, configId, sourceType, null, actorId);

            if (actor == null)
            {
                return null;
            }
            m_battleActorList.Add(actor);

            // 加入阵营
            if (!m_campActorDict.TryGetValue(campId, out var listVal))
            {
                listVal = new List<BattleActor>();
                m_campActorDict[campId] = listVal;
            }
            listVal.Add(actor);
            
            actor.CompBasic.CampId = campId;
            return actor;
        }


        /// <summary>
        /// 获取指定actor
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public BattleActor GetActor(uint actorId)
        {
            foreach (var actor in m_battleActorList)
            {
                if (actor.ActorId == actorId)
                {
                    return actor;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取指定actor
        /// </summary>
        /// <param name="campId"></param>
        /// <returns></returns>
        public IEnumerable<BattleActor> GetActorsByCamp(int campId)
        {
            foreach (var actor in m_battleActorList)
            {
                if (actor.CompBasic.CampId == campId)
                {
                    yield return actor;
                }
            }
        }

        #region env方法

        /// <summary>
        /// 推送process
        /// </summary>
        /// <param name="process"></param>
        public void PushProcessAndFlush(BattleShowProcess process)
        {
            m_compProcessManager.PushProcessToCache(process);
            m_compProcessManager.FlushAndRaiseEvent();
        }

        /// <summary>
        /// 获取结算器
        /// </summary>
        /// <returns></returns>
        public IBattleLogicResolver GetResolver()
        {
            return m_compResolver;
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 创建角色对象
        /// </summary>
        /// <returns></returns>
        protected virtual BattleActor CreateActorInternal(int actorType, int actorConfigId, BattleActorSourceType sourceType, object actorInitInfo, uint actorId = 0)
        {
            // 分配id
            actorId = actorId != 0 ? actorId : AllocActorId();

            if (GetActor(actorId) != null)
            {
                throw new Exception(string.Format("actorId:{0} is already exist", actorId));
            }

            // 构造actor对象
            BattleActor actor = null;
            switch (actorType)
            {
                case BattleActorTypeStartup.Player:
                {
                    actor = new BattleActorPlayer(this);
                }
                    break;
                case BattleActorTypeStartup.Enemy:
                {
                    actor = new BattleActorPrisoner(this);
                }
                    break;
                default:
                    Debug.LogError($"CreateCharActorInternal actor type err {actorType}");
                    return null;
            }
                
            // 初始化Actor
            if (!actor.Init(actorId, sourceType, actorInitInfo))
            {
                Debug.LogError("CreateCharActorInternal actor init fail");
                throw new Exception("CreateCharActorInternal actor init fail");
            }
            
            return actor;
        }

        /// <summary>
        /// 分配对象id
        /// </summary>
        /// <returns></returns>
        protected uint AllocActorId()
        {
            s_actorIdSeq++;
            if (s_actorIdSeq == 0)
            {
                s_actorIdSeq++;
            }
            return s_actorIdSeq;
        }

        #endregion

        public static uint s_actorIdSeq = 0;

        /// <summary>
        /// 容器
        /// </summary>
        public List<BattleActor> m_battleActorList = new List<BattleActor>();

        /// <summary>
        /// 阵营 - actor存储映射
        /// </summary>
        protected Dictionary<int, List<BattleActor>> m_campActorDict = new Dictionary<int, List<BattleActor>>();

        /// <summary>
        /// process处理
        /// </summary>
        protected BattleLogicCompProcessManager m_compProcessManager;

        /// <summary>
        /// Resolver
        /// </summary>
        protected BattleLogicCompResolver m_compResolver;

        /// <summary>
        /// 主流程
        /// </summary>
        protected BattleLogicCompMain m_compMain;


        #region 事件监听

        public void OnAddBuff(BattleActor actor, BattleActorBuff buff)
        {
        }

        public void OnCauseDamage(uint targetId, uint sourceId, long dmg, long realDmg)
        {
        }

        #endregion

    }

    /// <summary>
    /// 战斗角色来源类型
    /// </summary>
    public enum BattleActorSourceType
    {
        /// <summary>
        /// 战斗初始化
        /// </summary>
        Init,

        /// <summary>
        /// 事件创建(召唤)
        /// </summary>
        Event,

        /// <summary>
        /// 技能召唤
        /// </summary>
        Summon
    }
}
