using My.Framework.Runtime.UIExtention;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WholeDissolveController))]
public class WholeDissolveControllerEditor : Editor
{
	float _location;
	Vector2 _scale;


	protected void OnEnable()
	{
		var tempTarget = target as WholeDissolveController;
		_location = tempTarget.DissolveProgress;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI(); 

		var tempTarget = target as WholeDissolveController;

		GUILayout.Label("���ڷ�Χ");
		_location = EditorGUILayout.Slider(_location, 0, 1);

		tempTarget.DissolveProgress = _location;
	}
}

