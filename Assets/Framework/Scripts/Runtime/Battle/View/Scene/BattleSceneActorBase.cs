using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Battle.Actor;
using My.Framework.Runtime.Prefab;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// unity场景中actor基类
    /// </summary>
    public class BattleSceneActorBase : PrefabComponentBase
    {
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Initialize(BattleActor logicActor)
        {
            LogicActor = logicActor;

            // 执行绑定
            BindFields();

            // 命名点初始化
            NamedPointInit();
        }

        /// <summary>
        /// 设置坐标位置
        /// </summary>
        /// <param name="position"></param>
        /// <param name="worldSpace"></param>
        /// <param name="resetTailEffect"></param>
        public void SetActorPosition(Vector3 position, Boolean worldSpace = true, Boolean resetTailEffect = false)
        {
            if (worldSpace)
                transform.position = position;
            else
                transform.localPosition = position;

            if (resetTailEffect)
            {
                for (int i = 0; i < m_trailEffectHelperList.Count; ++i)
                {
                    m_trailEffectHelperList[i].ResetTrailEffect();
                }
            }
        }



        #region 装配树形结构

        /// <summary>
        /// 命名点初始化
        /// </summary>
        private void NamedPointInit()
        {
            for (int i = 0; i < m_namedPointRoot.childCount; i++)
            {
                var namedPoint = m_namedPointRoot.GetChild(i);
                m_namedPointDict.Add(namedPoint.name, namedPoint);
            }
        }

        #endregion

        /// <summary>
        /// 命名点缓存
        /// </summary>
        private readonly Dictionary<string, Transform> m_namedPointDict = new Dictionary<string, Transform>();

        private List<TrailEffectHelper> m_trailEffectHelperList = new List<TrailEffectHelper>();

        /// <summary>
        /// UI point根节点
        /// </summary>
        [AutoBind("./UIPoint")]
        protected Transform m_uiPoint;


        [AutoBind("./NamedPointRoot")]
        protected Transform m_namedPointRoot;

        /// <summary>
        /// 逻辑Actor对象
        /// </summary>
        public BattleActor LogicActor;
    }
}
