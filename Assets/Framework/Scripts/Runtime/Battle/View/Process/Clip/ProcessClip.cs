using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle.View
{
    /// <summary>
    /// 用于显示的片段
    /// </summary>
    public class ProcessClip
    {
        public void Init()
        {

        }
        public void UnInit()
        {
            OnUninit();
        }
        public void Start()
        {
            if (m_isStart)
            {
                return;
            }
            m_isStart = true;
            m_isEnd = false;
            OnStartClip();
        }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
        }

        public void End()
        {
            if (!m_isStart)
            {
                return;
            }
            m_isStart = false;
            m_isEnd = true;
            OnEndClip();
            if (ActionOnEnd != null)
            {
                Action<ProcessClip> temp = ActionOnEnd;
                ActionOnEnd = null;
                temp(this);
            }
        }


        protected virtual void OnUninit() { }
        protected virtual void OnStartClip() { }
        protected virtual void OnEndClip() { }
        protected virtual void OnUpdate(float deltaTime) { }


        public bool IsStart
        {
            get { return m_isStart; }
        }
        protected bool m_isStart;

        public bool IsEnd
        {
            get { return m_isEnd; }
        }
        protected bool m_isEnd;

        public virtual bool NeedStop { get { return false; } }

        /// <summary>
        /// 回调
        /// </summary>
        public event Action<ProcessClip> ActionOnEnd;
    }

    public class ActionProcessClip : ProcessClip
    {
        public override bool NeedStop
        {
            get { return true; }
        }
    }


    public class ActionProcessClipPrint : ActionProcessClip
    {
        #region Overrides of ProcessClip

        protected override void OnStartClip()
        {
            Debug.Log("I am print clip ");
        }

        #endregion
    }

    public class ProcessClipShow : ProcessClip
    {
        #region Overrides of ProcessClip

        protected override void OnStartClip()
        {
            UIControllerBattlePerform.Instance.ShowAnnounce(Vector3.zero, $"ActionProcessClipShow",
                () => { m_isFinished = true; });
        }

        public bool m_isFinished = false;
        public override bool NeedStop { get { return m_isFinished; } }

        #endregion
    }

    /// <summary>
    /// 并行process容器
    /// </summary>
    public class ParallelProcessClip : ProcessClip
    {

    }
}
