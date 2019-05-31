using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadEditor : EditorWindow
{
    private Rect headerSection;
    private Rect selectionSection;
    private bool roadSelected = true;
    public GameObject parentObject;
    [Range(1f, 300f)]
    private float cameraZoom = 50f;
    
    private enum views
    {
        firstSelection,
        roadSelection,
        intersectionSelection,
        roadRouteSelection
    };
    private views currentView = views.firstSelection;

    // Start: Default settings for creating a road
    bool oneWayStreet = false;
    Directions roadGeneralDirection1 = Directions.North;
    Directions roadGeneralDirection2 = Directions.South;
    // End: Default settings for creating a road



    [MenuItem("Window/HXRC/RoadMaker")]
    public static void OpenMainWindow()
    {
        RoadEditor roadEditor = (RoadEditor)GetWindow(typeof(RoadEditor));
        roadEditor.minSize = new Vector2(350, 300);
        roadEditor.maxSize = new Vector2(500, 300);
        roadEditor.titleContent = new GUIContent("Road Editor", "Start with an empty gameobject in desired location.");
        roadEditor.Show();
    }

    private void OnGUI()
    {
        DrawLayout();
        switch(currentView)
        {
            case views.firstSelection:
                DrawMainHeader();
                DrawMainSelection();
                break;
            case views.roadSelection:
                DrawRoadHeader();
                DrawRoadSelection();
                break;
            case views.intersectionSelection:
                DrawIntersectionHeader();
                DrawIntersectionSelection();
                break;
            case views.roadRouteSelection:
                DrawRouteEditHeader();
                DrawRoadRouteSelection();
                break;
        }
    }

    private void DrawLayout()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 50;
        selectionSection.x = 0;
        selectionSection.y = 50;
        selectionSection.width = Screen.width;
        selectionSection.height = 250;
    }

    private void DrawMainHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Main selection");
        GUILayout.EndArea();
    }

    private void DrawRoadHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Road setup");
        GUILayout.EndArea();
    }

    private void DrawIntersectionHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Intersection setup");
        GUILayout.EndArea();
    }

    private void DrawRouteEditHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Route setup");
        GUILayout.EndArea();
    }

    private void DrawMainSelection()
    {
        GUILayout.BeginArea(selectionSection);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        // This looks weird but works
        roadSelected = GUILayout.Toggle(roadSelected, "Create road");
        roadSelected = GUILayout.Toggle(!roadSelected, "Create intersection");
        roadSelected = !roadSelected;

        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        string selectedString;
        if (roadSelected)
        {
            selectedString = "a road";
        }
        else
        {
            selectedString = "an intersection";
        }
        GUILayout.Label("Select passage type: " + selectedString);
        GUILayout.Space(10f);

        /*
        if (GUILayout.Button("Start"))
        {
            //CardEditor.OpenNewWindow();
            //eventList = null;
            //selectionWindow.Close();

        }*/
        GUILayout.Label("Instructions:");
        GUILayout.Label("1. Create an empty gameobject.");
        GUILayout.Label("2. Give it a suitable name.");
        GUILayout.Label("3. Place it in desired location on map (scene view).");
        GUILayout.Label("4. Place the gameobject to the object field below.");
        parentObject = EditorGUILayout.ObjectField(parentObject, typeof(GameObject), true) as GameObject;
        if (parentObject != null)
        {

            if (GUILayout.Button("Next"))
            {
                if (roadSelected)
                {
                    currentView = views.roadSelection;
                }
                else
                {
                    currentView = views.intersectionSelection;
                }
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawRoadSelection()
    {
        GUILayout.BeginArea(selectionSection);
        GUILayout.BeginVertical();

        oneWayStreet = GUILayout.Toggle(oneWayStreet, "Is this a one-way street?");
        roadGeneralDirection1 = (Directions)EditorGUILayout.EnumPopup("Start general direction", roadGeneralDirection1);
        roadGeneralDirection2 = (Directions)EditorGUILayout.EnumPopup("End general direction", roadGeneralDirection2);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Back"))
        {
            currentView = views.firstSelection;
        }

        if (GUILayout.Button("Next"))
        {
            currentView = views.roadRouteSelection;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawIntersectionSelection()
    {
        GUILayout.BeginArea(selectionSection);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Back"))
        {
            currentView = views.firstSelection;
        }

        if (GUILayout.Button("Next"))
        {
            //
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawRoadRouteSelection()
    {
        GUILayout.BeginArea(selectionSection);
        GUILayout.BeginVertical();

        Selection.activeGameObject = parentObject;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Camera distance modifier");
        cameraZoom = EditorGUILayout.Slider(cameraZoom, 1f, 300f);
        GUILayout.EndHorizontal();
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.AlignViewToObject(parentObject.transform);
        sceneView.LookAtDirect(parentObject.transform.position, Quaternion.Euler(90, 0, 0), cameraZoom);
        sceneView.orthographic = true;

        DrawMarkerLine(Vector3.zero, GeneralDirection.DirectionVector(Directions.SouthWest), 100f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Back"))
        {
            Selection.activeGameObject = null;
            currentView = views.roadSelection;
        }

        if (GUILayout.Button("Next"))
        {
            //
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }


    private void DrawMarkerLine(Vector3 pos, Vector3 dir, float length)
    {
        Vector3 half = dir * 0.5f * length;
        Vector3 startPos = pos - half;
        Vector3 endPos = pos + half;
    }

    private void OnDestroy()
    {

    }
}
