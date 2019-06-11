using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Intersection))]
public class IntersectionInspector : Editor
{
    private Intersection intersection;
    private GameObject roadNetwork;
    // naming
    private bool nameAutoChecked = true;
    private bool nameIsValid = false;
    private string intersectionName = "";
    private string renameInfo = "";

    // framing
    private Vector3[] framingHorizontal;
    private Vector3[] framingVertical;
    private float step = 1f;

    //positioning line;
    Vector3 linePos;
    Vector3[] linePoints;
    float lineLength;
    Vector3 lineDir;
    bool showLine = false;
    float lineYAngle = 0f;
    float lineAngle = 5f;
    int nodesOnLine = 0;
    float[] nodePlaces;
    float[] tempPlaces;
    NodeInOut[] lineNodesInOut;

    private List<Vector3> pointsToDraw;
    private List<Color> pointsToDrawColors;

    private bool addingNodes = false;
    private bool confirming = false;

    public List<Vector3> inPositions;
    public List<Vector3> outPositions;
    public int inOutCount;
    public int inIndex = 0;
    public int outIndex = 0;

    //*************************** INSPECTOR START

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(intersection, "changed");
        NameMenu();
        if (intersection.allNodesSet)
        {
            SplineSetupMenu();
        }
        else
        {
            if (!intersection.framed)
            {
                FramingMenu();
            }
            else
            {
                SetupMenu();
            }
        }
        base.OnInspectorGUI();
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
            if (intersection.roadNetwork != null)
            {
                roadNetwork = intersection.roadNetwork;
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
                    intersection.roadNetwork = g;
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
                            intersection.roadNetwork = networks[i].gameObject;
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
            EditorGUILayout.LabelField("Intersection name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(intersection.gameObject.name);
            EditorGUILayout.EndHorizontal();
            if (!nameAutoChecked)
            {
                nameAutoChecked = true;
                nameIsValid = CheckName(intersection.gameObject.name);
                if (nameIsValid)
                {
                    intersectionName = intersection.gameObject.name;
                }
            }
            if (nameIsValid)
            {
                ItalicLabel("Name is valid");
            }
            else
            {
                intersectionName = "";
                WarningLabel("Invalid name. Name already exists.");
            }
            intersectionName = GUILayout.TextField(intersectionName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter name");
            if (GUILayout.Button("Rename"))
            {
                bool valid = CheckName(intersectionName);
                if (!valid)
                {
                    renameInfo = "New name was not valid.";
                }
                else
                {
                    renameInfo = "Name changed to '" + intersectionName + "'.";
                    intersection.gameObject.name = intersectionName;
                    nameIsValid = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            ItalicLabel(renameInfo);

            EditorGUILayout.Separator();
        }
        EditorGUILayout.LabelField("Traffic", EditorStyles.boldLabel);
        intersection.Traffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic size", intersection.Traffic);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Overall speed limit", EditorStyles.boldLabel);
        intersection.SpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Limit", intersection.SpeedLimit);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void FramingMenu()
    {
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);
        step = EditorGUILayout.FloatField("Step:", step);
        EditorGUILayout.Separator();
        // centerpoint
        EditorGUILayout.LabelField("Adjust centerpoint", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, -step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            intersection.CenterPoint += new Vector3(-step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            intersection.CenterPoint += new Vector3(step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        // bounding box
        EditorGUILayout.LabelField("Adjust bounding box", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Width+"))
        {
            intersection.FrameWidth += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Width-"))
        {
            float width = intersection.FrameWidth - step;
            if (width > 0f)
            {
                intersection.FrameWidth = width;
            }
            else
            {
                intersection.FrameWidth = 0.1f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height+"))
        {
            intersection.FrameHeight += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height-"))
        {
            float height = intersection.FrameHeight - step;
            if (height > 0f)
            {
                intersection.FrameHeight = height;
            }
            else
            {
                intersection.FrameHeight = 0f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Framing done"))
        {
            intersection.framed = true;
            UpdateNodesInBox();
        }
    }

    private void SetupMenu()
    {
        if (GUILayout.Button("Back to framing"))
        {
            intersection.framed = false;
        }
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Setup Menu", EditorStyles.boldLabel);

        if (intersection.GetInfoSize() > 0)
        {
            if (!addingNodes)
            {
                if (GUILayout.Button("Open node options"))
                {
                    addingNodes = true;
                    intersection.SetInfoIndexToFirst();
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (GUILayout.Button("Hide node options"))
                {
                    addingNodes = false;
                }
            }
        }
        if (addingNodes)
        {
            EntryNodesSetupMenu();
        }
        else
        {
            EntryUsingGuideLine();
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
        if (!confirming)
        {
            if (GUILayout.Button("Nodes set, start drawing lanes"))
            {
                confirming = true;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Are you sure?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
                inIndex = 0;
                outIndex = 0;
                intersection.allNodesSet = true;
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("No"))
            {
                confirming = false;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void EntryNodesSetupMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Select nodes", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (intersection.GetInfoSize() == 0)
        {
            ItalicLabel("There are no nodes in the selected area.");
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                intersection.MoveInfoIndex(-1);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Next"))
            {
                intersection.MoveInfoIndex(1);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            Nodes n = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            if (inOut == NodeInOut.NotUsed)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set as in-node"))
                {
                    intersection.SetInOut(NodeInOut.InNode);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Set as out-node"))
                {
                    intersection.SetInOut(NodeInOut.OutNode);
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (n.ParallelLeft || n.ParallelRight)
                {
                    if (GUILayout.Button("Select adjacents also"))
                    {
                        intersection.SelectAdjacents();
                        SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Deselect"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Deselect this and adjacents"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    intersection.SelectAdjacents();
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Deselect all"))
            {
                intersection.SetInOutAll(NodeInOut.NotUsed);
                SceneView.RepaintAll();
            }
        }
    }

    private void EntryUsingGuideLine()
    {
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Create new entry points", EditorStyles.boldLabel);
        if (showLine == false)
        {
            if (GUILayout.Button("Create new"))
            {
                linePos = intersection.CenterPoint;
                lineLength = intersection.FrameHeight * 0.5f;
                Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
                Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
                linePoints = new Vector3[] { p0, p1 };
                lineDir = (p1 - p0).normalized;
                showLine = true;
                nodesOnLine = 0;
                SceneView.RepaintAll();
            }
        }
        else
        {
            LinePlacementMenu();
            if (GUILayout.Button("Cancel"))
            {
                showLine = false;
                SceneView.RepaintAll();
            }
        }
    }

    private void SplineSetupMenu()
    {
        //in nodes
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            int val = inIndex - 1;
            if (val < 0)
            {
                inIndex = inPositions.Count - 1;
            }
            else
            {
                inIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            int val = inIndex + 1;
            if (val > inPositions.Count -1)
            {
                inIndex = 0;
            }
            else
            {
                inIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        //out nodes
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            int val = outIndex - 1;
            if (val < 0)
            {
                outIndex = outPositions.Count - 1;
            }
            else
            {
                outIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            int val = outIndex + 1;
            if (val > outPositions.Count - 1)
            {
                outIndex = 0;
            }
            else
            {
                outIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void LinePlacementMenu()
    {
        EditorGUILayout.Separator();
        step = EditorGUILayout.FloatField("Step", step);
        EditorGUILayout.LabelField("Adjust position", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            linePos.z += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            linePos.z -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            linePos.x -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            linePos.x += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust size", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Line size -"))
        {
            if (lineLength - step > 1f)
            {
                lineLength -= step;
            }
            else
            {
                lineLength = 1f;
            }
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Line size +"))
        {
            lineLength += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust angle", EditorStyles.boldLabel);
        float angle = lineAngle;
        angle = EditorGUILayout.FloatField("Angle", angle);
        {
            if (angle != lineAngle)
            {
                lineAngle = angle % 360f;
            }
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-"))
        {
            lineYAngle = (360f + lineYAngle - lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("+"))
        {
            lineYAngle = (360f + lineYAngle + lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        int nodes = nodesOnLine;
        nodes = EditorGUILayout.IntField("Nodes (" + nodesOnLine + ")", nodesOnLine);
        if (nodes != nodesOnLine)
        {
            if (nodes > -1 && nodes < 7)
            {
                nodesOnLine = nodes;
            }
        }
        if (nodesOnLine > 0)
        {
            bool changed = false;
            if (nodePlaces == null || nodePlaces.Length != 6)
            {
                changed = true;
                nodePlaces = new float[6];
            }
            if (lineNodesInOut == null || lineNodesInOut.Length != 6)
            {
                changed = true;
                lineNodesInOut = new NodeInOut[6];
                for (int i = 0; i < 6; i++)
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
            }
            if (tempPlaces == null || tempPlaces.Length != 6)
            {
                changed = true;
                tempPlaces = new float[6];
            }
            if (changed)
            {
                SetupLineNodes();
            }
        }
        if (nodesOnLine > 0)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Node placement on line (0-1)", EditorStyles.boldLabel);
        }
        for (int i = 0; i < nodesOnLine; i++)
        {
            tempPlaces[i] = EditorGUILayout.FloatField("" + (i+1) + " (" + nodePlaces[i] + ")", tempPlaces[i]);
            if (GUILayout.Button("Set"))
            {
                if (tempPlaces[i] != nodePlaces[i])
                {
                    bool isOk = true;
                    if (i == 0)
                    {
                        if (tempPlaces[i] < 0f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] <= nodePlaces[i - 1])
                        {
                            isOk = false;
                        }
                    }
                    if (i == nodesOnLine - 1)
                    {
                        if (tempPlaces[i] > 1f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] >= nodePlaces[i + 1])
                        {
                            isOk = false;
                        }
                    }
                    if (isOk)
                    {
                        nodePlaces[i] = tempPlaces[i];
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        tempPlaces[i] = nodePlaces[i];
                    }
                }
            }

            bool isOut = false;
            if (lineNodesInOut[i] == NodeInOut.OutNode)
            {
                isOut = true;
            }
            bool check = isOut;
            isOut = EditorGUILayout.ToggleLeft("is out node?", isOut);
            if (isOut != check)
            {
                if (isOut)
                {
                    lineNodesInOut[i] = NodeInOut.OutNode;
                }
                else
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.Separator();
        if (nodesOnLine > 0)
        {
            if (GUILayout.Button("Done"))
            {
                SaveHelperLine();
                showLine = false;
                ResetLineValues();
                UpdatePointsToDraw();
                SceneView.RepaintAll();
            }
        }
    }

    private void SaveHelperLine()
    {
        HelperLine h = new HelperLine();
        h.startPoint = linePoints[0];
        h.direction = lineDir;
        h.lenght = lineLength;
        List<float> pnts = new List<float>();
        List<NodeInOut> ios = new List<NodeInOut>();
        for (int i = 0; i < nodesOnLine; i++)
        {
            pnts.Add(nodePlaces[i]);
            ios.Add(lineNodesInOut[i]);
        }
        h.nodePoints = pnts;
        h.inOut = ios;
        intersection.helperLines.Add(h);
    }

    private void ResetLineValues()
    {
        nodePlaces = null;
        tempPlaces = null;
        lineNodesInOut = null;
        lineYAngle = 0f;
        lineAngle = 5f;
    }

    private void UpdatePointsToDraw()
    {
        pointsToDraw = new List<Vector3>();
        pointsToDrawColors = new List<Color>();
        for (int i = 0; i < intersection.helperLines.Count; i++)
        {
            HelperLine h = intersection.helperLines[i];
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            Vector3 p0 = h.startPoint;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                Color c = Color.blue;
                if (h.inOut[j] == NodeInOut.OutNode)
                {
                    c = Color.red;
                }
                pointsToDraw.Add(pnt);
                pointsToDrawColors.Add(c);
            }
        }
    }

    private void SetupLineNodes()
    {
        switch (nodesOnLine)
        {
            case 1:
                nodePlaces[0] = 0.5f;
                nodePlaces[1] = 1.0f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 2:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.75f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 3:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.5f;
                nodePlaces[2] = 0.75f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 4:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.4f;
                nodePlaces[2] = 0.6f;
                nodePlaces[3] = 0.8f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 5:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.35f;
                nodePlaces[2] = 0.5f;
                nodePlaces[3] = 0.65f;
                nodePlaces[4] = 0.8f;
                nodePlaces[5] = 1.0f;
                break;
            case 6:
                nodePlaces[0] = 0.125f;
                nodePlaces[1] = 0.275f;
                nodePlaces[2] = 0.425f;
                nodePlaces[3] = 0.575f;
                nodePlaces[4] = 0.725f;
                nodePlaces[5] = 0.875f;
                break;
        }
        tempPlaces[0] = nodePlaces[0];
        tempPlaces[1] = nodePlaces[1];
        tempPlaces[2] = nodePlaces[2];
        tempPlaces[3] = nodePlaces[3];
        tempPlaces[4] = nodePlaces[4];
        tempPlaces[5] = nodePlaces[5];
    }

    private void EntryHelp()
    {
        EditorGUILayout.LabelField("Entry direction means here a road connecting to this" +
            " intersection. It may consist of multiple lanes going both directions.", EditorStyles.wordWrappedLabel);
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

    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField(message, gs);
    }

    private void WarningLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.normal.textColor = Color.red;
        EditorGUILayout.LabelField(message, gs);
    }

    //************************ SCENE VIEW START


    private void OnSceneGUI()
    {
        Handles.color = Color.white;
        Handles.DrawLines(framingHorizontal);
        Handles.DrawLines(framingVertical);
        HighlightNodes();
        if (showLine)
        {
            ShowMarkerLine();
            if (nodesOnLine > 0)
            {
                ShowNodePlacesOnLine();
            }
        }
        if (pointsToDraw != null || pointsToDraw.Count > 0)
        {
            ShowSavedHelperPoints();
        }
        if (intersection.allNodesSet)
        {
            if (inPositions == null || outPositions == null)
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
                inIndex = 0;
                outIndex = 0;
            }
            ShowNodeNumbers();
        }
    }

    private void DrawSceneDisc(GameObject targetObject, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(targetObject.transform.position, new Vector3(0f, 1f, 0f),
            m * Vector3.Distance(targetObject.transform.position,
            SceneView.lastActiveSceneView.camera.transform.position));
    }

    private void DrawSceneDisc(Vector3 pos, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(pos, new Vector3(0f, 1f, 0f), m * Vector3.Distance(
            pos, SceneView.lastActiveSceneView.camera.transform.position));
    }

    private void ShowNodeNumbers()
    {
        for (int i = 0; i < inPositions.Count; i++)
        {
            Handles.Label(inPositions[i], "In " + i);
        }

        for (int i = 0; i < outPositions.Count; i++)
        {
            Handles.Label(outPositions[i], "Out " + i);
        }
    }

    private void ShowMarkerLine()
    {
        Handles.color = Color.cyan;
        Handles.DrawLine(linePoints[0], linePoints[1]);
    }

    private void ShowNodePlacesOnLine()
    {
        for (int i = 0; i < nodesOnLine; i++)
        {
            Vector3 pos = linePoints[0] + nodePlaces[i] * lineLength * lineDir;
            if (lineNodesInOut[i] == NodeInOut.InNode)
            {
                DrawSceneDisc(pos, Color.blue, false);
            }
            else
            {
                DrawSceneDisc(pos, Color.red, false);
            }
        }
    }

    private void ShowSavedHelperPoints()
    {
        for (int i = 0; i < pointsToDraw.Count; i++)
        {
            DrawSceneDisc(pointsToDraw[i], pointsToDrawColors[i], false);
        }
    }

    private void HighlightNodes()
    {
        if (!intersection.allNodesSet)
        {
            Nodes selected = null;
            if (intersection.GetInfoIndex >= 0)
            {
                selected = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            }
            if (selected != null)
            {
                DrawSceneDisc(selected.gameObject, Color.yellow, true);
            }
        }
        else
        {
            Vector3 pos = inPositions[inIndex];
            DrawSceneDisc(pos, Color.yellow, true);

            pos = outPositions[outIndex];
            DrawSceneDisc(pos, Color.magenta, true);
        }
        int index = intersection.GetInfoIndex;
        if (intersection.nodesInBox != null)
        {
            for (int i = 0; i < intersection.nodesInBox.Length; i++)
            {
                NodeInfo ni = intersection.nodesInBox[i];
                if (ni.inOut == NodeInOut.InNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.blue, false);
                }
                else if (ni.inOut == NodeInOut.OutNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.red, false);
                }
            }
        }
    }

    private void RotateLine (bool clockwise)
    {
        float angle = lineAngle;
        if (!clockwise)
        {
            angle = -angle;
        }
        Vector3 dir0 = linePoints[0] - linePos;
        Vector3 dir1 = linePoints[1] - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, angle, 0f))*dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, angle, 0f)) * dir1;
        linePoints[0] = dir0 + linePos;
        linePoints[1] = dir1 + linePos;
    }

    private void UpdateLinePosition()
    {
        Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
        Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
        //rotation
        Vector3 dir0 = p0 - linePos;
        Vector3 dir1 = p1 - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir1;
        p0 = dir0 + linePos;
        p1 = dir1 + linePos;

        linePoints = new Vector3[] { p0, p1 };
        bool needToAdjust = CheckLinePointsInBounds();
        lineDir = (linePoints[1] - linePoints[0]).normalized;
        if (needToAdjust)
        {
            lineLength = Vector3.Distance(linePoints[0], linePoints[1]);
            linePos = linePoints[0] + lineDir * lineLength * 0.5f;
        }
    }

    private bool CheckLinePointsInBounds()
    {
        float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
        float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
        float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
        float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

        bool needToAdjust = false;

        if (linePoints[0].x < minX)
        {
            needToAdjust = true;
            linePoints[0].x = minX;
        }
        else if (linePoints[0].x > maxX)
        {
            needToAdjust = true;
            linePoints[0].x = maxX;
        }
        if (linePoints[1].x < minX)
        {
            needToAdjust = true;
            linePoints[1].x = minX;
        }
        else if (linePoints[1].x > maxX)
        {
            needToAdjust = true;
            linePoints[1].x = maxX;
        }
        if (linePoints[0].z < minZ)
        {
            needToAdjust = true;
            linePoints[0].z = minZ;
        }
        else if (linePoints[0].z > maxZ)
        {
            needToAdjust = true;
            linePoints[0].z = maxZ;
        }
        if (linePoints[1].z < minZ)
        {
            needToAdjust = true;
            linePoints[1].z = minZ;
        }
        else if (linePoints[1].z > maxZ)
        {
            needToAdjust = true;
            linePoints[1].z = maxZ;
        }

        return needToAdjust;
    }

    private void UpdateFramingBox()
    {
        Vector3 corner1 = intersection.CenterPoint;
        corner1 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner2 = intersection.CenterPoint;
        corner2 += new Vector3(intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner3 = intersection.CenterPoint;
        corner3 += new Vector3(intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        Vector3 corner4 = intersection.CenterPoint;
        corner4 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        framingHorizontal = new Vector3[] { corner1, corner2, corner3, corner4 };
        framingVertical = new Vector3[] { corner4, corner1, corner3, corner2 };
    }

    private void UpdateNodesInBox()
    {
        if (!intersection.nodesInBoxSet)
        {
            float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
            float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
            float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
            float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

            List<Nodes> nodes = new List<Nodes>();
            Nodes[] allNodes = GameObject.FindObjectsOfType<Nodes>();
            for (int i = 0; i < allNodes.Length; i++)
            {
                float nodeX = allNodes[i].gameObject.transform.position.x;
                float nodeZ = allNodes[i].gameObject.transform.position.z;
                if (nodeX > minX && nodeX < maxX && nodeZ > minZ && nodeZ < maxZ)
                {
                    nodes.Add(allNodes[i]);
                }
            }
            NodeInfo[] nInfo = new NodeInfo[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                nInfo[i].node = nodes[i];
                nInfo[i].inOut = NodeInOut.NotUsed;
            }
            intersection.nodesInBox = nInfo;
        }
    }
    //************************ ENABLE / DISABLE

    private void DisableTools()
    {
        Tools.current = Tool.View;
        Tools.hidden = true;
    }
    
    private void OnEnable()
    {
        intersection = target as Intersection;
        UpdateFramingBox();

        if (intersection.framed)
        {
            Tools.current = Tool.View;
            Tools.hidden = true;
            if (intersection.nodesInBox == null)
            {
                UpdateNodesInBox();
            }
        }
        UpdatePointsToDraw();

        if (intersection.allNodesSet)
        {
            inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
            inIndex = 0;
            outIndex = 0;
        }
        SetCameraAngle();
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void SetCameraAngle()
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.AlignViewToObject(intersection.transform);
        sceneView.LookAtDirect(intersection.transform.position, Quaternion.Euler(90, 0, 0), 30f);
        sceneView.orthographic = true;
    }
}
