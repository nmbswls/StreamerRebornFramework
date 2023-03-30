using System.Collections;
using System.Collections.Generic;
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

		/// <summary>
		/// 从开始到消失的时间
		/// </summary>
		[Header("消散时间")]
		public float DissolveProgress = 0;

		private float m_lastDissolveProgress = 0;

		[SerializeField]
		private Material SourceMaterial;
		[SerializeField]
		private Material SourceMaterialTMP;
		[SerializeField]
		private Material SourceMaterialTMP_Sprite;

		[SerializeField]
		private Graphic[] HandledChilds;

		/// <summary>
		/// 覆盖的material
		/// </summary>
		private Material m_overrideMaterial = null;
		/// <summary>
		/// 覆盖的material TMP
		/// </summary>
		private Material m_overrideMaterialTMP = null;
		/// <summary>
		/// 覆盖的material TMP_Sprite
		/// </summary>
		private Material m_overrideMaterialTMP_Sprite = null;

		private float m_dissolveLocation;
		public float DissolveLocation
		{
			get { return m_dissolveLocation; }
			set
			{
				m_dissolveLocation = value;
				m_overrideMaterial?.SetFloat("_DissolveLocation", m_dissolveLocation);
				m_overrideMaterialTMP?.SetFloat("_DissolveLocation", m_dissolveLocation);
				m_overrideMaterialTMP_Sprite?.SetFloat("_DissolveLocation", m_dissolveLocation);
			}
		}

		//private Material[] cachedMats;


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
			if (m_overrideMaterialTMP == null)
			{
				m_overrideMaterialTMP = new Material(SourceMaterialTMP);
			}
			if (m_overrideMaterialTMP_Sprite == null)
			{
				m_overrideMaterialTMP_Sprite = new Material(SourceMaterialTMP_Sprite);
			}

			for (int i = 0; i < HandledChilds.Length; i++)
			{
				if(HandledChilds[i] is TextMeshProUGUI)
                {
					var tmp = HandledChilds[i] as TextMeshProUGUI;
					m_overrideMaterialTMP.mainTexture = tmp.fontMaterial.mainTexture;
					tmp.fontMaterial = m_overrideMaterialTMP;

					m_overrideMaterialTMP_Sprite.mainTexture = tmp.spriteAsset.material.mainTexture;
					tmp.spriteAsset.material = m_overrideMaterialTMP_Sprite;

					//TMP_SubMeshUI subElem = HandledChilds[i].GetComponentInChildren<TMP_SubMeshUI>();
					//m_overrideMaterialTMP_Sprite.mainTexture = subElem.spriteAsset.material.mainTexture;
					//subElem.spriteAsset.material = m_overrideMaterialTMP_Sprite;
				}
				else
                {
					HandledChilds[i].material = m_overrideMaterial;
				}
			}

			m_currentDissolveParam = new DissolveParam() {
				m_dissolveTime = duration,
				m_beginValue = 0,
				m_endValue = 1,
			};

			m_dissolveTimer = 0;

			DissolveProgress = 0;
			m_lastDissolveProgress = 0;

			DissolveLocation = 0;
		}

		private void Update()
		{
			//if (m_lastDissolveProgress != DissolveProgress)
			{
				DissolveLocation = DissolveProgress;
				m_lastDissolveProgress = DissolveProgress;
			}


			if(m_currentDissolveParam != null)
            {
				m_dissolveTimer += Time.deltaTime;
				float rate = m_dissolveTimer / m_currentDissolveParam.m_dissolveTime;
				
				if(rate >= 1.0f)
                {
					DissolveEnd();
					return;
				}
				DissolveProgress = Mathf.Lerp(m_currentDissolveParam.m_beginValue, m_currentDissolveParam.m_endValue, rate);
			}
		}

		void Awake()
		{
			//cachedMats = new Material[HandledChilds.Length];
			//for (int i = 0; i < HandledChilds.Length; i++)
			//{
			//	if (HandledChilds[i] is TextMeshProUGUI)
			//	{
			//		var tmp = HandledChilds[i] as TextMeshProUGUI;
			//		cachedMats[i] = tmp.fontMaterial;
			//	}
			//	else
   //             {
			//		cachedMats[i] = HandledChilds[i].material;
			//	}
			//}
		}

		public void DissolveEnd()
        {
			//for (int i = 0; i < HandledChilds.Length; i++)
			//{
			//	if (HandledChilds[i] is TextMeshProUGUI)
			//	{
			//		var tmp = HandledChilds[i] as TextMeshProUGUI;
			//		tmp.fontMaterial = cachedMats[i];
			//	}
			//	else
   //             {
			//		HandledChilds[i].material = cachedMats[i];
			//	}
			//}
			m_currentDissolveParam = null;
			m_dissolveTimer = 0;
			m_lastDissolveProgress = 0;
		}

        private void OnValidate()
        {
			if (m_overrideMaterial == null)
			{
				m_overrideMaterial = new Material(SourceMaterial);
			}
			if (m_overrideMaterialTMP == null)
			{
				m_overrideMaterialTMP = new Material(SourceMaterialTMP);
			}
			if (m_overrideMaterialTMP_Sprite == null)
			{
				m_overrideMaterialTMP_Sprite = new Material(SourceMaterialTMP_Sprite);
			}
		}
    }
}


