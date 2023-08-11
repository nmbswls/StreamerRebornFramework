using My.Framework.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// env环境
    /// </summary>
    public interface IBattleMainEnv
    {
        /// <summary>
        /// 日志接口
        /// </summary>
        /// <returns></returns>
        ILogicLogger LogicLoggerGet();
    }

    public interface IBattleLogic : 
        IBattleMainCompFSM,
        IBattleLogicCompMain
    {

    }

    public partial class BattleLogic : IBattleLogic, IBattleLogicCompOwnerBase
    {

        public BattleLogic(IBattleMainEnv env, BattleInitInfoBase initInfo)
        {
            m_env = env;
            m_initInfo = initInfo;
        }

        public virtual bool Initialize()
        {
            if(!AllCompInitialize())
            {
                return false;
            }
            // 组件后初始化
            if (!PostInitAllComps())
            {
                return false;
            }

            RegEvent4Dispatch();

            return true;
        }

        /// <summary>
        /// 初始化所有组件
        /// </summary>
        /// <returns></returns>
        protected virtual bool AllCompInitialize()
        {
            m_compList.Clear();
            m_compNameDict.Clear();

            CompProcessManager = new BattleLogicCompProcessManager(this);
            m_compList.Add(CompProcessManager);

            CompResolver = new BattleLogicCompResolver(this);
            m_compList.Add(CompProcessManager);

            Recorder = new BattleLogicCompRecorder(this);
            m_compList.Add(CompProcessManager);

            Rebuilder = new BattleLogicCompRebuilder(this, null);
            m_compList.Add(CompProcessManager);

            foreach(var comp in m_compList)
            {
                m_compNameDict[comp.CompName] = comp;
                if(!comp.Initialize())
                {
                }
            }

            return true;
        }

        /// <summary>
        /// 后初始化所有组件
        /// </summary>
        protected bool PostInitAllComps()
        {
            foreach (var comp in m_compList)
            {
                if (!comp.PostInitialize())
                {
                    m_env.LogicLoggerGet().LogError(nameof(BattleLogic), nameof(PostInitAllComps),
                        $"Init Fail comp={comp}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Tick(float dt)
        {

        }

        /// <summary>
        /// 战斗开始
        /// </summary>
        public void BattleStart()
        {
            foreach (var comp in m_compList)
            {
                comp.BattleStart();
            }
        }


        /// <summary>
        /// 获取comp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T CompGet<T>(string compName) where T : BattleLogicCompBase
        {
            if(!m_compNameDict.TryGetValue(compName, out BattleLogicCompBase val))
            {
                return null;
            }

            return val as T;
        }

        public FakeBattleConfig ConfigGet()
        {
            return FakeBattleConfig;
        }


        #region 转发方法

        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        public BattleController GetBattleControllerById(int controllerId)
        {
            return CompMain.GetBattleControllerById(controllerId);
        }

        #endregion

        #region 基础组件

        public BattleLogicCompMain CompMain;

        public BattleLogicCompProcessManager CompProcessManager;

        public BattleLogicCompResolver CompResolver;

        public BattleLogicCompRecorder Recorder;

        public BattleLogicCompRebuilder Rebuilder;

        public BattleLogicCompRuler CompRuler;

        public BattleLogicCompFsm CompFSM;

        public BattleLogicCompActorContainer CompActorContainer;

        #endregion

        /// <summary>
        /// 组件列表
        /// </summary>
        protected List<BattleLogicCompBase> m_compList = new List<BattleLogicCompBase>();

        /// <summary>
        /// 组件名称索引
        /// </summary>
        protected Dictionary<string, BattleLogicCompBase> m_compNameDict = new Dictionary<string, BattleLogicCompBase>();

        /// <summary>
        /// 环境
        /// </summary>
        protected IBattleMainEnv m_env;

        /// <summary>
        /// 战斗初始化信息
        /// </summary>
        protected BattleInitInfoBase m_initInfo;

        /// <summary>
        /// 伪造的战斗配置
        /// </summary>
        public FakeBattleConfig FakeBattleConfig = FakeBattleConfig.GetFake();

        
    }
}
