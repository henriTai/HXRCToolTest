﻿using System.Collections;
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
    private float angle = 0f;
    private bool arraysInitialized = false;

    private bool nameIsValid = false;
    private bool nameAutoChecked = true;
    private string roadName = "";
    private string renameInfo = "";

    private ParallelBezierSplines parallel;
    private GameObject roadNetwork;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private bool perSpline;
    private bool advancedOptionsOn = false;
    private bool[] selected;
    private Vector3[] rightBusPoints;
    private Vector3[] leftBusPoints;

    //for basic settings
    private bool laneCountSet = false;
    bool linkedToNode = false;
    // if this is false, current selected general direction is used
    bool directionSet = false;
    private bool previewNodes = true;
    bool verifying = false;

    private int leftLaneCount;
    private int rightLaneCount;
    [SerializeField]
    private float spacingOverride = 1f;
    [SerializeField]
    private float adjustment = 0f;
    private float[] lSpacing;
    private float[] rSpacing;
    [SerializeField]
    private Nodes[] lEnd;
    [SerializeField]
    private Nodes[] rStart;
    [SerializeField]
    private Vector3[] guidePoints;
    [SerializeField]
    private float defaultLength = 10f;
    [SerializeField]
    Directions generalDirection;
    [SerializeField]
    Vector3 directionVector;

    //All nodes in scene are gathered here for drawing purposes
    Nodes[] allNodes;
    

    public override void OnInspectorGUI()
    {
        parallel = target as ParallelBezierSplines;
        Undo.RecordObject(parallel, "changed");

        NameMenu();

        if (arraysInitialized == false)
        {
            InitializeArrays();
        }
        if (!parallel.Initialized)
        {
            BasicSettingsMenu();
        }
        else
        {
            if (!parallel.LanesSet)
            {
                EditMenu();
            }
            else
            {
                WaypointEditMenu();
            }
        }
        base.OnInspectorGUI();
    }

    private void InitializeArrays()
    {
        if (lSpacing == null || lSpacing.Length != 3)
        {
            lSpacing = new float[] { 0, 0, 0 };
        }
        if (rSpacing == null || rSpacing.Length != 3)
        {
            rSpacing = new float[] { 0, 0, 0 };
        }
        if (lEnd == null || lEnd.Length != 3)
        {
            lEnd = new Nodes[] { null, null, null };
        }
        if (rStart == null || rStart.Length != 3)
        {
            rStart = new Nodes[] { null, null, null };
        }
        if (guidePoints == null || guidePoints.Length != 4)
        {
            Vector3 pos = parallel.transform.position;
            guidePoints = new Vector3[] { pos, pos, pos, pos };
        }
        if (selected == null || selected.Length != 6)
        {
            //right lanes 1, 2, 3, left lanes 1, 2, 3
            selected = new bool[] { false, false, false, false, false, false };
        }
        generalDirection = Directions.North;
        directionVector = GeneralDirection.DirectionVector(generalDirection);

        arraysInitialized = true;
    }

    private void NameMenu()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Road network:", EditorStyles.boldLabel);
        if (roadNetwork == null)
        {
            EditorGUILayout.LabelField("Not selected");
        }
        else
        {
            EditorGUILayout.LabelField(roadNetwork.name);
        }
        EditorGUILayout.EndHorizontal();
        if (roadNetwork == null)
        {
            if (parallel.roadNetwork != null)
            {
                roadNetwork = parallel.roadNetwork;
                nameAutoChecked = false;
            }
            else
            {
                RoadNetwork[] networks = GameObject.FindObjectsOfType<RoadNetwork>();
                if (networks == null || networks.Length == 0)
                {
                    GameObject g = new GameObject();
                    g.AddComponent<RoadNetwork>();
                    g.name = "NodeNetwork";
                    roadNetwork = g;
                    parallel.roadNetwork = g;
                }
                else if (networks.Length == 1)
                {
                    roadNetwork = networks[0].gameObject;
                    nameAutoChecked = false;
                }
                else
                {
                    EditorGUILayout.LabelField("Select parent network", EditorStyles.boldLabel);
                    for (int i = 0; i < networks.Length; i++)
                    {
                        bool selected = false;
                        selected = EditorGUILayout.Toggle(networks[i].gameObject.name, selected);
                        if (selected)
                        {
                            roadNetwork = networks[i].gameObject;
                            parallel.roadNetwork = networks[i].gameObject;
                            nameAutoChecked = false;
                        }
                    }
                }
            }
        }

        EditorGUILayout.Separator();

        if (roadNetwork != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Road name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(parallel.gameObject.name);
            EditorGUILayout.EndHorizontal();
            if (!nameAutoChecked)
            {
                nameAutoChecked = true;
                nameIsValid = CheckName(parallel.gameObject.name);
                if (nameIsValid)
                {
                    roadName = parallel.gameObject.name;
                }
            }
            if (nameIsValid)
            {
                ItalicLabel("Name is valid");
            }
            else
            {
                roadName = "";
                WarningLabel("Invalid name. Name already exists.");
            }
            roadName = GUILayout.TextField(roadName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter name");
            if (GUILayout.Button("Rename"))
            {
                bool valid = CheckName(roadName);
                if (!valid)
                {
                    renameInfo = "New name was not valid.";
                }
                else
                {
                    renameInfo = "Name changed to '" + roadName + "'.";
                    parallel.gameObject.name = roadName;
                    nameIsValid = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            ItalicLabel(renameInfo);

            EditorGUILayout.Separator();
        }
        EditorGUILayout.LabelField("Traffic", EditorStyles.boldLabel);
        parallel.Traffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic size", parallel.Traffic);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Overall speed limit", EditorStyles.boldLabel);
        parallel.SpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Limit", parallel.SpeedLimit);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void WarningLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.normal.textColor = Color.red;
        EditorGUILayout.LabelField(message, gs);
    }

    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField(message, gs);
    }

    private bool CheckName(string name)
    {
        if (roadNetwork == null)
        {
            return false;
        }
        Transform t = roadNetwork.transform.Find(name);
        if (t == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DrawEditorLine()
    {
        int thickness = 2;
        int padding = 10;
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, Color.black);
    }

    private void BasicSettingsMenu()
    {
        if (verifying)
        {
            EditorGUILayout.LabelField("Are you sure?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                verifying = false;
                InitializeParallel();
            }
            if (GUILayout.Button("No"))
            {
                verifying = false;
            }
            EditorGUILayout.EndHorizontal();
            return;
        }
        parallel = target as ParallelBezierSplines;
        ShowSetupInstructions();
        if (!laneCountSet)
        {
            ShowLaneCountSelection();
            return;
        }
        if (GUILayout.Button("Reset and set lane counts again (1.)"))
        {
            ResetValues();
            SceneView.RepaintAll();
        }
        ShowLaneSetupSelection();
        
        if (leftLaneCount > 0 || rightLaneCount > 0)
        {
            ShowSpacingSelection();
            if (GUILayout.Button("Setup done"))
            {
                verifying = true;
            }
        }

    }

    private void EditMenu()
    {
        ShowEditInstructions();
        if (GUILayout.Button("Add segment"))
        {
            parallel.AddCurve();
        }
        ShowSelectedNodeInfo();
        ShowAngleChangeOption();
        ShowAdvancedOptions();
        if (GUILayout.Button("Continue"))
        {
            parallel.LanesSet = true;
        }
    }

    private void WaypointEditMenu()
    {
        ShowNodeSetupInstructions();
        EditorGUILayout.LabelField("Node set-up", EditorStyles.boldLabel);
        if (GUILayout.Button("Back"))
        {
            if (parallel.NodesSet)
            {
                parallel.NodesSet = false;
                rightBusPoints = null;
                leftBusPoints = null;
                SceneView.RepaintAll();
            }
            else
            {
                parallel.LanesSet = false;
            }
        }

        EditorGUILayout.Separator();

        if (parallel.NodesSet)
        {
            BusLanesMenu();
            LaneChangeMenu();
            return;
        }

        EditorGUILayout.LabelField("Node placement", EditorStyles.boldLabel);

        bool prev = previewNodes;
        prev = EditorGUILayout.Toggle("Preview nodes", prev);
        if (prev != previewNodes)
        {
            previewNodes = prev;
            SceneView.RepaintAll();
        }

        for (int i = 0; i < parallel.SegmentCount; i++)
        {
            int pts = parallel.GetNodesOnSegment(i);
            pts = EditorGUILayout.IntField("Seg. " + (i + 1) + " nodes", pts);
            if (pts > 0 && pts != parallel.GetNodesOnSegment(i))
            {
                parallel.SetNodesOnSegment(i, pts);
            }
        }

        DrawEditorLine();
        if (GUILayout.Button("Next"))
        {
            //check that node count is set for each segment and count nodes
            bool segSet = true;
            int nodes = 1;
            for (int i = 0; i < parallel.SegmentCount; i++)
            {
                int n = parallel.GetNodesOnSegment(i);
                if (n == 0)
                {
                    segSet = false;
                    break;
                }
                else
                {
                    nodes += n;
                }
            }
            if (segSet == true)
            {
                parallel.NodeCount = nodes;
                if (!parallel.BusLaneRight)
                {
                    parallel.BusRightStart = 0;
                    parallel.BusRightEnd = nodes - 1;
                }
                else
                {
                    if (parallel.BusRightEnd > nodes - 1)
                    {
                        parallel.BusRightEnd = nodes - 1;
                    }
                    if (parallel.BusRightStart >= parallel.BusRightEnd)
                    {
                        parallel.BusRightStart = parallel.BusRightEnd - 1;
                    }
                }
                if (!parallel.BusLaneLeft)
                {
                    parallel.BusLeftStart = 0;
                    parallel.BusLeftEnd = nodes - 1;
                }
                else
                {
                    if (parallel.BusLeftEnd > nodes - 1)
                    {
                        parallel.BusLeftEnd = nodes - 1;
                    }
                    if (parallel.BusLeftStart >= parallel.BusLeftEnd)
                    {
                        parallel.BusLeftStart = parallel.BusLeftEnd - 1;
                    }
                }
                parallel.NodesSet = true;
            }
            SceneView.RepaintAll();
        }
    }

    private void BusLanesMenu()
    {
        EditorGUILayout.LabelField("Bus lanes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Is the righmost lane on the RIGHT side (BLUE) lanes a bus lane?", EditorStyles.wordWrappedLabel);
        bool busOn = parallel.BusLaneRight;
        parallel.BusLaneRight = EditorGUILayout.Toggle(parallel.BusLaneRight);
        if (busOn != parallel.BusLaneRight)
        {
            rightBusPoints = null;
            SceneView.RepaintAll();
        }
        if (parallel.BusLaneRight)
        {
            int val = EditorGUILayout.IntField("From " + parallel.BusRightStart, parallel.BusRightStart);
            if (val != parallel.BusRightStart)
            {
                if (val >= 0 && val < parallel.BusRightEnd)
                {
                    rightBusPoints = null;
                    parallel.BusRightStart = val;
                    SceneView.RepaintAll();
                }
            }
            val = EditorGUILayout.IntField("To " + parallel.BusRightEnd, parallel.BusRightEnd);
            if (val != parallel.BusRightEnd)
            {
                if (val > parallel.BusRightStart && val < parallel.NodeCount)
                {
                    rightBusPoints = null;
                    parallel.BusRightEnd = val;
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.LabelField("(0-" + (parallel.NodeCount-1) + ")");
            EditorGUILayout.Separator();
        }

        EditorGUILayout.LabelField("Is the righmost lane on the LEFT side (RED) lanes a bus lane?", EditorStyles.wordWrappedLabel);
        busOn = parallel.BusLaneLeft;
        parallel.BusLaneLeft = EditorGUILayout.Toggle(parallel.BusLaneLeft);
        if (busOn != parallel.BusLaneLeft)
        {
            leftBusPoints = null;
            SceneView.RepaintAll();
        }
        if (parallel.BusLaneLeft)
        {
            int val = EditorGUILayout.IntField("From " + parallel.BusLeftStart, parallel.BusLeftStart);
            if (val != parallel.BusLeftStart)
            {
                if (val >= 0 && val < parallel.BusLeftEnd)
                {
                    leftBusPoints = null;
                    parallel.BusLeftStart = val;
                    SceneView.RepaintAll();
                }
            }
            val = EditorGUILayout.IntField("To " + parallel.BusLeftEnd, parallel.BusLeftEnd);
            if (val != parallel.BusLeftEnd)
            {
                if (val > parallel.BusLeftStart && val < parallel.NodeCount)
                {
                    leftBusPoints = null;
                    parallel.BusLeftEnd = val;
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.LabelField("(0-" + (parallel.NodeCount - 1) + ")");
        }
        DrawEditorLine();
    }

    private void LaneChangeMenu()
    {
        EditorGUILayout.LabelField("Lane change options", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        bool v;
        if (parallel.RightLaneCount > 1)
        {
            EditorGUILayout.LabelField("Right lane 1", EditorStyles.boldLabel);
            v = parallel.permittedLaneChanges[0];
            v = EditorGUILayout.ToggleLeft("Change to lane 2 (right) permitted?", v);
            if (v != parallel.permittedLaneChanges[0])
            {
                parallel.permittedLaneChanges[0] = v;
            }

            EditorGUILayout.LabelField("Right lane 2", EditorStyles.boldLabel);
            v = parallel.permittedLaneChanges[1];
            v = EditorGUILayout.ToggleLeft("Change to lane 1 (left) permitted?", v);
            if (v != parallel.permittedLaneChanges[1])
            {
                parallel.permittedLaneChanges[1] = v;
            }
            if (parallel.RightLaneCount > 2)
            {
                v = parallel.permittedLaneChanges[2];
                v = EditorGUILayout.ToggleLeft("Change to lane 3 (right) permitted?", v);
                if (v != parallel.permittedLaneChanges[2])
                {
                    parallel.permittedLaneChanges[2] = v;
                }
                EditorGUILayout.LabelField("Right lane 3", EditorStyles.boldLabel);
                v = parallel.permittedLaneChanges[3];
                v = EditorGUILayout.ToggleLeft("Change to lane 2 (left) permitted?", v);
                if (v != parallel.permittedLaneChanges[3])
                {
                    parallel.permittedLaneChanges[3] = v;
                }
            }

            EditorGUILayout.Separator();
        }

        if (parallel.LeftLaneCount > 1)
        {
            EditorGUILayout.LabelField("Left lane 1", EditorStyles.boldLabel);
            v = parallel.permittedLaneChanges[4];
            v = EditorGUILayout.ToggleLeft("Change to lane 2 (right) permitted?", v);
            if (v != parallel.permittedLaneChanges[4])
            {
                parallel.permittedLaneChanges[4] = v;
            }

            EditorGUILayout.LabelField("Left lane 2", EditorStyles.boldLabel);
            v = parallel.permittedLaneChanges[5];
            v = EditorGUILayout.ToggleLeft("Change to lane 1 (left) permitted?", v);
            if (v != parallel.permittedLaneChanges[5])
            {
                parallel.permittedLaneChanges[5] = v;
            }
            if (parallel.RightLaneCount > 2)
            {
                v = parallel.permittedLaneChanges[6];
                v = EditorGUILayout.ToggleLeft("Change to lane 3 (right) permitted?", v);
                if (v != parallel.permittedLaneChanges[6])
                {
                    parallel.permittedLaneChanges[6] = v;
                }
                EditorGUILayout.LabelField("Left lane 3", EditorStyles.boldLabel);
                v = parallel.permittedLaneChanges[7];
                v = EditorGUILayout.ToggleLeft("Change to lane 2 (left) permitted?", v);
                if (v != parallel.permittedLaneChanges[7])
                {
                    parallel.permittedLaneChanges[7] = v;
                }
            }
        }

    }

    private void ShowAdvancedOptions()
    {
        if (!advancedOptionsOn)
        {
            if (GUILayout.Button("Show advanced options"))
            {
                advancedOptionsOn = true;
                SceneView.RepaintAll();
            }
            return;
        }
        if (GUILayout.Button("Hide advanced options"))
        {
            advancedOptionsOn = false;
            SceneView.RepaintAll();
        }
        EditorGUILayout.LabelField("Advanced options", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Edit selected lanes:", EditorStyles.boldLabel);
        bool s = selected[0];
        selected[0] = EditorGUILayout.Toggle("R. lane 1", selected[0]);
        if (s != selected[0])
        {
            SceneView.RepaintAll();
        }
        s = selected[1];
        selected[1] = EditorGUILayout.Toggle("R. lane 2", selected[1]);
        if (s != selected[1])
        {
            SceneView.RepaintAll();
        }
        s = selected[2];
        selected[2] = EditorGUILayout.Toggle("R. lane 3", selected[2]);
        if (s != selected[2])
        {
            SceneView.RepaintAll();
        }
        s = selected[3];
        selected[3] = EditorGUILayout.Toggle("L. lane 1", selected[3]);
        if (s != selected[3])
        {
            SceneView.RepaintAll();
        }
        s = selected[4];
        selected[4] = EditorGUILayout.Toggle("L. lane 2", selected[4]);
        if (s != selected[4])
        {
            SceneView.RepaintAll();
        }
        s = selected[5];
        selected[5] = EditorGUILayout.Toggle("L. lane 3", selected[5]);
        if (s != selected[5])
        {
            SceneView.RepaintAll();
        }
    }

    private void ShowSelectedNodeInfo()
    {
        if (selectedIndex > parallel.ControlPointCount - 1)
        {
            selectedIndex = parallel.ControlPointCount - 1;
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            if (selectedIndex < 1)
            {
                selectedIndex = parallel.ControlPointCount - 1;
            }
            else
            {
                selectedIndex--;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.LabelField("Bezier point " + selectedIndex);
        if (GUILayout.Button("Next"))
        {
            if (selectedIndex == parallel.ControlPointCount - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex++;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        int pointNumber = -1;
        int leftPoint = -1;
        string nodeString, leftString;
        leftString = "";
        if (selectedIndex == -1)
        {
            nodeString = "not selected";
            leftString = "";
        }
        else
        {
            if (selectedIndex % 3 == 0)
            {
                pointNumber = selectedIndex / 3;
                nodeString = "point " + pointNumber;
                leftPoint = (parallel.ControlPointCount - 1 - selectedIndex) / 3;
                leftString = " (point " + leftPoint + ")";
            }
            else if (selectedIndex == 1)
            {
                pointNumber = 0;
                nodeString = "curve control (point 0)";
                leftPoint = (parallel.ControlPointCount - 1) / 3;
                leftString = "curve control (point " + leftPoint + ")";
            }
            else if (selectedIndex == parallel.ControlPointCount - 2)
            {
                pointNumber = (selectedIndex + 1) / 3;
                nodeString = "curve control (point " + pointNumber + ")";
                leftPoint = 0;
                leftString = "curve control (point 0)";
            }
            else if ((selectedIndex + 1) % 3 == 0)
            {
                pointNumber = (selectedIndex + 1) / 3;
                nodeString = "curve control 1 (point " + pointNumber + ")";
                leftPoint = (parallel.ControlPointCount - 1) / 3 - pointNumber;
                leftString = "curve control 2 (point " + leftPoint + ")";
            }
            else
            {
                pointNumber = (selectedIndex - 1) / 3;
                nodeString = "curve control 2 (point " + pointNumber + ")";
                leftPoint = (parallel.ControlPointCount - 1) / 3 - pointNumber;
                leftString = "curve control 1 (point " + leftPoint + ")";
            }
        }
        EditorGUILayout.LabelField("Selected node: " + nodeString);
        EditorGUILayout.LabelField("Left side: " + leftString);
        if (pointNumber != -1)
        {
            EditorGUILayout.LabelField("Point " + pointNumber, EditorStyles.boldLabel);
            if (pointNumber != -1) // this value was 0, it prevented editing first nodes on right lane (on purpose) but also left lane end nodes
            {
                EditorGUILayout.LabelField("Spacing options", EditorStyles.boldLabel);
                //right lane spacings
                for (int i = 0; i < parallel.RightLaneCount; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    rSpacing[i] = EditorGUILayout.FloatField("R. lane " + (i+1) 
                        + " current: " + parallel.GetRightSpacing(i, selectedIndex), rSpacing[i]);
                    if (GUILayout.Button("Change"))
                    {
                        if (rSpacing[i] != parallel.GetRightSpacing(i, selectedIndex))
                        {
                            float changeAmount = rSpacing[i] - parallel.GetRightSpacing(i, selectedIndex);
                            parallel.SetRightSpacing(i, selectedIndex, rSpacing[i]);
                            int index = pointNumber * 3;
                            Vector3 right = GeneralDirection.DirectionRight(parallel.GetDirection((float)index / parallel.CurveCount));
                            Vector3 moved = changeAmount * right;
                            for (int j = i; j < 3; j++)
                            {
                                Vector3 newPoint = moved + parallel.GetControlPointRight(j, index);
                                parallel.SetControlPointRight(j, index, newPoint);
                                parallel.RecalculateLengthRight(j, index);
                            }

                            SceneView.RepaintAll();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                //left lane spacings
                for (int i = 0; i < parallel.LeftLaneCount; i++)
                {
                    int index = leftPoint * 3;
                    EditorGUILayout.BeginHorizontal();
                    lSpacing[i] = EditorGUILayout.FloatField("L. lane " + (i + 1)
                        + "current: " + parallel.GetLeftSpacing(i, index), lSpacing[i]);
                    if (GUILayout.Button("Change"))
                    {
                        if (lSpacing[i] != parallel.GetLeftSpacing(i, index))
                        {
                            float changeAmount = lSpacing[i] - parallel.GetLeftSpacing(i, index);
                            parallel.SetLeftSpacing(i, index, lSpacing[i]);
                            Vector3 right = GeneralDirection.DirectionRight(parallel.GetDirection((float)pointNumber * 3 / parallel.CurveCount));
                            Vector3 moved = - changeAmount * right;
                            for (int j = i; j < 3; j++)
                            {
                                Vector3 newPoint = moved + parallel.GetControlPointLeft(j, index);
                                parallel.SetControlPointLeft(j, index, newPoint);
                                parallel.RecalculateLengthLeft(j, index);
                            }

                            SceneView.RepaintAll();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (pointNumber == 0 || pointNumber == parallel.ControlPointCount / 3)
            {
                // linking options
            }
        }

    }

    private void ShowAngleChangeOption()
    {
        if (selectedIndex > 1)
        {
            EditorGUILayout.LabelField("Change angle (Point " + (selectedIndex + 1)/3 + ")", EditorStyles.boldLabel);
            angle = EditorGUILayout.FloatField("Angle", angle);
            if (GUILayout.Button("Add angle"))
            {
                if (angle != 0f)
                {
                    Vector3 deg = new Vector3(0f, angle, 0f);
                    int pivotIndex = ((selectedIndex + 1) / 3) * 3;
                    Vector3 pivot = parallel.GetControlPoint(pivotIndex);
                    //move guide spline's control points
                    Vector3 dir = parallel.GetControlPoint(pivotIndex - 1) - pivot;
                    dir = Quaternion.Euler(deg) * dir;
                    parallel.SetControlPoint(pivotIndex - 1, dir + pivot);

                    for (int i = 0; i < 3; i++)
                    {
                        //right lanes
                        dir = parallel.GetControlPointRight(i, pivotIndex) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetControlPointRight(i, pivotIndex, dir + pivot);
                        dir = parallel.GetControlPointRight(i, pivotIndex - 1) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetControlPointRight(i, pivotIndex - 1, dir + pivot);
                        //left lanes
                        int leftIndex = parallel.ControlPointCount - 1 - pivotIndex;
                        dir = parallel.GetControlPointLeft(i, leftIndex) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetControlPointLeft(i, leftIndex, dir + pivot);
                        dir = parallel.GetControlPointLeft(i, leftIndex + 1) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetControlPointLeft(i, leftIndex + 1, dir + pivot);

                    }
                }
                SceneView.RepaintAll();
            }
        }
    }

    private void InitializeParallel()
    {
        //parallel.Reset();
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        Vector3 gp0, gp1, gp2, gp3;
        gp0 = Vector3.zero;
        gp1 = guidePoints[1] - guidePoints[0];
        gp2 = guidePoints[2] - guidePoints[0];
        gp3 = guidePoints[3] - guidePoints[0];
        // Note: node points (0, 3) must be set BEFORE the control points (1, 2)
        //parallel.transform.position = gp0;
        parallel.transform.position += right * adjustment;
        parallel.SetControlPoint(0, gp0);
        parallel.SetControlPoint(3, gp3);
        parallel.SetControlPoint(1, gp1);
        parallel.SetControlPoint(2, gp2);
        parallel.RecalculateLength(0);

        parallel.RightLaneCount = rightLaneCount;
        parallel.LeftLaneCount = leftLaneCount;
        float space = 0f;
        for (int i = 0; i < rightLaneCount; i++)
        {
            space += rSpacing[i];
            parallel.SetControlPointRight(i, 0, gp0 + right * space);
            parallel.SetControlPointRight(i, 3, gp3 + right * space);
            parallel.SetControlPointRight(i, 1, gp1 + right * space);
            parallel.SetControlPointRight(i, 2, gp2 + right * space);
            parallel.SetRightSpacing(i, 0, rSpacing[i]);
            parallel.SetRightSpacing(i, 3, rSpacing[i]);
            parallel.RecalculateLengthRight(i, 0);
        }
        space = 0f;
        for (int i = 0; i < leftLaneCount; i++)
        {
            space += lSpacing[i];
            //opposite direction
            parallel.SetControlPointLeft(i, 3, gp0 - right * space);
            parallel.SetControlPointLeft(i, 0, gp3 - right * space);
            parallel.SetControlPointLeft(i, 2, gp1 - right * space);
            parallel.SetControlPointLeft(i, 1, gp2 - right * space);
            parallel.SetLeftSpacing(i, 0, lSpacing[i]);
            parallel.SetLeftSpacing(i, 3, lSpacing[i]);
            parallel.RecalculateLengthLeft(i, 0);
        }
        adjustment = 0f;
        parallel.Initialized = true;
        SceneView.RepaintAll();
    }

    private void ShowSetupInstructions()
    {
        EditorGUILayout.LabelField("Basic settings:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Instructions:");
        EditorGUILayout.LabelField("1. Set lane counts (0 - 3) for both directions.");
        EditorGUILayout.LabelField("2. Set initial spacing between the lanes. The guiding spline is placed " +
            "in the middle between right side and left side lanes. Spacings are numbered from the centre " +
            "to the sides.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("3. You can anchor this road section to an existing nodepoint using " +
            "the object link field of the respective lane you want to link. (You can link the rest of the lanes " +
            "later on.) The direction will be derived from the linked nodepoint.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("4. If none of the lanes are linked to an existing node, you can set the " +
            "initial direction of the lanes. Yellow line marks the guiding spline and blue lines and red lines " +
            "right side lanes and left side lanes respectively. You can also set the length of the (first) " +
            "spline(s), adjust their uniform positioning (left - right) and set an uniform spacing value for " +
            "each lane.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("5. When you are happy with the settings, press 'Setup done'-button and these " +
            "initial settings are used for creating a set of parallel splines.", EditorStyles.wordWrappedLabel);
        DrawEditorLine();
    }

    private void ShowEditInstructions()
    {
        EditorGUILayout.LabelField("Edit mode instructions:", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void ShowNodeSetupInstructions()
    {
        EditorGUILayout.LabelField("Node set-up instructions:", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void ShowLaneCountSelection()
    {
        EditorGUILayout.LabelField("1. Lanes:", EditorStyles.boldLabel);
        rightLaneCount = EditorGUILayout.IntSlider(new GUIContent("Right lanes count", "0 - 3")
            , rightLaneCount, 0, 3);
        leftLaneCount = EditorGUILayout.IntSlider(new GUIContent("Left lanes count", "opposite direction, 0 - 3")
            , leftLaneCount, 0, 3);
        if (rightLaneCount != 0 || leftLaneCount != 0)
        {
            if (GUILayout.Button("Done"))
            {
                laneCountSet = true;
            }
        }
        DrawEditorLine();
    }

    private void ShowLaneSetupSelection()
    {
        EditorGUILayout.LabelField("2. - 3. Lane spacing and linking",
                EditorStyles.boldLabel);
        ShowRightLaneSelection();
        ShowLeftLaneSelection();
    }

    private void ShowRightLaneSelection()
    {
        if (rightLaneCount > 0)
        {
            EditorGUILayout.LabelField("Right side lanes: " + rightLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < rightLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (right)");
            float sp = EditorGUILayout.Slider("Spacing " + (i + 1), rSpacing[i], 0f, 10f);
            if (sp != rSpacing[i])
            {
                rSpacing[i] = sp;
                SceneView.RepaintAll();
            }
            if (linkedToNode == false)
            {
                Nodes n = null;
                n = EditorGUILayout.ObjectField("Object link", rStart[i], typeof(Nodes), true) as Nodes;
                if (n != null)
                {
                    rStart[i] = n;
                    linkedToNode = true;
                    if (n.OutNodes.Length > 0)
                    {
                        directionVector = (n.OutNodes[0].transform.position - n.transform.position).normalized;
                        directionSet = true;
                    }
                    else
                    {
                        directionSet = false;
                    }
                }
            }
            else
            {
                if (rStart[i] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Linked: " + rStart[i].gameObject.name, EditorStyles.whiteLabel);
                    if (GUILayout.Button("Remove"))
                    {
                        rStart[i] = null;
                        linkedToNode = false;
                        directionVector = GeneralDirection.DirectionVector(generalDirection);
                        SceneView.RepaintAll();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        DrawEditorLine();
    }

    private void ShowLeftLaneSelection()
    {
        if (leftLaneCount > 0)
        {
            EditorGUILayout.LabelField("Left side lanes: " + leftLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < leftLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (left)");
            float sp = EditorGUILayout.Slider("Spacing " + (i + 1), lSpacing[i], 0f, 10f);
            if (sp != lSpacing[i])
            {
                lSpacing[i] = sp;
                SceneView.RepaintAll();
            }
            if (linkedToNode == false)
            {
                Nodes n = null;
                n = EditorGUILayout.ObjectField("Object link", lEnd[i], typeof(Nodes), true) as Nodes;
                if (n)
                {
                    linkedToNode = true;
                    lEnd[i] = n;
                    if (n.OutNodes.Length > 0)
                    {
                        directionVector = (n.transform.position - n.OutNodes[0].transform.position).normalized;
                        SceneView.RepaintAll();
                        directionSet = true;
                    }
                    else
                    {
                        directionSet = false;
                    }
                }
            }
            else
            {
                if (lEnd[i] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Linked: " + lEnd[i].gameObject.name, EditorStyles.whiteLabel);
                    if (GUILayout.Button("Remove"))
                    {
                        lEnd[i] = null;
                        linkedToNode = false;
                        directionVector = GeneralDirection.DirectionVector(generalDirection);
                        SceneView.RepaintAll();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        DrawEditorLine();
    }

    private void ShowSpacingSelection()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("4. Uniform spacing", EditorStyles.boldLabel);

        if (!linkedToNode)
        {
            Directions d = Directions.East;
            d = (Directions) EditorGUILayout.EnumPopup("General direction", generalDirection);
            if (d != generalDirection)
            {
                generalDirection = d;
                directionVector = GeneralDirection.DirectionVector(generalDirection);
                SceneView.RepaintAll();
            }
        }
        float length = EditorGUILayout.Slider(new GUIContent("Length", "Length of (the first) spline"),
        defaultLength, 1f, 100f);
        if (length != defaultLength)
        {
            defaultLength = length;
            SceneView.RepaintAll();
        }
        if (!linkedToNode)
        {
            float adj = EditorGUILayout.Slider(new GUIContent("Adustment", "Adust left-right"),
            adjustment, -5f, 5f);
            if (adj != adjustment)
            {
                adjustment = adj;
                SceneView.RepaintAll();
            }
        }

        spacingOverride = EditorGUILayout.Slider(new GUIContent("Uniform spacing", "Override all spacing values"),
        spacingOverride, 0f, 10f);
        if (GUILayout.Button("Override spacing values"))
        {
            for (int i = 0; i < 3; i++)
            {
                lSpacing[i] = spacingOverride;
                rSpacing[i] = spacingOverride;
                SceneView.RepaintAll();
            }
        }
        DrawEditorLine();
    }

    private void ResetValues()
    {
        for (int i=0; i < 3; i++)
        {
            lSpacing[i] = 0f;
            rSpacing[i] = 0f;
            rStart[i] = null;
            lEnd[i] = null;
            guidePoints[i] = parallel.transform.position;
        }
        guidePoints[3] = parallel.transform.position;
        adjustment = 0f;
        linkedToNode = false;
        laneCountSet = false;
        leftLaneCount = 0;
        rightLaneCount = 0;
        spacingOverride = 1f;
        directionVector = GeneralDirection.DirectionVector(generalDirection);
        directionSet = false;
    }

    private void OnSceneGUI()
    {
        if (parallel != null)
        {
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(parallel.transform.localPosition, new Vector3(0f, 1f, 0f),
                0.01f*Vector3.Distance(parallel.transform.position,
                SceneView.lastActiveSceneView.camera.transform.position));
        }
        else
        {
            parallel = target as ParallelBezierSplines;
        }
        handleTransform = parallel.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        if (!parallel.Initialized)
        {
            DrawBasicSettingsObjects();
        }
        else
        {
            
            DrawParallelBezier();
            DrawSegmentLines();
            if (parallel.LanesSet && previewNodes)
            {
                DrawNodeVisualization();
            }
            if (parallel.NodesSet)
            {
                DrawBusLanes();
            }
        }
    }

    private void DrawParallelBezier()
    {
        Vector3 p0, p1, p2, p3;
        p0 = ShowPoint(0);
        for (int i = 1; i < parallel.ControlPointCount; i +=3)
        {
            p1 = ShowPoint(i);
            p2 = ShowPoint(i + 1);
            p3 = ShowPoint(i + 2);

            Handles.color = Color.red;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.yellow, null, 4f);
            p0 = p3;
        }
        for (int lanes = 0; lanes < parallel.RightLaneCount; lanes++)
        {
            p0 = ShowPointRight(lanes, 0);
            for (int i = 1; i < parallel.ControlPointCount; i += 3)
            {
                p1 = ShowPointRight(lanes, i);
                p2 = ShowPointRight(lanes, i + 1);
                p3 = ShowPointRight(lanes, i + 2);
                Handles.color = Color.magenta;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.blue, null, 2f);
                p0 = p3;
            }
        }

        for (int lanes = 0; lanes < parallel.LeftLaneCount; lanes++)
        {
            p0 = ShowPointLeft(lanes, 0);
            for (int i = 1; i < parallel.ControlPointCount; i += 3)
            {
                p1 = ShowPointLeft(lanes, i);
                p2 = ShowPointLeft(lanes, i + 1);
                p3 = ShowPointLeft(lanes, i + 2);

                Handles.color = Color.magenta;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.red, null, 2f);
                p0 = p3;
            }
        }
    }

    private void DrawSegmentLines()
    {
        Vector3 leftPoint, rightPoint, labelPoint;
        for (int i = 0; i < parallel.ControlPointCount; i += 3)
        {
            int leftI = parallel.ControlPointCount -  1 - i;
            leftPoint = parallel.GetControlPoint(i);
            rightPoint = parallel.GetControlPoint(i);
            labelPoint = parallel.GetControlPoint(i);
            switch (parallel.LeftLaneCount)
            {
                case 1:
                    leftPoint = parallel.GetControlPointLeft(0, leftI);
                    labelPoint = leftPoint;
                    break;
                case 2:
                    leftPoint = parallel.GetControlPointLeft(1, leftI);
                    labelPoint = leftPoint;
                    break;
                case 3:
                    leftPoint = parallel.GetControlPointLeft(2, leftI);
                    labelPoint = leftPoint;
                    break;
            }
            switch (parallel.RightLaneCount)
            {
                case 1:
                    rightPoint = parallel.GetControlPointRight(0, i);
                    break;
                case 2:
                    rightPoint = parallel.GetControlPointRight(1, i);
                    break;
                case 3:
                    rightPoint = parallel.GetControlPointRight(2, i);
                    break;
            }
            labelPoint.x -= 6f;
            labelPoint.z += 1f;
            Handles.color = Color.white;
            Handles.DrawLine(leftPoint, rightPoint);
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(labelPoint, "Pt. " + i/3, style);
        }
    }

    private void DrawNodeVisualization()
    {
        Handles.color = Color.yellow;
        float handleSize = 0.3f;
        Vector3 pos;
        //Start poits for each lane
        for (int i = 0; i < parallel.RightLaneCount; i++)
        {
            pos = parallel.GetRightLaneStartPoint(i);
            Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
        }
        for (int i = 0; i < parallel.LeftLaneCount; i++)
        {
            // points on left lane are inversed for easier handling
            pos = parallel.GetLeftEndPoint(i);
            Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
        }
        for (int seg = 0; seg < parallel.SegmentCount; seg++)
        {
            int lSeg = parallel.SegmentCount - seg - 1;
            int nodesOnSeg = parallel.GetNodesOnSegment(seg);
            //right lanes
            for (int lane = 0; lane < parallel.RightLaneCount; lane++)
            {
                for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointRight(lane, seg, (float)pnt / nodesOnSeg);
                    Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
                }
            }
            //left lanes
            for (int lane = 0; lane < parallel.LeftLaneCount; lane++)
            {
                for (int pnt = 0; pnt < nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointLeft(lane, lSeg, (float)pnt / nodesOnSeg);
                    Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
                }
            }
        }
    }

    private void DrawBusLanes()
    {
        if (parallel.BusLaneRight)
        {
            if (rightBusPoints == null)
            {
                int lane = parallel.RightLaneCount - 1;
                int arraySize = parallel.BusRightEnd - parallel.BusRightStart + 1;
                int pntsLeft = arraySize;
                rightBusPoints = new Vector3[arraySize];
                int index = 0;
                if (parallel.BusRightStart == 0)
                {
                    rightBusPoints[index] = parallel.GetRightLaneStartPoint(lane);
                    pntsLeft--;
                    index++;
                }
                //find start segment and node
                int segment = 0;
                int node = parallel.BusRightStart;
                if (node == 0)
                {
                    node++;
                }
                while (node > parallel.GetNodesOnSegment(segment))
                {
                    node -= parallel.GetNodesOnSegment(segment);
                    segment++;
                }
                int pntsOnSegment = parallel.GetNodesOnSegment(segment);
                while(pntsLeft > 0)
                {
                    if (node > parallel.GetNodesOnSegment(segment))
                    {
                        node -= parallel.GetNodesOnSegment(segment);
                        segment++;
                        pntsOnSegment = parallel.GetNodesOnSegment(segment);
                    }
                    rightBusPoints[index] = parallel.GetSegmentedPointRight(lane, segment, (float)node / pntsOnSegment);
                    pntsLeft--;
                    node++;
                    index++;
                }
            }
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4f, rightBusPoints);
        }
        if (parallel.BusLaneLeft)
        {
            if (leftBusPoints == null)
            {
                int lane = parallel.LeftLaneCount - 1;
                int arraySize = parallel.BusLeftEnd - parallel.BusLeftStart + 1;
                int pntsLeft = arraySize;
                leftBusPoints = new Vector3[arraySize];
                int index = 0;
                if (parallel.BusLeftStart == 0)
                {
                    leftBusPoints[index] = parallel.GetLeftLaneStartPoint(lane);
                    pntsLeft--;
                    index++;
                }
                //find start segment and node
                int segment = 0;
                int node = parallel.BusLeftStart;
                if (node == 0)
                {
                    node++;
                }
                while (node > parallel.GetNodesOnSegment(segment))
                {
                    node -= parallel.GetNodesOnSegment(segment);
                    segment++;
                }
                int pntsOnSegment = parallel.GetNodesOnSegment(segment);
                while (pntsLeft > 0)
                {
                    if (node > parallel.GetNodesOnSegment(segment))
                    {
                        node -= parallel.GetNodesOnSegment(segment);
                        segment++;
                        pntsOnSegment = parallel.GetNodesOnSegment(segment);
                    }
                    leftBusPoints[index] = parallel.GetSegmentedPointLeft(lane, segment, (float)node / pntsOnSegment);
                    pntsLeft--;
                    node++;
                    index++;
                }
            }
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4f, leftBusPoints);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(parallel.GetControlPoint(index));

        if (advancedOptionsOn || parallel.LanesSet)
        {
            return point;
        }
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
        {
            size *= 2f; // 1st node bigger
        }
        Handles.color = Color.cyan;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint(); // refresh inspector
        }
        if (selectedIndex == index && index != 0)
        {
            Event e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            Vector3 dRight = GeneralDirection.DirectionRight(parallel.GetDirection((float)index / parallel.CurveCount));
            Vector3 p = handleTransform.InverseTransformPoint(point);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(parallel, "MovePoint");
                EditorUtility.SetDirty(parallel);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Vector3[] leftMoves = new Vector3[3];
                Vector3[] rightMoves = new Vector3[3];

                int leftIndex = parallel.ControlPointCount - 1 - index;
                leftMoves[0] = parallel.GetControlPointLeft(0, leftIndex) - parallel.GetControlPoint(index);
                leftMoves[1] = parallel.GetControlPointLeft(1, leftIndex) - parallel.GetControlPoint(index);
                leftMoves[2] = parallel.GetControlPointLeft(2, leftIndex) - parallel.GetControlPoint(index);
                rightMoves[0] = parallel.GetControlPointRight(0, index) - parallel.GetControlPoint(index);
                rightMoves[1] = parallel.GetControlPointRight(1, index) - parallel.GetControlPoint(index);
                rightMoves[2] = parallel.GetControlPointRight(2, index) - parallel.GetControlPoint(index);

                parallel.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
                parallel.RecalculateLength(selectedIndex);

                if (index % 3 != 0)
                {
                    leftMoves[0] -= parallel.GetControlPointLeft(0, leftIndex);
                    leftMoves[0] += parallel.GetControlPoint(index);
                    leftMoves[1] -= parallel.GetControlPointLeft(1, leftIndex);
                    leftMoves[1] += parallel.GetControlPoint(index);
                    leftMoves[2] -= parallel.GetControlPointLeft(2, leftIndex);
                    leftMoves[2] += parallel.GetControlPoint(index);
                    rightMoves[0] -= parallel.GetControlPointRight(0, index);
                    rightMoves[0] += parallel.GetControlPoint(index);
                    rightMoves[1] -= parallel.GetControlPointRight(1, index);
                    rightMoves[1] += parallel.GetControlPoint(index);
                    rightMoves[2] -= parallel.GetControlPointRight(2, index);
                    rightMoves[2] += parallel.GetControlPoint(index);
                }

                for (int i = 0; i < parallel.RightLaneCount; i++)
                {
                    
                    parallel.AdjustControlPointRight(i, index, rightMoves[i]);
                }

                for (int i = 0; i < parallel.LeftLaneCount; i++)
                {
                    parallel.AdjustControlPointLeft(i, index, leftMoves[i]);
                }
            }
        }
        return point;
    }

    private Vector3 ShowPointRight(int lane, int index)
    {
        Vector3 point = handleTransform.TransformPoint(parallel.GetControlPointRight(lane, index));
        if (!advancedOptionsOn)
        {
            return point;
        }
        if (selected[lane])
        {
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f; // 1st node bigger
            }
            Handles.color = Color.cyan;
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = index;
                Repaint(); // refresh inspector
            }
            if (selectedIndex == index && index != 0 && selectedIndex % 3 != 0)
            {
                Event e = Event.current;
                var controlID = GUIUtility.GetControlID(FocusType.Passive);
                var eventType = e.GetTypeForControl(controlID);

                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);
                Vector3 dRight = GeneralDirection.DirectionRight(parallel.GetDirectionRight(lane, (float)index / parallel.CurveCount));
                Vector3 p = handleTransform.InverseTransformPoint(point);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(parallel, "MovePoint");
                    EditorUtility.SetDirty(parallel);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Vector3[] leftMoves = new Vector3[3];
                    Vector3[] rightMoves = new Vector3[3];

                    int leftIndex = parallel.ControlPointCount - 1 - index;
                    leftMoves[0] = parallel.GetControlPointLeft(0, leftIndex) - parallel.GetControlPointRight(lane, index);
                    leftMoves[1] = parallel.GetControlPointLeft(1, leftIndex) - parallel.GetControlPointRight(lane, index);
                    leftMoves[2] = parallel.GetControlPointLeft(2, leftIndex) - parallel.GetControlPointRight(lane, index);
                    rightMoves[0] = parallel.GetControlPointRight(0, index) - parallel.GetControlPointRight(lane, index);
                    rightMoves[1] = parallel.GetControlPointRight(1, index) - parallel.GetControlPointRight(lane, index);
                    rightMoves[2] = parallel.GetControlPointRight(2, index) - parallel.GetControlPointRight(lane, index);

                    parallel.SetControlPointRight(lane, index, handleTransform.InverseTransformPoint(point));
                    parallel.RecalculateLengthRight(lane, selectedIndex);
                    if (index % 3 != 0)
                    {
                        leftMoves[0] -= parallel.GetControlPointLeft(0, leftIndex);
                        leftMoves[0] += parallel.GetControlPointRight(lane, index);
                        leftMoves[1] -= parallel.GetControlPointLeft(1, leftIndex);
                        leftMoves[1] += parallel.GetControlPointRight(lane, index);
                        leftMoves[2] -= parallel.GetControlPointLeft(2, leftIndex);
                        leftMoves[2] += parallel.GetControlPointRight(lane, index);
                        rightMoves[0] -= parallel.GetControlPointRight(0, index);
                        rightMoves[0] += parallel.GetControlPointRight(lane, index);
                        rightMoves[1] -= parallel.GetControlPointRight(1, index);
                        rightMoves[1] += parallel.GetControlPointRight(lane, index);
                        rightMoves[2] -= parallel.GetControlPointRight(2, index);
                        rightMoves[2] += parallel.GetControlPointRight(lane, index);
                    }

                    for (int i = 0; i < parallel.RightLaneCount; i++)
                    {
                        if (lane != i && selected[i]==true)
                        {
                            parallel.AdjustControlPointRight(i, index, rightMoves[i]);
                        }
                    }

                    for (int i = 0; i < parallel.LeftLaneCount; i++)
                    {
                        if (selected[i + 3])
                        {
                            parallel.AdjustControlPointLeft(i, index, leftMoves[i]);
                        }
                    }
                }
            }
        }

        return point;
    }

    private Vector3 ShowPointLeft(int lane, int index)
    {
        int leftIndex = parallel.ControlPointCount - 1 - index;
        Vector3 point = handleTransform.TransformPoint(parallel.GetControlPointLeft(lane, leftIndex));
        if (!advancedOptionsOn)
        {
            return point;
        }
        //*****************
        if (selected[lane + 3])
        {
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f; // 1st node bigger
            }
            Handles.color = Color.cyan;
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = leftIndex;
                Repaint(); // refresh inspector
            }
            
            if (selectedIndex == leftIndex && selectedIndex % 3 != 0)
            {
                Event e = Event.current;
                var controlID = GUIUtility.GetControlID(FocusType.Passive);
                var eventType = e.GetTypeForControl(controlID);

                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);
                Vector3 dRight = GeneralDirection.DirectionRight(parallel.GetDirectionLeft(lane, (float)selectedIndex / parallel.CurveCount));
                Vector3 p = handleTransform.InverseTransformPoint(point);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(parallel, "MovePoint");
                    EditorUtility.SetDirty(parallel);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Vector3[] leftMoves = new Vector3[3];
                    Vector3[] rightMoves = new Vector3[3];

                    leftMoves[0] = parallel.GetControlPointLeft(0, leftIndex) - parallel.GetControlPointLeft(lane, selectedIndex);
                    leftMoves[1] = parallel.GetControlPointLeft(1, leftIndex) - parallel.GetControlPointLeft(lane, selectedIndex);
                    leftMoves[2] = parallel.GetControlPointLeft(2, leftIndex) - parallel.GetControlPointLeft(lane, selectedIndex);
                    rightMoves[0] = parallel.GetControlPointRight(0, index) - parallel.GetControlPointLeft(lane, selectedIndex);
                    rightMoves[1] = parallel.GetControlPointRight(1, index) - parallel.GetControlPointLeft(lane, selectedIndex);
                    rightMoves[2] = parallel.GetControlPointRight(2, index) - parallel.GetControlPointLeft(lane, selectedIndex);

                    parallel.SetControlPointLeft(lane, selectedIndex, handleTransform.InverseTransformPoint(point));
                    parallel.RecalculateLengthLeft(lane, selectedIndex);
                    if (index % 3 != 0)
                    {
                        leftMoves[0] -= parallel.GetControlPointLeft(0, leftIndex);
                        leftMoves[0] += parallel.GetControlPointLeft(lane, leftIndex);
                        leftMoves[1] -= parallel.GetControlPointLeft(1, leftIndex);
                        leftMoves[1] += parallel.GetControlPointLeft(lane, leftIndex);
                        leftMoves[2] -= parallel.GetControlPointLeft(2, leftIndex);
                        leftMoves[2] += parallel.GetControlPointLeft(lane, leftIndex);
                        rightMoves[0] -= parallel.GetControlPointRight(0, index);
                        rightMoves[0] += parallel.GetControlPointLeft(lane, leftIndex);
                        rightMoves[1] -= parallel.GetControlPointRight(1, index);
                        rightMoves[1] += parallel.GetControlPointLeft(lane, leftIndex);
                        rightMoves[2] -= parallel.GetControlPointRight(2, index);
                        rightMoves[2] += parallel.GetControlPointLeft(lane, leftIndex);
                    }

                    for (int i = 0; i < parallel.RightLaneCount; i++)
                    {
                        if (selected[i])
                        {
                            parallel.AdjustControlPointRight(i, index, rightMoves[i]);
                        }
                    }

                    for (int i = 0; i < parallel.LeftLaneCount; i++)
                    {
                        if (lane != i && selected[i + 3])
                        {
                            parallel.AdjustControlPointLeft(i, index, leftMoves[i]);
                        }
                    }
                }
            }
        }
        //*******************
        
        return point;
    }


    private void DrawBasicSettingsObjects()
    {
        if (allNodes == null)
        {
            allNodes = GameObject.FindObjectsOfType<Nodes>();
        }
        if (linkedToNode == false && laneCountSet)
        {
            foreach (Nodes n in allNodes)
            {
                Handles.Label(n.transform.position, n.gameObject.name);
            }
            SceneView.RepaintAll();
        }
        if (!directionSet)
        {
            SetStartDirection();
        }
        SetStartPositions();
        Handles.DrawBezier(guidePoints[0], guidePoints[3], guidePoints[1], guidePoints[2], Color.yellow, null, 4f);
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        float dist = 0f;
        Vector3 p0, p1, p2, p3;
        for (int i = 0; i < rightLaneCount; i++)
        {
            dist += rSpacing[i];
            p0 = guidePoints[0] + dist * right;
            p1 = guidePoints[1] + dist * right;
            p2 = guidePoints[2] + dist * right;
            p3 = guidePoints[3] + dist * right;
            Handles.DrawBezier(p0, p3, p1, p2, Color.blue, null, 2f);
        }
        dist = 0f;
        for (int i = 0; i < leftLaneCount; i++)
        {
            dist += lSpacing[i];
            p0 = guidePoints[0] - dist * right;
            p1 = guidePoints[1] - dist * right;
            p2 = guidePoints[2] - dist * right;
            p3 = guidePoints[3] - dist * right;
            Handles.DrawBezier(p0, p3, p1, p2, Color.red, null, 2f);
        }

    }

    private void SetStartDirection()
    {
        if (linkedToNode)
        {
            for (int i = 0; i < rightLaneCount; i++)
            {
                if (rStart[i] != null)
                {
                    directionSet = true;
                    Vector3 dir;
                    bool hasDir = rStart[i].GetDirectionIn(out dir);
                    if (hasDir)
                    {
                        directionVector = dir;
                    }
                    break;
                }
            }
            if (!directionSet)
            {
                for (int i = 0; i < leftLaneCount; i++)
                {
                    if (lEnd[i] != null)
                    {
                        if (lEnd[i].OutNodes.Length > 0)
                        {
                            if (lEnd[i].GetDirectionOut(out Vector3 dir))
                            {
                                directionVector = dir;
                                directionSet = true;
                            }
                        }
                        break;
                    }
                }
            }
        }
        if (!directionSet)
        {
            directionVector = GeneralDirection.DirectionVector(generalDirection);
        }
    }

    private void SetStartPositions()
    {
        if (parallel == null)
        {
            parallel = target as ParallelBezierSplines;
        }
        if (guidePoints == null || guidePoints.Length != 4)
        {
            Vector3 pos = parallel.transform.position;
            guidePoints = new Vector3[] { pos, pos, pos, pos };
        }
        Vector3 left = new Vector3(-directionVector.z, directionVector.y, directionVector.x);
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        if (linkedToNode)
        {
            bool found = false;
            float dist = 0f;
            for (int i = 0; i < rightLaneCount; i++)
            {
                dist += rSpacing[i];
                if (rStart[i]!=null)
                {
                    guidePoints[0] = rStart[i].transform.position + left * dist;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                dist = 0f;
                for (int i = 0; i < leftLaneCount; i++)
                {
                    dist += lSpacing[i];
                    if (lEnd[i] != null)
                    {
                        guidePoints[0] = lEnd[i].transform.position + right * dist;
                        break;
                    }
                }
            }            
        }
        else
        {
            guidePoints[0] = parallel.transform.position + adjustment * right;
        }
        guidePoints[1] = guidePoints[0] + directionVector * (defaultLength / 3f);
        guidePoints[2] = guidePoints[1] + directionVector * (defaultLength / 3f);
        guidePoints[3] = guidePoints[2] + directionVector * (defaultLength / 3f);

    }

    private void OnEnable()
    {
        Tools.current = Tool.View;
        Tools.hidden = true;
        SetCameraAngle();
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void SetCameraAngle()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (parallel == null)
        {
            parallel = target as ParallelBezierSplines;
        }
        sceneView.AlignViewToObject(parallel.transform);
        sceneView.LookAtDirect(parallel.transform.position, Quaternion.Euler(90, 0, 0), 30f);
        sceneView.orthographic = true;
    }
}
