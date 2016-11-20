using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CookieCutter))]
public class CookieCutterEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Cut"))
        {
            (target as CookieCutter).Cut();
        }
    }
}
