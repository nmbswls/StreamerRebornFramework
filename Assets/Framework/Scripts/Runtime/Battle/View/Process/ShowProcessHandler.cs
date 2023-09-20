using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Logic;

namespace My.Framework.Battle.View
{

    public class ProcessBunch
    {
        public List<BattleShowProcess> m_processList;
        public bool AllFinished = false;

        public void SendAllProcessEnd()
        {
            if (m_processList != null)
            {
                for (int i = 0; i < m_processList.Count; ++i)
                {
                    m_processList[i].IsEnd = true;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ShowProcessHandler
    {
        /// <summary>
        /// 对外方法 推入process并立即触发一次更新
        /// </summary>
        /// <param name="process"></param>
        public void InputProcess(BattleShowProcess process)
        {
            // 否则加入缓存队列 等待推送
            m_cachedProcessList.Add(process);
        }

        public void Tick(float dt)
        {
            // 更新process
            TryFlushProcess();

            // 更新bunch
            TickProcessBunch();

            // 更新播放clip
            TickProcessClip(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void TickProcessBunch()
        {
            if (m_currBunch == null) return;

            // 等待播放
            if (!m_currBunch.AllFinished)
            {
                return;
            }

            // 释放
            m_currBunch.SendAllProcessEnd();
            m_currBunch = null;

            HandleNextBunch();
        }


        /// <summary>
        /// 更新当前clip播放
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void TickProcessClip(float dt)
        {
            if (m_curClip != null)
            {
                if (m_curClip.NeedStop)
                {
                    m_curClip.End();
                }
                m_curClip.Update(dt);
            }
        }

        
        /// <summary>
        /// bunch process
        /// </summary>
        protected void TryFlushProcess()
        {
            int index = 0;
            m_tempProcessList.Clear();

            // 将当前缓存的process 以bar为分界点 分批打包
            while (index < m_cachedProcessList.Count)
            {
                var process = m_cachedProcessList[index];
                // 如果process intent类型为bar 直接结束播放
                if (process.Type == ProcessTypes.Bar)
                {
                    BunchProcess(m_tempProcessList);
                    m_tempProcessList.Clear();
                    m_cachedProcessList.RemoveRange(0, index + 1);
                    index = 0;
                    continue;
                }

                m_tempProcessList.Add(process);
                index += 1;
            }

            
        }

        /// <summary>
        /// process批结束，开始表现
        /// </summary>
        public void BunchProcess(List<BattleShowProcess> processList)
        {
            if (processList.Count == 0)
            {
                return;
            }

            var newBunch = new ProcessBunch();
            newBunch.m_processList = new List<BattleShowProcess>(processList);

            StartBunch(newBunch);
        }

        /// <summary>
        /// 处理一个bunch
        /// </summary>
        /// <param name="bunch"></param>
        public void StartBunch(ProcessBunch bunch)
        {
            if (bunch == null)
            {
                return;
            }
            m_bunchList.Add(bunch);

            if (m_currBunch == null && !m_isPaused)
            {
                HandleNextBunch();
            }
        }

        /// <summary>
        /// 处理下一个bundle
        /// </summary>
        public void HandleNextBunch()
        {
            if (m_isPaused)
            {
                return;
            }

            if (m_bunchList.Count == 0)
            {
                return;
            }

            var bunch = m_bunchList[0];
            m_bunchList.RemoveAt(0);

            m_currBunch = bunch;
            StartClip(CreateClipFromProcessBunch(bunch));
        }

        #region 处理process clip

        /// <summary>
        /// 开始播放clip
        /// </summary>
        /// <param name="clip"></param>
        public void StartClip(ProcessClip clip)
        {
            if (clip == null)
            {
                return;
            }
            m_clipList.Add(clip);
            if (!IsClipStart && !m_isPaused)
            {
                ProcessNextClip();
            }
        }

        /// <summary>
        /// 处理下个clip
        /// </summary>
        public void ProcessNextClip()
        {
            m_curClip = null;
            if (m_clipList.Count > 0)
            {
                while (m_clipList.Count > 0 && !m_isPaused)
                {
                    ProcessClip process = m_clipList[0];
                    m_clipList.RemoveAt(0);
                    process.Init();
                    process.Start();
                    if (process.IsStart)
                    {
                        m_curClip = process;
                        m_curClip.ActionOnEnd += OnProcessClipEnd;
                        break;
                    }
                    else
                    {
                        process.UnInit();
                    }
                }
            }
        }

        /// <summary>
        /// clip结束回调
        /// </summary>
        /// <param name="clip"></param>
        private void OnProcessClipEnd(ProcessClip clip)
        {
            clip.UnInit();
            if (m_curClip != clip)
            {
                UnityEngine.Debug.LogWarning("Cannot End ProcessClip which is not Current Process Clip. Please Remove it or Skip it:");
                return;
            }
            ProcessNextClip();
        }

        #endregion

        #region 工具方法


        /// <summary>
        /// 通过process数据创建新片段
        /// </summary>
        /// <returns></returns>
        protected ProcessClip CreateClipFromProcessBunch(ProcessBunch bunch)
        {
            var mainProcess = new SequenceProcessClip();

            if (bunch.m_processList.Count == 1)
            {
                // todo temp
                ProcessClip clip;
                switch (bunch.m_processList[0].Type)
                {
                    case ProcessTypes.Print:
                    {
                        clip = new ActionProcessClipPrint();
                        break;
                    }
                    case ProcessTypes.Show:
                    {
                        clip = new ProcessClipShow();
                        break;
                    }
                    default:
                        clip = new ActionProcessClip();
                        break;
                }
                mainProcess.Add(clip);
            }
            else
            {
                foreach (var process in bunch.m_processList)
                {
                    // todo temp
                    ProcessClip clip;
                    switch (process.Type)
                    {
                        case ProcessTypes.Print:
                        {
                            clip = new ActionProcessClipPrint();
                            break;
                        }
                        case ProcessTypes.Show:
                        {
                            clip = new ProcessClipShow();
                            break;
                        }
                        default:
                            clip = new ActionProcessClip();
                            break;
                    }
                    mainProcess.Add(clip);
                }
            }

            mainProcess.ActionOnEnd += (clip) => { bunch.AllFinished = true; };

            return mainProcess;
        }


        #endregion


        private bool m_isPaused;

        private ProcessBunch m_currBunch;
        private List<ProcessBunch> m_bunchList = new List<ProcessBunch>();
        private List<BattleShowProcess> m_cachedProcessList = new List<BattleShowProcess>();

        /// <summary>
        /// 临时缓存
        /// </summary>
        private List<BattleShowProcess> m_tempProcessList = new List<BattleShowProcess>();

        public bool IsClipStart
        {
            get { return m_curClip != null; }
        }
        private ProcessClip m_curClip;
        private List<ProcessClip> m_clipList = new List<ProcessClip>();

        /// <summary>
        /// 当前战斗逻辑对象
        /// </summary>
        public BattleLogic BattleLogic;
    }
}
