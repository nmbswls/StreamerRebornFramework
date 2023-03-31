using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StreamerReborn
{

    public interface IBattleAudienceEnv
    {

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

        #region ��������

        public virtual bool Initialize()
        {
            return true;
        }

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

        #endregion

    }
}

