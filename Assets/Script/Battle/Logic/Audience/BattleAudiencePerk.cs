using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class BattleAudiencePerk
    {
        BattleAudience Owner;

        public int ConfInfo;

        public BattleAudiencePerk(BattleAudience owner)
        {
            this.Owner = owner;
        }

        public virtual bool Trigger()
        {
            if (!CanBeTrigger())
            {
                return false;
            }

            // ��������
            //PushContextState(this);

            EventOnPreTrigger?.Invoke(this);

            // ������Ч����
            IncreaseTirggerTimes();

            // ��ʼ��ȴ
            StartCoolDown();

            // ִ��Ч��
            DoTriggerAction();


            EventOnPostTrigger?.Invoke(this);

            // �˳�����
            //PushContextState(this);

            return true;
        }

        public virtual bool CanBeTrigger()
        {
            return false;
        }

        /// <summary>
        /// buff���� 
        /// </summary>
        protected void OnTrigger()
        {
        }

        /// <summary>
        /// CD
        /// </summary>
        public void StartCoolDown()
        {
            
        }

        /// <summary>
        /// ���Ӵ�������
        /// </summary>
        public void IncreaseTirggerTimes()
        {
            m_triggeredTimes++;
            m_triggeredTimesOfCurTurn++;
        }


        /// <summary>
        /// ִ�д�����Ϊ
        /// </summary>
        protected virtual void DoTriggerAction()
        {
            Owner.OnPerkTrigger(ConfInfo);
        }

        /// <summary>
        /// �������� ���� ����
        /// </summary>
        protected int m_confInfo;

        /// <summary>
        /// ����������
        /// </summary>
        protected int m_triggeredTimes;
        protected int m_triggeredTimesOfCurTurn;

        #region �¼�

        public event Action<BattleAudiencePerk> EventOnPreTrigger;

        public event Action<BattleAudiencePerk> EventOnPostTrigger;

        #endregion

        public bool m_isPassiveSkill;
        public int m_sourceSkillId;
    }
}

