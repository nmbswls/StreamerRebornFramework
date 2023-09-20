using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.View
{
    /// <summary>
    /// 基础顺序process clip
    /// </summary>
    public class SequenceProcessClip : ProcessClip
    {
        private List<ProcessClip> m_subProcessList = new List<ProcessClip>();
        private ProcessClip m_curProcess;

        /// <summary>
        /// 添加子片段
        /// </summary>
        /// <param name="clip"></param>
        public void Add(ProcessClip clip)
        {
            if (clip == null)
            {
                return;
            }
            m_subProcessList.Add(clip);
        }

        protected override void OnStartClip()
        {
            ProcessNext();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (m_curProcess != null)
            {
                m_curProcess.Update(deltaTime);
                if (m_curProcess.NeedStop)
                {
                    m_curProcess.End();
                }
            }

        }

        public void ProcessNext()
        {
            m_curProcess = null;
            while (m_subProcessList.Count > 0)
            {
                ProcessClip process = m_subProcessList[0];
                m_subProcessList.RemoveAt(0);
                process.Start();
                if (process.IsStart)
                {
                    m_curProcess = process;
                    m_curProcess.ActionOnEnd += OnSubProcessClipEnd;
                    break;
                }
                else
                {
                    process.UnInit();
                }
            }

            CheckEnd();

        }

        /// <summary>
        /// 检查当前片段结束
        /// </summary>
        protected void CheckEnd()
        {
            if (m_subProcessList.Count == 0 && m_curProcess == null && IsStart)
            {
                End();
            }
        }

        /// <summary>
        /// 单个clip 结束
        /// </summary>
        /// <param name="clip"></param>
        protected void OnSubProcessClipEnd(ProcessClip clip)
        {
            if (m_curProcess != clip)
            {
                return;
            }

            clip.UnInit();
            ProcessNext();
        }
    }
}
