using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace My.Framework.Runtime
{
    public class SimpleCoroutineWrapper
    {
        public void StartCoroutine(Func<IEnumerator> corutine)
        {
            m_corutineList.AddLast(corutine());
        }
        public void StartCoroutine(IEnumerator corutine)
        {
            m_corutineList.AddLast(corutine);
        }

        public void CancelAll()
        {
            m_cancel = true;
        }

        /// <summary>
        /// tick驱动
        /// </summary>
        public bool Tick()
        {
            if (m_corutineList.Count == 0)
            {
                return false;
            }

            var currCorutine = m_corutineList.First;
            while (currCorutine != null)
            {
                if (m_cancel)
                {
                    break;
                }

                // 执行当前指向的corutine, 这个执行过程有可能会产生新的corutine并添加到m_corutineList的尾部
                bool corutineExecResult = MoveNext(currCorutine.Value);
                // 由于执行到这里的时候，有可能链表添加了新的corutine导致currCorutine.Next的值发生变化，所以需要refresh一下nextCorutine
                LinkedListNode<IEnumerator> nextCorutine = currCorutine.Next;
                // 将执行完成的corutine移除
                if (!corutineExecResult)
                {
                    m_corutineList.Remove(currCorutine);
                }
                // 更新currCorutine，继续下一次循环
                currCorutine = nextCorutine;
            }

            if (m_cancel)
            {
                m_corutineList.Clear();
                m_cancel = false;
            }

            return true;
        }

        /// <summary>
        /// 支持嵌套调用的协程函数调度算法。
        /// </summary>
        /// <param name="iter">当前帧要处理的协程</param>
        /// <returns></returns>
        protected bool MoveNext(IEnumerator iter)
        {
            if (iter == null)
            {
                return false;   // 空指针，关闭协程。
            }

            if (iter.Current != null)
            {
                IEnumerator sub = iter.Current as IEnumerator;
                // 优先执行子协程
                if (MoveNext(sub))
                {
                    return true;
                }
            }

            // 然后执行父协程
            return iter.MoveNext();
        }

        /// <summary>
        /// 等待调度的携程列表
        /// </summary>
        private readonly LinkedList<IEnumerator> m_corutineList = new LinkedList<IEnumerator>();

        /// <summary>
        /// 是否已经cancel
        /// </summary>
        private bool m_cancel;
    }
}
