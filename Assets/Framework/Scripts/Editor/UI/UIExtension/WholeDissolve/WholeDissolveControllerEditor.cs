using My.Framework.Runtime.UIExtention;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WholeDissolveController))]
public class WholeDissolveControllerEditor : Editor
{
	float _range;
	float _width;
	Vector2 _scale;


	protected void OnEnable()
	{
		var tempTarget = target as WholeDissolveController;
		_range = tempTarget.Range;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI(); 

		var tempTarget = target as WholeDissolveController;

		GUILayout.Label("ÏûÈÚ·¶Î§");
		_range = EditorGUILayout.Slider(_range, 0, 1);

		tempTarget.Range = _range;
	}
}

