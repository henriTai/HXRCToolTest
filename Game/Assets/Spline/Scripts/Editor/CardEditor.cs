using UnityEngine;
using UnityEditor;


public class CardEditor : EditorWindow
{


    static void OpenWindow()
    {
        CardEditor cardEditor = (CardEditor)GetWindow(typeof(CardEditor));
        cardEditor.minSize = new Vector2(600, 300);
        cardEditor.Show();
    }

    public static void OpenNewWindow()
    {
        CardEditor cardEditor = (CardEditor)GetWindow(typeof(CardEditor));
        cardEditor.minSize = new Vector2(600, 300);
        cardEditor.Show();
    }

    private void OnEnable()
    {

    }

    private void OnGUI()
    {

    }
}

public class SelectionWindow : EditorWindow
{

    Rect headerSection;
    Rect selectionSection;
    string[] eventList;
    int selectedEvent = 0;
    static SelectionWindow selectionWindow;

    [MenuItem("Window/Card Editor")]
    static void OpenWindow()
    {
        selectionWindow = (SelectionWindow)GetWindow(typeof(SelectionWindow));
        selectionWindow.minSize = new Vector2(300, 300);
        selectionWindow.Show();
    }

    private void OnEnable()
    {
        /*
        LogScript editorLog = Resources.Load<LogScript>("EditorLog/EditorLog");
        eventList = new string[editorLog.eventNames.Count + 1];
        eventList[0] = "None";
        for (int i = 0; i < editorLog.eventNames.Count; i++)
        {
            eventList[i + 1] = editorLog.eventNames[i];
        }
        //tähän voi laittaa tekstuurit
        */
    }

    private void OnGUI()
    {
        DrawLayout();
        DrawHeader();
        DrawSelection();
    }

    void DrawLayout()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 50;
        selectionSection.x = 0;
        selectionSection.y = 50;
        selectionSection.width = Screen.width;
        selectionSection.height = 150;
    }

    void DrawHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Card Editor");
        GUILayout.EndArea();
    }

    void DrawSelection()
    {
        GUILayout.BeginArea(selectionSection);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("New Event");
        if (GUILayout.Button("Start"))
        {
            CardEditor.OpenNewWindow();
            eventList = null;
            selectionWindow.Close();

        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Open event");
        if (eventList != null)
        {
            if (eventList.Length != 0)
            {
                selectedEvent = EditorGUILayout.Popup(selectedEvent, eventList);
                if (selectedEvent != 0)
                {
                    if (GUILayout.Button("Open"))
                    {

                    }
                }

            }
            else
            {
                GUILayout.Label("No events to open");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }


}
