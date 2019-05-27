using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrumpWindow : EditorWindow
{
    bool m_trumpIsBest = false;
    float m_slider = 50f;
    public static Texture m_tex;

    [MenuItem("Metropolia/TrumpWindow")]
    public static void CreateWindow()
    {
        TrumpWindow wnd = (TrumpWindow)EditorWindow.GetWindow(typeof(TrumpWindow));
        if (m_tex==null)
        {
            var tex = Resources.Load<Texture2D>("trump_img");
            m_tex = tex;
        }
        wnd.titleContent = new GUIContent("Trump", m_tex, "This is THE TRUMP window");

    }
    //update method
    private void OnGUI()
    {
        m_trumpIsBest = GUILayout.Toggle(m_trumpIsBest, "Trump is best!");
        if (m_trumpIsBest)
        {
            this.titleContent = new GUIContent("Trump", m_tex, "The REAL TRUMP window");
        }
        else
        {
            this.titleContent = new GUIContent("Trump", m_tex, "FAKE NEWS! WITCH HUNT!");
        }
        GUIContent gu;
        if (m_slider < 50f)
        {
            gu = new GUIContent("Poll ratings", "RIGGED SYSTEM DEEP STATE CROOKED HILLARY FAKE NEWS FAKE NEWS");
        }
        else
        {
            gu = new GUIContent("Poll ratings", "MAKE AMERICA GREAT AGAIN!");
        }
        GUILayout.Label(gu);
        //there is also a separate IntSlider
        m_slider = EditorGUILayout.Slider(m_slider, 0f, 100f);


    }
}
