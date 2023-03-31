using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace My.Framework.Runtime.UIExtention
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class WholeDissolveItemTmp : MonoBehaviour
	{
		private TextMeshProUGUI m_textMeshPro;

		void Awake()
        {
			m_textMeshPro = GetComponent<TextMeshProUGUI>();
			m_mainMaterial = m_textMeshPro.fontMaterial;

			m_subMaterial = new Material(m_textMeshPro.spriteAsset.material);
			m_subMaterial.shaderKeywords = m_textMeshPro.spriteAsset.material.shaderKeywords;
			m_subMaterial.name += " (Instance)";
		}


		private void OnEnable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
		}
		/// <summary>
		/// This function is called when the behaviour becomes disabled () or inactive.
		/// </summary>
		protected void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
		}

		private static List<TMP_SubMeshUI> s_SubMeshUIs = new List<TMP_SubMeshUI>();
		private List<Material> m_subMeshMaterials = new List<Material>();

		private Material m_mainMaterial;
		private Material m_subMaterial;
		/// <summary>
		/// Called when any TextMeshPro generated the mesh.
		/// </summary>
		/// <param name="obj">TextMeshPro object.</param>
		void OnTextChanged(Object obj)
		{
			var textInfo = m_textMeshPro.textInfo;
			if (m_textMeshPro != obj || textInfo.characterCount - textInfo.spaceCount <= 0)
			{
				return;
			}

			s_SubMeshUIs.Clear();
			m_subMeshMaterials.Clear();
			GetComponentsInChildren(false, s_SubMeshUIs);
			foreach (var sm in s_SubMeshUIs)
			{
				sm.sharedMaterial = m_subMaterial;
			}
		}

		public void SetDissolveEffectFactor(float effectFactor)
        {
			m_mainMaterial.SetFloat("_DissolveLocation", effectFactor);
			m_subMaterial.SetFloat("_DissolveLocation", effectFactor);
		}

	}
}


