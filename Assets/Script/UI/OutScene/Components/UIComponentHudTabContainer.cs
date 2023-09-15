using System;
using System.Collections.Generic;
using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StreamerReborn
{
    /// <summary>
    /// hud的tab控制器
    /// </summary>
    public abstract class UIComponentHudTabContainer : UIComponentBase
    {

        /// <summary>
        /// 显示tab
        /// </summary>
        public void Show(params object[] args)
        {
            m_currItemIndex = -1;
            m_currTabIndex = -1;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                // show animation
            }

            OnInitTabList(args);

            while (m_tabComponentList.Count < m_currTabList.Count)
            {
                var newGo = GameObject.Instantiate(m_buildTabPrefab, m_buildTabContainer);
                var tabItem = AddTabComponent(newGo);
                tabItem.BindFields();
                m_tabComponentList.Add(tabItem);
                newGo.transform.SetParent(m_buildTabContainer);
                tabItem.OnSelect += OnTabSelect;
                newGo.SetActive(true);
            }

            int index = 0;
            for (; index < m_currTabList.Count; index++)
            {
                InitTabView(index);
                m_tabComponentList[index].gameObject.SetActive(true);
            }
            for (; index < m_tabComponentList.Count; index++)
            {
                m_tabComponentList[index].gameObject.SetActive(false);
            }

            // 默认选择
            if (m_currTabList.Count > 0)
            {
                OnTabSelect(0);
            }
        }


        /// <summary>
        /// 初始化tab信息
        /// </summary>
        protected virtual void OnInitTabList(params object[] args)
        {
        }

        /// <summary>
        /// 初始化item信息
        /// </summary>
        protected virtual void OnInitItemList()
        {
        }

        /// <summary>
        /// 添加实例化component
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        protected abstract UIComponentSelectableInList AddTabComponent(GameObject go);

        /// <summary>
        /// 添加实例化component
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        protected abstract UIComponentSelectableInList AddItemComponent(GameObject go);

        /// <summary>
        /// 初始化tab视图
        /// </summary>
        protected virtual void InitTabView(int tabIndex)
        {

        }

        /// <summary>
        /// 初始化item视图
        /// </summary>
        protected virtual void InitItemView(int itemIndex)
        {

        }

        /// <summary>
        /// 具体实现类
        /// </summary>
        protected virtual void ItemSelectedImpl()
        {

        }


        /// <summary>
        /// 当tab被选择时
        /// </summary>
        /// <param name="tabIdx"></param>
        protected void OnTabSelect(int tabIdx)
        {
            if (m_currTabIndex == tabIdx)
            {
                return;
            }

            if (tabIdx < 0 || tabIdx >= m_tabComponentList.Count)
            {
                return;
            }

            foreach (var tabItem in m_tabComponentList)
            {
                tabItem.SetSelect(false);
            }
            m_currTabIndex = tabIdx;
            UpdateTabContent(m_currTabIndex);
            m_tabComponentList[tabIdx].SetSelect(true);

            // 记录
            if (m_savedItemIndexByTab.ContainsKey(m_currTabIndex))
            {
                OnItemSelect(m_savedItemIndexByTab[m_currTabIndex]);
            }
        }


        /// <summary>
        /// 更新tab 内容
        /// </summary>
        protected void UpdateTabContent(int tabIdx)
        {
            OnInitItemList();

            // 补充不满的
            while (m_itemComponentList.Count < m_currItemList.Count)
            {
                var newGo = GameObject.Instantiate(m_buildIconPrefab, m_buildIconContainer);
                var itemComponent = AddItemComponent(newGo);
                itemComponent.BindFields();

                m_itemComponentList.Add(itemComponent);
                newGo.transform.SetParent(m_buildIconContainer);
                itemComponent.OnSelect += OnItemSelect;
                newGo.SetActive(true);
            }

            int index = 0;
            for (; index < m_currItemList.Count; index++)
            {
                InitItemView(index);

                m_itemComponentList[index].gameObject.SetActive(true);
            }
            for (; index < m_itemComponentList.Count; index++)
            {
                m_itemComponentList[index].gameObject.SetActive(false);
            }

            OnItemSelect(-1);
        }

        /// <summary>
        /// 被点选
        /// </summary>
        /// <param name="itemIdx"></param>
        protected void OnItemSelect(int itemIdx)
        {
            if (m_currItemIndex == itemIdx)
            {
                return;
            }

            // 记录
            m_savedItemIndexByTab[m_currTabIndex] = itemIdx;

            // 取消所有选择
            foreach (var item in m_itemComponentList)
            {
                item.SetSelect(false);
            }

            m_currItemIndex = itemIdx;
            ItemSelectedImpl();

            // 更新点击视图
            if (m_currItemIndex >= 0 && m_currItemIndex < m_itemComponentList.Count)
            {
                m_itemComponentList[m_currItemIndex].SetSelect(true);
            }
        }



        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();
        }

        /// <summary>
        /// 
        /// </summary>
        public TabInfo CurrTabInfo
        {
            get
            {
                if (m_currTabIndex < 0 || m_currTabIndex >= m_currTabList.Count)
                {
                    return default(TabInfo);
                }
                return m_currTabList[m_currTabIndex];
            }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        protected int m_currTabIndex = -1;
        protected int m_currItemIndex = -1;

        /// <summary>
        /// 当前tab 唯一标识信息 各子类分别解析
        /// </summary>
        protected List<TabInfo> m_currTabList = new List<TabInfo>();
        /// <summary>
        /// 当前item 唯一标识信息 各子类分别解析
        /// </summary>
        protected List<int> m_currItemList = new List<int>();

        /// <summary>
        /// 缓存各tab的选择 用于在来回切换时能保留选择
        /// </summary>
        private Dictionary<int, int> m_savedItemIndexByTab = new Dictionary<int, int>();

        /// <summary>
        /// tab 信息 复合类型
        /// </summary>
        public struct TabInfo
        {
            public int Id;
            public int P1;
        }

        /// <summary>
        /// tab
        /// </summary>
        protected List<UIComponentSelectableInList> m_tabComponentList = new List<UIComponentSelectableInList>();

        /// <summary>
        /// 池
        /// </summary>
        protected List<GameObject> m_tabObjectPool = new List<GameObject>();

        /// <summary>
        /// item
        /// </summary>
        protected List<UIComponentSelectableInList> m_itemComponentList = new List<UIComponentSelectableInList>();

        /// <summary>
        /// 池
        /// </summary>
        protected List<GameObject> m_itemObjectPool = new List<GameObject>();


        #region 内部变量

        /// <summary>
        /// 属性面板
        /// </summary>
        public GameObject m_charStatPrefab;


        #endregion

        #region 绑定区域

        [AutoBind("./ItemList/Viewport/Content")]
        public Transform m_buildIconContainer;

        [AutoBind("./ItemList/HeadItem")]
        public GameObject m_buildIconPrefab;

        [AutoBind("./TabList/Viewport/Content")]
        public Transform m_buildTabContainer;

        [AutoBind("./TabList/HeadItem")]
        public GameObject m_buildTabPrefab;

        [AutoBind("./Title/Title_Value")]
        public Text m_titleText;

        [AutoBind("./CloseButton")]
        public Button m_closeButton;

        #endregion
    }
}
