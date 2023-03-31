using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UIExtention
{
	[ExecuteInEditMode]
	public class WholeDissolveController : MonoBehaviour
	{

		class DissolveParam
        {
			public float m_dissolveTime;
			public float m_beginValue;
			public float m_endValue;
        }

		private DissolveParam m_currentDissolveParam;
		private float m_dissolveTimer;


		[SerializeField]
		private Material SourceMaterial;

		[SerializeField]
		private Graphic[] HandledChilds;
		[SerializeField]
		private WholeDissolveItemTmp[] dissolveItems;

		/// <summary>
		/// 覆盖的material
		/// </summary>
		private Material m_overrideMaterial = null;


		private float m_effectFactor;
		public float EffectFactor
		{
			get { return m_effectFactor; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_effectFactor, value))
				{
					m_effectFactor = value;
					UpdateMaterials();
				}
			}
		}

		private Material[] cachedMats;

		/// <summary>
		/// 开始消散
		/// </summary>
		public void StartDissolve(float duration = 1.0f)
        {
			// 多次消散
			if(m_currentDissolveParam != null)
            {
                Debug.LogError("WholeDissolveController StartDissolve While Playing Dissolve");
				return;
			}

			if(m_overrideMaterial == null)
            {
				m_overrideMaterial =  new Material(SourceMaterial);
			}
			

			for (int i = 0; i < HandledChilds.Length; i++)
			{
				HandledChilds[i].material = m_overrideMaterial;
			}

			m_currentDissolveParam = new DissolveParam() {
				m_dissolveTime = duration,
				m_beginValue = 0,
				m_endValue = 1,
			};

			m_dissolveTimer = 0;

			EffectFactor = 0;
		}

		private void Update()
		{
			if (m_currentDissolveParam != null)
            {
				m_dissolveTimer += Time.deltaTime;
				float rate = m_dissolveTimer / m_currentDissolveParam.m_dissolveTime;
				
				if(rate >= 1.0f)
                {
					DissolveEnd();
					return;
				}
				EffectFactor = Mathf.Lerp(m_currentDissolveParam.m_beginValue, m_currentDissolveParam.m_endValue, rate);
			}
		}

		void Awake()
		{
			cachedMats = new Material[HandledChilds.Length];
            for (int i = 0; i < HandledChilds.Length; i++)
            {
				cachedMats[i] = HandledChilds[i].material;
			}
        }

		public void DissolveEnd()
        {
   //         for (int i = 0; i < HandledChilds.Length; i++)
   //         {
			//	HandledChilds[i].material = cachedMats[i];
			//}

            m_currentDissolveParam = null;
			m_dissolveTimer = 0;
		}

        private void OnValidate()
        {
			if (m_overrideMaterial == null)
			{
				m_overrideMaterial = new Material(SourceMaterial);
			}
		}

		/// <summary>
		/// 更新材质数据
		/// </summary>
		protected void UpdateMaterials()
        {
			m_overrideMaterial?.SetFloat("_DissolveLocation", m_effectFactor);
			foreach (var dissolveItem in dissolveItems)
			{
				dissolveItem.SetDissolveEffectFactor(EffectFactor);
			}
		}
    }
}


