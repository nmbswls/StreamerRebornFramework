using My.Framework.Runtime.UIExtention;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WholeDissolveController))]
public class WholeDissolveControllerEditor : Editor
{
	float _effectFactor;

	protected void OnEnable()
	{
		var tempTarget = target as WholeDissolveController;
		_effectFactor = tempTarget.EffectFactor;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI(); 

		var tempTarget = target as WholeDissolveController;

		GUILayout.Label("ÏûÈÚ²ÎÊý");
		_effectFactor = EditorGUILayout.Slider(_effectFactor, 0, 1);

		tempTarget.EffectFactor = _effectFactor;
	}
}

