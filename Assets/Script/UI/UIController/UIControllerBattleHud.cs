using My.Framework.Runtime.Scene;
using My.Framework.Runtime.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StreamerReborn
{
    public class UIControllerBattleHud : UIControllerBase
    {
        public UIControllerBattleHud(string name) : base(name)
        {
        }

        /// <summary>
        /// 包装参数
        /// </summary>
        /// <returns></returns>
        public static UIControllerBattleHud Create()
        {
            var intent = new UIIntent("BattleHud");
            return UIManager.Instance.StartUIController(intent) as UIControllerBattleHud;
        }

        /// <summary>
        /// 获取当前
        /// </summary>
        /// <returns></returns>
        public static UIControllerBattleHud GetCurrentHud()
        {
            return UIManager.Instance.FindUIControllerByName("BattleHud") as UIControllerBattleHud;
        }

        protected override void RefreshView()
        {

        }

        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_compHud = m_uiCompArray[0] as UIComponentBattleHud;
        }


        

        protected override void OnTick(float dt)
        {
            m_compHud?.Tick(dt);
            if(m_compHud.m_cardContainer.IsPreviewChooseTarget())
            {
                UIComponentAudience targetAudience = null;

                EventSystem uiEventSystem = EventSystem.current;
                PointerEventData eventData = new PointerEventData(uiEventSystem);
                eventData.position = Input.mousePosition;
                uiEventSystem.RaycastAll(eventData, m_cacheRaycastList);
                
                if (m_cacheRaycastList.Count > 0)
                {
                    foreach (var obj in m_cacheRaycastList)
                    {
                        var audience = obj.gameObject.GetComponent<UIComponentAudience>();
                        if (audience != null)
                        {
                            targetAudience = audience;
                        }
                    }
                }

                if(targetAudience == null)
                {
                    m_compHud.m_arrowHint.gameObject.SetActive(false);
                }
                else
                {
                    m_compHud.m_arrowHint.gameObject.SetActive(true);
                    m_compHud.m_arrowHint.SetParabolaPoints(m_compHud.m_cardContainer.UseCardPreviewRoot.position, targetAudience.transform.position, 6);
                }
            }
            else
            {
                m_compHud.m_arrowHint.gameObject.SetActive(false);
            }
        }
        private List<RaycastResult> m_cacheRaycastList = new List<RaycastResult>();
        #region 观众接口

        /// <summary>
        /// 添加观众
        /// </summary>
        public void AddAudience()
        {
            m_compHud.m_audienceContainer.TempAddAudience();
        }

        #endregion


        /// <summary>
        /// 主component
        /// </summary>
        protected UIComponentBattleHud m_compHud;

        #region 装配信息

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "BattleHud",
                m_layerResPath = "Assets/RuntimeAssets/UI/UIPrefab/Battle/BattleHud.prefab",
            },
        };
        protected override UIComponentDesc[] UIComponentDescArray
        {
            get { return m_uiComponentDescArray; }
        }
        private readonly UIComponentDesc[] m_uiComponentDescArray =
        {
            new UIComponentDesc
            {
                m_attachLayerName = "BattleHud",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBattleHud).ToString(),
                m_compName = "BattleHud"
            },
        };

        #endregion
    }
}

