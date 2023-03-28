using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UIExtention
{
	[ExecuteInEditMode]
	public class WholeDissolveController : MonoBehaviour
	{
		/// <summary>
		/// 从开始到消失的时间
		/// </summary>
		[Header("消散时间")]
		public float DissolveTime = 1f;

		[SerializeField]
		private Material SourceMaterial;

		
		[SerializeField]
		private Graphic[] HandledChilds;

		/// <summary>
		/// 覆盖的material
		/// </summary>
		private Material m_overrideMaterial = null;

		private float _range;
		public float Range
		{
			get { return _range; }
			set
			{
				_range = value;
				m_overrideMaterial?.SetFloat("_DissolveRange", _range);
			}
		}

		private Material[] cachedMats;

		private float m_dissolveTimer;

		/// <summary>
		/// 开始消散
		/// </summary>
		public void StartDissolve()
        {
			// 多次消散
			if(m_dissolveTimer > 0)
            {
                Debug.LogError("WholeDissolveController StartDissolve While Playing Dissolve");
				return;
			}

			m_dissolveTimer = 1f;
			if(m_overrideMaterial == null)
            {
				m_overrideMaterial =  new Material(SourceMaterial);
			}
			_range = 0f;
			for (int i = 0; i < HandledChilds.Length; i++)
			{
				HandledChilds[i].material = m_overrideMaterial;
			}
		}

		private void Update()
		{
			if(m_dissolveTimer <= 0)
            {
				return;
            }
			m_dissolveTimer -= 1.0f / DissolveTime * Time.deltaTime;
			Range = m_dissolveTimer * 1.0f;
			if(m_dissolveTimer <= 0)
            {
				DissolveEnd();
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
			for (int i = 0; i < HandledChilds.Length; i++)
			{
				HandledChilds[i].material = cachedMats[i];
			}
		}
		

	}
}


