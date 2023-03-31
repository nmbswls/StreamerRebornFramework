using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{

    public interface IBattleAudienceEnv
    {
        void OnPerkTrigger(BattleAudience audience, int perkInfo);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public class BattleAudience
    {
        
        public BattleAudience(uint instanceId, int configId, IBattleAudienceEnv env)
        {
            ConfigId = configId;
            InstanceId = instanceId;
            m_env = env;
        }

        public virtual bool Initialize()
        {
            CompAttribute = new BattleAudienceAttribute();
            CompAttribute.Initialize(this);

            return true;
        }

        public void AddSatisfaction(int val)
        {
            CompAttribute.Satisfaction += val;

            foreach (var perk in m_perkList)
            {
                perk.Trigger();
            }
            CheckAudienceSatisfy();
        }

        public void AddBuff()
        {
            // ��������perk

            foreach (var perk in m_perkList)
            {
                perk.Trigger();
            }
        }

        public void OnBorn()
        {
            {
                var newPerk = new BattleAudiencePerk(this);
                m_perkList.Add(newPerk);
            }

            foreach(var perk in m_perkList)
            {
                perk.Trigger();
            }
        }

        protected void CheckAudienceSatisfy()
        {
            if(CompAttribute.Satisfaction >= CompAttribute.MaxSatisfaction)
            {
                // �׳�����chain

            }
        }

        /// <summary>
        /// ����
        /// </summary>
        public void OnPerkTrigger(int confInfo)
        {
            m_env.OnPerkTrigger(this, confInfo);
        }

        #region ��������


        public virtual void UnInitialize()
        {

        }

        public virtual void Tick(float dt)
        {

        }

        public virtual object DumpToData()
        {
            return null;
        }

        #endregion

        #region ��Ա����

        public uint InstanceId;

        public int ConfigId;

        protected IBattleAudienceEnv m_env;


        public BattleAudienceAttribute CompAttribute;

        private List<BattleAudiencePerk> m_perkList = new List<BattleAudiencePerk>();

        #endregion
    }
}

