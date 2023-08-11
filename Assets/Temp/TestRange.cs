using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestRange : MonoBehaviour
{
    [Range(0,1)]
    public float Range;

    public TextMeshProUGUI tmpMesh;


    // Start is called before the first frame update
    void Start()
    {
        tmpMesh = GetComponent<TextMeshProUGUI>();
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


    List<TMP_SubMeshUI> s_SubMeshUIs = new List<TMP_SubMeshUI>();
    /// <summary>
    /// Called when any TextMeshPro generated the mesh.
    /// </summary>
    /// <param name="obj">TextMeshPro object.</param>
    void OnTextChanged(Object obj)
	{
		// Skip if the object is different from the current object or the text is empty.
		var textInfo = tmpMesh.textInfo;
		if (tmpMesh != obj || textInfo.characterCount - textInfo.spaceCount <= 0)
		{
			return;
		}
        s_SubMeshUIs.Clear();

        GetComponentsInChildren<TMP_SubMeshUI>(false, s_SubMeshUIs);
		foreach (var sm in s_SubMeshUIs)
		{
            sm.material.SetFloat("_DissolveLocation", 0);
        }
	}


	// Update is called once per frame
	void Update()
    {

        //var mts = tmpMesh.fontSharedMaterials;
        foreach(var sm in s_SubMeshUIs)
        {
            sm.sharedMaterial.SetFloat("_DissolveLocation", Range);
        }
        //var tt = transform.GetChild(0);
        //var subMesh = tt.gameObject.GetComponent<TMP_SubMeshUI>();
        //mat2 = subMesh.material;

        //mat1.SetFloat("_DissolveLocation", Range);
        //mat2.SetFloat("_DissolveLocation", Range);
    }
}
