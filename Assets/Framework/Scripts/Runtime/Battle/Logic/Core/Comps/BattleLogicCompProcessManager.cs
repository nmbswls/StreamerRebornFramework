using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Logic
{
    /// <summary>
    /// process管理器
    /// </summary>
    public interface IBattleLogicCompProcessManager
    {
        /// <summary>
        /// 推process
        /// </summary>
        /// <param name="process"></param>
        /// <returns>true 表示需要等待 false表示无需等待</returns>
        bool PushProcessToCache(BattleShowProcess process);

        /// <summary>
        /// 推送process
        /// </summary>
        void FlushAndRaiseEvent();
    }

    public class BattleLogicCompProcessManager : BattleLogicCompBase, IBattleLogicCompProcessManager
    {
        public BattleLogicCompProcessManager(IBattleLogicCompOwnerBase owner) : base(owner)
        {
        }

        public override string CompName { get { return GamePlayerCompNames.ProcessManager; } }

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

            return true;
        }


        public override void Tick(float dt)
        {
            // 移除播放完毕的process
            for (int i = m_playingProcessList.Count - 1; i >= 0; i--)
            {
                if (m_playingProcessList[i].IsEnd)
                {
                    m_playingProcessList.RemoveAt(i);
                }
            }
        }


        #region IBattleLogicCompProcessManager 实现

        /// <summary>
        /// 推process
        /// </summary>
        /// <param name="process"></param>
        /// <returns>true 表示需要等待 false表示无需等待</returns>
        public bool PushProcessToCache(BattleShowProcess process)
        {
            m_processCacheList.Add(process);
            return true;
        }

        /// <summary>
        /// 清空process队列抛出事件
        /// </summary>
        public void FlushAndRaiseEvent()
        {
            if (m_processCacheList.Count > 0)
            {
                if (EventOnProcessFlush != null)
                {
                    foreach (var process in m_processCacheList)
                    {
                        m_playingProcessList.Add(process);
                    }
                    EventOnProcessFlush(m_processCacheList);
                    m_processCacheList.Clear();
                }
            }
        }


        #endregion

        #region 组件间方法

        /// <summary>
        /// 判断是否全部结束
        /// </summary>
        /// <returns></returns>
        public bool IsPlayingProcess()
        {
            return m_playingProcessList.Count == 0;
        }

        #endregion

        #region 工厂类


        #endregion

        /// <summary>
        /// 抛出表现层
        /// </summary>
        public event Action<List<BattleShowProcess>> EventOnProcessFlush;

        /// <summary>
        /// process列表
        /// </summary>
        protected List<BattleShowProcess> m_processCacheList = new List<BattleShowProcess>();

        /// <summary>
        /// 正在播放的process列表
        /// </summary>
        protected List<BattleShowProcess> m_playingProcessList = new List<BattleShowProcess>();

        
    }
}
