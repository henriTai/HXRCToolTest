using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

public static class ObjectTagger
{
    public enum IconType
    {
        Label = 0,
        Small = 1,
        Pix16 = 2
    };

    [SerializeField]
    static readonly int[] iconCount = { 8, 16, 16 };

    [SerializeField]
    static readonly GUIContent[] labelIconArray =
    {
        EditorGUIUtility.IconContent("sv_label_0"),
        EditorGUIUtility.IconContent("sv_label_1"),
        EditorGUIUtility.IconContent("sv_label_2"),
        EditorGUIUtility.IconContent("sv_label_3"),
        EditorGUIUtility.IconContent("sv_label_4"),
        EditorGUIUtility.IconContent("sv_label_5"),
        EditorGUIUtility.IconContent("sv_label_6"),
        EditorGUIUtility.IconContent("sv_label_7")
    };
    [SerializeField]
    static readonly GUIContent[] smallIconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot1_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot2_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot3_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot4_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot5_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot6_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot7_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot8_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot9_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot10_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot11_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot12_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot13_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot14_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot15_sml")
    };
    [SerializeField]
    static readonly GUIContent[] pix16IconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot1_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot2_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot5_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot7_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot10_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot12_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot13_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot15_pix16_gizmo")
    };

    public static void SetIcon(GameObject g, IconType i, int index)
    {
        int div = iconCount[(int) i];
        int ind = index % div;
        ParallelBezierSplines b = new ParallelBezierSplines();
        switch(i)
        {
            case IconType.Label:
                var iconL = labelIconArray[ind];
                var eguL = typeof(EditorGUIUtility);
                var flagsL = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsL = new object[] { g, iconL.image };
                var setIconL = eguL.GetMethod("SetIconForObject", flagsL, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconL.Invoke(null, argsL);
                break;
            case IconType.Small:
                var iconS = smallIconArray[ind];
                var eguS = typeof(EditorGUIUtility);
                var flagsS = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsS = new object[] { g, iconS.image };
                var setIconS = eguS.GetMethod("SetIconForObject", flagsS, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconS.Invoke(null, argsS);
                break;
            case IconType.Pix16:
                var iconP = pix16IconArray[ind];
                var eguP = typeof(EditorGUIUtility);
                var flagsP = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsP = new object[] { g, iconP.image };
                var setIconP = eguP.GetMethod("SetIconForObject", flagsP, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconP.Invoke(null, argsP);
                break;
        }
    }
}
