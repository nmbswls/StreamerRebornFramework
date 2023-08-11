using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.Framework.Runtime.UI
{
    public class UIControllerSimpleHint : UIControllerBase
    {
        public UIControllerSimpleHint(string name) : base(name)
        {
        }

        /// <summary>
        /// 使用指定参数创建加载
        /// </summary>
        public static void ShowSimpleHint(string hintContent)
        {
            var uiIntent = new UIIntent("SimpleHint");
            var ctrl = UIManager.Instance.StartUIController(uiIntent) as UIControllerSimpleHint;
            if (ctrl == null)
            {
                Debug.LogError("ShowSimpleHint Fail");
                return;
            }
            ctrl.ShowHint(hintContent);
        }


        protected override void InitAllUIComponents()
        {
            base.InitAllUIComponents();
            m_hintRoot = m_uiCompArray[0].transform;
            m_hintPrefeb = m_hintRoot.transform.GetChild(0).gameObject;
        }

        protected override void OnTick(float dt)
        {
            for (int i = m_hintObjs.Count - 1; i >= 0; i--)
            {
                m_hintObjs[i].Tick(dt);
                if (m_hintObjs[i].IsFinish())
                {
                    GameObject.Destroy(m_hintObjs[i].gameObject);
                    m_hintObjs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 显示
        /// </summary>
        public void ShowHint(string hintContent)
        {
            if (m_hintPrefeb == null) return;
            var newGo = GameObject.Instantiate(m_hintPrefeb, m_hintRoot);
            var comp = newGo.GetComponent<UIComponentSimpleHint>();
            comp.SetHintParam(hintContent, 1);
            m_hintObjs.Add(comp);
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        protected override void RefreshView()
        {
            
        }



        protected override void InitLayerStateOnLoadAllResCompleted()
        {
            base.InitLayerStateOnLoadAllResCompleted();
        }



        #region 点击事件

        #endregion

       
        /// <summary>
        /// 
        /// </summary>
        protected List<UIComponentSimpleHint> m_hintObjs = new List<UIComponentSimpleHint>();

        protected Transform m_hintRoot;
        protected GameObject m_hintPrefeb;

        protected override LayerDesc[] LayerDescArray
        {
            get { return m_layerDescArray; }
        }
        private readonly LayerDesc[] m_layerDescArray =
        {
            new LayerDesc
            {
                m_layerName = "SimpleHint",
                m_layerResPath = "Assets/Framework/Resources/UI/Preset/SimpleHint.prefab",
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
                m_attachLayerName = "SimpleHint",
                m_attachPath = "./",
                m_compTypeName = typeof(UIComponentBase).ToString(),
                m_compName = "SimpleHint"
            },
        };

    }
}

