using System;
using UnityEngine;
using UnityEditor;

public class AssetPostProcessor : AssetPostprocessor
{
    private void OnPostprocessGameObjectWithUserProperties(
        GameObject go, string[] names, System.Object[] values)
    {
        for (int i=0; i< names.Length; ++i)
        {
            string propName = names[i];
            if (propName == "UDP3DSMAX")
            {
                string p = values[i].ToString();
                string[] arr = p.Split('=');
                string n = arr[0].Trim();
                string m = arr[1].Trim();

                if (n.ToLower() == "pickable")
                {
                    GameItemCreator gic = go.AddComponent<GameItemCreator>();
                    GameObject ic = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Objects/icon_Icon.fbx");
                    gic.m_icon = ic;
                    bool pickable = Convert.ToBoolean(m);
                    gic.m_pickable = pickable;
                }
            }
        }
    }
}
