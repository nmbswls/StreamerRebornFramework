
using My.Runtime;
using System;
using UnityEngine;

namespace StreamerReborn.World
{
    public class ActorControllerCharacter : ActorControllerBase
    {
        public override bool Initialize(int configId, SceneActor logicActor)
        {
            if (!base.Initialize(configId, logicActor))
            {
                return false;
            }

            // 预览/演出等情况 不与逻辑对象绑定
            if(logicActor == null)
            {
                return true;
            }

            return true;
        }

        protected override void Update()
        {
            if (m_waitTimer > 0)
            {
                m_waitTimer -= Time.deltaTime;
                return;
            }
        }

        //protected SceneCharacterActor CharacterActor
        //{
        //    get { return (TempSceneCharacterActor)m_logicActor; }
        //}

        public Vector2 NextWanderTarget;

        private float m_waitTimer = 0;

        public override bool OnClick()
        {
            Debug.LogError("click event on " + gameObject.name);
            return true;
        }

        #region 事件

        #endregion
    }
}

