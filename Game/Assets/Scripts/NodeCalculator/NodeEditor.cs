using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow {

    private List<BaseNode> windows = new List<BaseNode>();

    private Vector2 mousePos;
    private BaseNode selectedNode;
    private bool makeTransitionMode = false;
    public Vector2 scrollPos = Vector2.zero;
    public Rect workSpace = new Rect(0, 0, 1000, 1000);

    [MenuItem("Metropolia/ NodeEditor")]
    static void ShowEditor()
    {
        NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
    }

    private void OnGUI()
    {
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, workSpace);
        Event e = Event.current;
        mousePos = e.mousePosition;
        ShrinkArea();

        if (e.button == 1 && !makeTransitionMode)
        {
            if (e.type == EventType.MouseDown)
            {
                int selectIndex = SelectedWindow();

                GenericMenu menu = new GenericMenu();

                if (selectIndex == -1)
                {
                    menu.AddItem(new GUIContent("Add Input Node"), false, ContextCallback, "inputNode");
                    menu.AddItem(new GUIContent("Add Output Node"), false, ContextCallback, "outputNode");
                    menu.AddItem(new GUIContent("Add Calculation Node"), false, ContextCallback, "calcNode");
                    menu.AddItem(new GUIContent("Add Comparison Node"), false, ContextCallback, "compNode");
                } else
                {
                    menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, "makeTransition");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");

                }

                menu.ShowAsContext();
                e.Use();
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && makeTransitionMode)
        {
            int selectIndex = SelectedWindow();
            if (selectIndex != -1 && !windows[selectIndex].Equals(selectedNode))
            {
                windows[selectIndex].SetInput((BaseInputNode)selectedNode, mousePos);
                makeTransitionMode = false;
                selectedNode = null;
            }

            if (selectIndex == -1)
            {
                makeTransitionMode = false;
                selectedNode = null;
            }
            e.Use();
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && !makeTransitionMode)
        {
            int selectIndex = SelectedWindow();

            if (selectIndex != -1)
            {
                BaseInputNode nodeToChange = windows[selectIndex].ClickedOnInput(mousePos);
                if (nodeToChange)
                {
                    selectedNode = nodeToChange;
                    makeTransitionMode = true;
                }
            }
        }
        if (makeTransitionMode && selectedNode)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            DrawNodeCurve(selectedNode.windowRect, mouseRect);
            Repaint();
        }
        foreach (BaseNode n in windows)
        {
            n.DrawCurves();
            if (n.windowRect.x + 200 > workSpace.xMax)
            {
                workSpace.xMax += (n.windowRect.x - workSpace.xMax);
            }
            if (n.windowRect.y + 150 > workSpace.yMax)
            {
                workSpace.yMax += (n.windowRect.y - workSpace.yMax);
            }
            if (n.windowRect.x < 0)
            {
                n.windowRect.x = 0;
            }
            if (n.windowRect.y < 0)
            {
                n.windowRect.y = 0;
            }
        }


        BeginWindows();

        for (int i=0; i < windows.Count; i++)
        {
            windows[i].windowRect = GUI.Window(i, windows[i].windowRect, DrawNodeWindow, windows[i].windowTitle);
        }

        EndWindows();
        GUI.EndScrollView();
    }

    void DrawNodeWindow (int id)
    {
        windows[id].DrawWindow();
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();

        if (clb.Equals("inputNode"))
        {
            InputNode inputNode = CreateInstance<InputNode>();
            inputNode.windowRect = new Rect(mousePos.x, mousePos.y, 200, 150);
            ResizeWorkArea();
            windows.Add(inputNode);
        }
        else if (clb.Equals("outputNode"))
        {
            OutputNode outputNode = CreateInstance<OutputNode>();
            outputNode.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);
            ResizeWorkArea();
            windows.Add(outputNode);
        }
        else if (clb.Equals("calcNode"))
        {
            CalcNode calcNode = CreateInstance<CalcNode>();
            calcNode.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);
            ResizeWorkArea();
            windows.Add(calcNode);
        }
        else if (clb.Equals("compNode"))
        {
            ComparisonNode comparisonNode = CreateInstance<ComparisonNode>();
            comparisonNode.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);
            ResizeWorkArea();
            windows.Add(comparisonNode);
        }
        else if (clb.Equals("makeTransition"))
        {
            int selectIndex = SelectedWindow();
            if (selectIndex != -1)
            {
                selectedNode = windows[selectIndex];
                makeTransitionMode = true;
            }
        }
        else if (clb.Equals("deleteNode"))
        {
            int selectIndex = SelectedWindow();

            if (selectIndex != -1)
            {
                BaseNode selNode = windows[selectIndex];
                windows.RemoveAt(selectIndex);
                foreach (BaseNode n in windows)
                {
                    n.NodeDeleted(selNode);
                }
            }
        }
    }

    public static void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2f, start.y + start.height / 2f, 0f);
        Vector3 endPos = new Vector3(end.x + end.width / 2f, end.y + end.height / 2f, 0f);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowColor = new Color(0f, 0f, 0f, .06f);

        for (int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowColor, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }

    private int SelectedWindow()
    {
        int selected = -1;
        for (int i = 0; i < windows.Count; i++)
        {
            if (windows[i].windowRect.Contains(mousePos))
            {
                selected = i;
                break;
            }
        }
        return selected;

    }

    private void ResizeWorkArea ()
    {
        if (mousePos.x + 200 > workSpace.xMax)
        {
            workSpace.xMax += (mousePos.x + 220 - workSpace.xMax);
        }
        if (mousePos.y + 200 > workSpace.yMax)
        {
            workSpace.yMax += (mousePos.y + 170 - workSpace.yMax);
        }
        //ShrinkArea();

    }

    private void ShrinkArea()
    {
        float xMin = 1000000f;
        float yMin = 1000000f;
        float xMax = 0f;
        float yMax = 0f;
        foreach (BaseNode n in windows)
        {
            if (n.windowRect.x < xMin)
            {
                xMin = n.windowRect.x;
            }
            if (n.windowRect.x + 220 > xMax)
            {
                xMax = n.windowRect.x + 220;
            }
            if (n.windowRect.y < yMin)
            {
                yMin = n.windowRect.y;
            }
            if (n.windowRect.y > yMax)
            {
                yMax = n.windowRect.y;
            }
        }
        float xDiff = workSpace.xMax - (xMax - xMin);
        float yDiff = workSpace.yMax - (yMax - yMin);
        if (xMin != 0f && (xMax - xMin > position.width-220))
        {
            foreach (BaseNode n in windows)
            {
                n.windowRect.x -= xMin;
            }
        }
        if (yMin != 0f && (yMax - yMin > position.height-170))
        {
            foreach (BaseNode n in windows)
            {
                n.windowRect.y -= yMin;
            }
        }

        workSpace.xMax = (xMax - xMin + 220);
        workSpace.yMax = (yMax - yMin + 170);
    }


}
