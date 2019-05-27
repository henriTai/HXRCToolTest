using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Trump))]
public class TrumpEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Trump t = (Trump)target;
        GUILayout.Label(string.Format("Ivanka {0} - Junior {1} - Eric {2}", t.Ivanka, t.Junior, t.Eric));
    }
}
