using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineHelper))]
public class LineHelperInspector : Editor
{
    LineHelper helper;

    private void OnSceneGUI()
    {
        helper = target as LineHelper;

        Vector3 start, end;
        for (int i=0; i< helper.lines; i++)
        {
            Vector3 pos = helper.GetLine(i, out start, out end);
            Handles.DrawLine(start, end);
        }
    }
}
