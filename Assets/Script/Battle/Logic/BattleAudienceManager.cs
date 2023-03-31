using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn
{
    public class BattleAudienceManager : IBattleAudienceEnv
    {
        public BattleManager Owner;
        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="dt"></param>
        public void Tick(float dt)
        {
            foreach (var actor in AudienceContainer.Values)
            {
                actor.Tick(dt);
            }

            foreach (var id2Remove in m_actorToRemove)
            {
                var actor = AudienceContainer[id2Remove];
                actor.UnInitialize();
            }
        }


        #region ��ɫ���

        /// <summary>
        /// 创建观众
        /// </summary>
        /// <returns></returns>
        public BattleAudience CreateBattleAudience(int configId, uint allocatedId = 0)
        {
            BattleAudience newAudience = CreateAudienceInternal(configId, 0, null);

            EventOnAddAudience?.Invoke(newAudience);
            return newAudience;
        }


        /// <summary>
        /// 下一帧移除actor
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public bool RemoveSceneActor(uint actorId)
        {
            if (!m_actorToRemove.Contains(actorId))
            {
                m_actorToRemove.Add(actorId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 创建观众
        /// </summary>
        /// <returns></returns>
        protected BattleAudience CreateAudienceInternal(int configId, uint allcatedId = 0, object initData = null)
        {
            BattleAudience newAudience = null;
            allcatedId = allcatedId != 0 ? allcatedId : AllocActorId();

            newAudience = new BattleAudience(allcatedId, configId, this);

            if (newAudience == null || !newAudience.Initialize())
            {
                return null;
            }

            AudienceContainer.Add(newAudience.InstanceId, newAudience);
            newAudience.Initialize();
            newAudience.OnBorn();

            return newAudience;
        }

        protected uint AllocActorId()
        {
            // 分配id
            while (m_currentInstId == 0 || AudienceContainer.ContainsKey(m_currentInstId))
            {
                m_currentInstId = m_currentInstId + 1;
            }
            return m_currentInstId;
        }

        #endregion


        
        /// <summary>
        /// 等待移除列表
        /// </summary>
        private List<uint> m_actorToRemove = new List<uint>();

        #region ？


        /// <summary>
        /// 添加观众事件
        /// </summary>
        public event Action<BattleAudience> EventOnAddAudience;

        /// <summary>
        /// 移除观众事件
        /// </summary>
        public event Action<BattleAudience> EventOnRemoveAudience;

        #endregion

        #region 内部变量

        public Dictionary<uint, BattleAudience> AudienceContainer = new Dictionary<uint, BattleAudience>();

        
        protected uint m_currentInstId;

        #endregion
    }
}

