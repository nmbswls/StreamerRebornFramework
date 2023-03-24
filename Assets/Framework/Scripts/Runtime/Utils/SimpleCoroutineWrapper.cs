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
        /// tick����
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

                // ִ�е�ǰָ���corutine, ���ִ�й����п��ܻ�����µ�corutine����ӵ�m_corutineList��β��
                bool corutineExecResult = MoveNext(currCorutine.Value);
                // ����ִ�е������ʱ���п�������������µ�corutine����currCorutine.Next��ֵ�����仯��������Ҫrefreshһ��nextCorutine
                LinkedListNode<IEnumerator> nextCorutine = currCorutine.Next;
                // ��ִ����ɵ�corutine�Ƴ�
                if (!corutineExecResult)
                {
                    m_corutineList.Remove(currCorutine);
                }
                // ����currCorutine��������һ��ѭ��
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
        /// ֧��Ƕ�׵��õ�Э�̺��������㷨��
        /// </summary>
        /// <param name="iter">��ǰ֡Ҫ�����Э��</param>
        /// <returns></returns>
        protected bool MoveNext(IEnumerator iter)
        {
            if (iter == null)
            {
                return false;   // ��ָ�룬�ر�Э�̡�
            }

            if (iter.Current != null)
            {
                IEnumerator sub = iter.Current as IEnumerator;
                // ����ִ����Э��
                if (MoveNext(sub))
                {
                    return true;
                }
            }

            // Ȼ��ִ�и�Э��
            return iter.MoveNext();
        }

        /// <summary>
        /// �ȴ����ȵ�Я���б�
        /// </summary>
        private readonly LinkedList<IEnumerator> m_corutineList = new LinkedList<IEnumerator>();

        /// <summary>
        /// �Ƿ��Ѿ�cancel
        /// </summary>
        private bool m_cancel;
    }
}
