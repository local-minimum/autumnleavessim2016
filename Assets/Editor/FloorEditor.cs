using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FloorGenerator))]
public class FloorEditor : Editor {

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		if (GUILayout.Button ("ReBuild")) {
			(target as FloorGenerator).ReBuild ();
		}
	}
}
