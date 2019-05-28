using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
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

    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private bool perSpline;


    private static Color[] modeColors =
    {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    public override void OnInspectorGUI()
    {
        spline = target as BezierSpline;

        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle loop");
            EditorUtility.SetDirty(spline);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            spline.Loop = loop;
            spline.RecalculateLength(0);
        }

        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            bool changesMade = DrawSelectedPointInspector();
            if (changesMade)
            {
                spline.RecalculateLength(selectedIndex);
            }
            //**
            string lab = "Segment length: " + spline.GetSegmentLength(selectedIndex);
            GUILayout.Label(lab);
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            spline.RecalculateLength(spline.ControlPointCount - 1);
            // piirtäminen pitää ehkä muuttaa
        }
        EditorGUILayout.BeginHorizontal();
        perSpline = EditorGUILayout.Toggle("Per spline?", perSpline);
        wayPoints = EditorGUILayout.IntSlider("Waypoints", wayPoints, 0, 200);
        EditorGUILayout.EndHorizontal();
        if (wayPoints > 0)
        {
            if (GUILayout.Button("Spawn waypoints to marked points"))
            {
                Vector3 point = spline.GetPoint(0);
                string name = spline.gameObject.name + "_waypoints";
                GameObject parent;

                if (spline.waypointParent==null)
                {
                    parent = new GameObject(name);
                    parent.transform.position = point;
                    spline.waypointParent = parent;
                }
                else
                {
                    parent = spline.waypointParent;
                }

                if (spline.wayPoints==null)
                {
                    spline.wayPoints = new List<GameObject>();
                }
                else
                {
                    for (int i = spline.wayPoints.Count; i > 0; i--)
                    {
                        GameObject g = spline.wayPoints[i - 1];
                        spline.wayPoints.RemoveAt(i - 1);
                        g.transform.parent = null;
                        DestroyImmediate(g);
                    }
                }
                if (perSpline)
                {
                    int splines = spline.GetSegmentCount();
                    for (int i = 0; i < splines; i++)
                    {
                        for (int j = 0; j < wayPoints; j++)
                        {
                            name = spline.gameObject.name + "_" + i + "_" + j;
                            point = spline.GetSegmentedPoint(i, j/ (float)wayPoints);
                            GameObject g = new GameObject(name);
                            g.transform.position = point;
                            spline.wayPoints.Add(g);
                            g.transform.parent = parent.transform;
                            g.AddComponent<Nodes>();
                            ObjectTagger.SetIcon(g, ObjectTagger.IconType.Small, i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < wayPoints; i++)
                    {
                        name = spline.gameObject.name + "_" + i;
                        point = spline.GetPoint(i / (float)wayPoints);
                        GameObject g = new GameObject(name);
                        g.transform.position = point;
                        spline.wayPoints.Add(g);
                        g.transform.parent = parent.transform;
                        g.AddComponent<Nodes>();
                        ObjectTagger.SetIcon(g, ObjectTagger.IconType.Pix16, i);
                    }
                }
                for (int i = spline.wayPoints.Count - 1; i > 0; i--)
                {
                    spline.wayPoints[i - 1].GetComponent<Nodes>().AddNode(spline.wayPoints[i].GetComponent<Nodes>());
                }
            }
        }
        else
        {
            if (spline.wayPoints.Count > 0)
            {
                if (GUILayout.Button("Remove waypoints"))
                {
                    for (int i = spline.wayPoints.Count; i > 0; i--)
                    {
                        GameObject g = spline.wayPoints[i - 1];
                        spline.wayPoints.RemoveAt(i - 1);
                        g.transform.parent = null;
                        DestroyImmediate(g);
                    }
                    GameObject go = spline.waypointParent;
                    spline.waypointParent = null;
                    DestroyImmediate(go);
                }
            }

        }
    }

    private bool DrawSelectedPointInspector()
    {
        bool hasChanged = false;
        GUILayout.Label("Selected Point (" + selectedIndex + ")");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point (inspector)");
            EditorUtility.SetDirty(spline);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            spline.SetControlPoint(selectedIndex, point);
            hasChanged = true;
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode",
            spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Changed point mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            hasChanged = true;
        }

        return hasChanged;
    }

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
        ShowDirections();
        DrawWaypoints();
    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point;
        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 0; i <= steps; i++)
        {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index==0)
        {
            size *= 2f; // 1st node bigger
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size* handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint(); // refresh inspector
        }
        if (selectedIndex == index)
        {
            Event e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "MovePoint");
                EditorUtility.SetDirty(spline);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
                spline.RecalculateLength(selectedIndex);
            }
        }
        return point;
    }

    private void DrawWaypoints()
    {
        for (int i=0; i<spline.wayPoints.Count; i++)
        {
            Handles.color = Color.white;
            Vector3 pos = spline.wayPoints[i].transform.position;
            Handles.RadiusHandle(Quaternion.identity, pos, 1f);
        }
    }
}
