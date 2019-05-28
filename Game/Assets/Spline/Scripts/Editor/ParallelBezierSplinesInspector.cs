using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ParallelBezierSplines))]
public class ParallelBezierSplinesInspector : Editor
{
    // for displaying bezier direction at set intervals
    private const int stepsPerCurve = 10;
    // length modifier of direction visualisation lines
    private const float directionScale = 0.5f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    [Range(0, 200)]
    public int wayPoints = 0;
    private int selectedIndex = -1;

    private ParallelBezierSplines parallel;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private bool perSpline;

    //for basic settings
    private bool basicSettingsDone = false;
    private int leftLaneCount;
    private int rightLaneCount;
    [SerializeField]
    private float spacingOverride = 1f;
    [SerializeField]
    private float adjustment = 0f;
    private float[] lSpacing;
    private float[] rSpacing;
    [SerializeField]
    private GameObject[] lStart;
    [SerializeField]
    private GameObject[] rStart;
    Nodes[] allNodes;
    


    public override void OnInspectorGUI()
    {
        if (lSpacing==null || lSpacing.Length != 3)
        {
            lSpacing = new float[] { 0, 0, 0 };
        }
        if (rSpacing==null || rSpacing.Length != 3)
        {
            rSpacing = new float[] { 0, 0, 0 };
        }
        if (lStart==null || lStart.Length != 3)
        {
            lStart = new GameObject[] { null, null, null };
        }
        if (rStart==null || rStart.Length != 3)
        {
            rStart = new GameObject[] { null, null, null };
        }
        parallel = target as ParallelBezierSplines;
        if (basicSettingsDone == false)
        {
            BasicSettingsMenu();
        }
        base.OnInspectorGUI();
    }

    private void BasicSettingsMenu()
    {
        EditorGUILayout.LabelField("Basic settings:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Instructions");
        EditorGUILayout.LabelField("1. Set lane counts (0 - 3)");
        GUILayoutOption[] layoutOptions = new GUILayoutOption[]{ };
        EditorGUILayout.LabelField("2. Set initial spacing between the lanes. The guiding spline is placed" +
            "in the middle between right side and left side lanes. Spacings are numbered from the centre" +
            "to the sides.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.LabelField("Lanes:", EditorStyles.boldLabel);
        rightLaneCount = EditorGUILayout.IntSlider(new GUIContent("Right lanes count", "0 - 3")
            ,rightLaneCount, 0, 3);
        leftLaneCount = EditorGUILayout.IntSlider(new GUIContent("Left lanes count", "opposite direction, 0 - 3")
            , leftLaneCount, 0, 3);
        if (rightLaneCount > 0)
        {
            EditorGUILayout.LabelField("Right side lanes: " + rightLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < rightLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (right)");
            rSpacing[i] = EditorGUILayout.Slider("Spacing " + (i+1),rSpacing[i], 0f, 5f);
            rStart[i] = EditorGUILayout.ObjectField("Object link" ,rStart[i], typeof(Nodes), true) as GameObject;
        }
        if (leftLaneCount > 0)
        {
            EditorGUILayout.LabelField("Left side lanes: " + leftLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < leftLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (left)");
            lSpacing[i] = EditorGUILayout.Slider("Spacing " + (i + 1), lSpacing[i], 0f, 5f);
        }
        if (leftLaneCount > 0 || rightLaneCount > 0)
        {
            EditorGUILayout.LabelField("Uniform spacing", EditorStyles.boldLabel);

            adjustment = EditorGUILayout.Slider(new GUIContent("Adustment", "Adust left-right"),
            adjustment, -5f, 5f);

            spacingOverride = EditorGUILayout.Slider(new GUIContent("Uniform spacing", "Override all spacing values"),
            spacingOverride, 0f, 10f);
            if (GUILayout.Button("Override spacing values"))
            {
                for (int i = 0; i < 3; i++)
                {
                    lSpacing[i] = spacingOverride;
                    rSpacing[i] = spacingOverride;
                }
            }
        }

    }

    private void OnSceneGUI()
    {
        if (!basicSettingsDone)
        {
            if (allNodes==null)
            {
                allNodes = GameObject.FindObjectsOfType<Nodes>();
            }
            foreach (Nodes n in allNodes)
            {
                Handles.Label(n.transform.position, n.gameObject.name);
            }
        }
    }
}
