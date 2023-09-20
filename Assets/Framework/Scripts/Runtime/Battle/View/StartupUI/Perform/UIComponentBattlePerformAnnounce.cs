using My.Framework.Runtime.Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using My.ConfigData;
using My.Framework.Runtime.Resource;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    


    public class UIComponentBattlePerformAnnounce : UIComponentBase
    {
        public enum EnumAnnounceType
        {
            Buff,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiName"></param>
        public override void Initlize(string uiName)
        {
            base.Initlize(uiName);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            base.Tick(dt);
            foreach (var it in m_announceItemList)
            {
                it.Tick(dt);
            }
        }

        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
            //m_announceItemPrefab = SimpleResourceManager.Instance.LoadAssetSync<GameObject>("AnnounceItem");
            m_announceItemPrefab = transform.Find("ItemPrefab").gameObject;
        }

        #region 对外方法

        /// <summary>
        /// 显示触发宣告
        /// </summary>
        /// <param name="content"></param>
        public void ShowAnnounce(Vector3 originWorldPos, string content, Action callback = null)
        {
            UIComponentBattlePerformAnnounceItem announceItem;
            if (m_announcePool.Count > 0)
            {
                announceItem = m_announcePool.Dequeue();
                announceItem.gameObject.SetActive(true);
            }
            else
            {
                if (m_announceItemPrefab != null)
                {
                    GameObject go = GameObject.Instantiate(m_announceItemPrefab);
                    //PrefabControllerCreater.CreateAllControllers(go);
                    announceItem = go.AddComponent<UIComponentBattlePerformAnnounceItem>();
                    announceItem.Initialize();
                }
                else
                {
                    Debug.LogError("SkillAnnouncePrefab not exist");
                    return;
                }
            }

            announceItem.transform.SetParent(m_announceRoot, false);
            announceItem.transform.localScale = Vector3.one;
            announceItem.transform.SetAsLastSibling();
            announceItem.transform.position = originWorldPos;


            announceItem.EventOnShowEnd = callback;

            float duration = announceItem.Initialize(EnumAnnounceType.Buff, content);
            m_announceItemList.Add(announceItem);

            StartCoroutine(DoAnnounce(announceItem, duration));
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 准备回收
        /// </summary>
        /// <param name="announceItem"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator DoAnnounce(UIComponentBattlePerformAnnounceItem announceItem, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (announceItem.gameObject.activeSelf)
                announceItem.gameObject.SetActive(false);

            announceItem.EventOnShowEnd?.Invoke();

            m_announceItemList.Remove(announceItem);
            m_announcePool.Enqueue(announceItem);
        }

        #endregion

        #region 事件


        #endregion

        #region 绑定区域

        [AutoBind("./AnnounceRoot")]
        public Transform m_announceRoot;

        #endregion

        private GameObject m_announceItemPrefab;
        private readonly Queue<UIComponentBattlePerformAnnounceItem> m_announcePool = new Queue<UIComponentBattlePerformAnnounceItem>();

        protected readonly List<UIComponentBattlePerformAnnounceItem> m_announceItemList = new List<UIComponentBattlePerformAnnounceItem>();
    }


    /// <summary>
    /// 宣告Item
    /// </summary>
    public class UIComponentBattlePerformAnnounceItem : UIComponentBase
    {
        public override void Clear()
        {
            base.Clear();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
        }

        /// <summary>
        /// 初始化技能宣告UI信息和要跟随的角色
        /// </summary>
        public float Initialize(UIComponentBattlePerformAnnounce.EnumAnnounceType announceType, string showText)
        {
            float maxDuration = 0f;
            m_camera = gameObject.GetComponentInParent<Camera>();

            m_showText.text = showText;
            maxDuration = 0.5f;
            //maxDuration = m_commonUIState.GetMaxTweenDurationByUIState("Show");

            Vector3 viewport;
            if (!CheckIsInScreen(out viewport))
                return 0;
            return maxDuration;
        }

        protected void LateUpdate()
        {
            if (m_camera == null)
                return;
            Vector3 viewport;
            if (CheckIsInScreen(out viewport))
            {
                m_mainRoot.SetActive(true);
                viewport.z = gameObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>().anchoredPosition3D.z;
                gameObject.transform.position = m_camera.ViewportToWorldPoint(viewport);
            }
        }

        private Boolean CheckIsInScreen(out Vector3 viewport)
        {
            viewport = Vector3.zero;
            if (BindingTarget == null)
            {
                return true;
            }
            //var actorPos = ((BattleSceneActor)m_battleSceneActor).HudRoot.position + new Vector3(0.3f, 0.3f, 0);
            viewport = Camera.main.WorldToViewportPoint(BindingTarget.position);
            if (viewport.z < 0 || viewport.x < 0f || viewport.y < 0f || viewport.x > 1f || viewport.y > 1f)
            {
                m_mainRoot.SetActive(false);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 表现结束回调
        /// </summary>
        public Action EventOnShowEnd;

        private Transform BindingTarget;
        private Camera m_camera;

        #region Bind
        [AutoBind(".")]
        protected GameObject m_mainRoot;

        [AutoBind("./ShowText")] 
        protected Text m_showText;
        #endregion
    }
}



