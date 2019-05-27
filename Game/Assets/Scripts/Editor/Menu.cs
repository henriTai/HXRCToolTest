using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Menu : MonoBehaviour
{
    [MenuItem("Metropolia/Drop")]
    public static void HelloMenu()
    {
        if (Selection.gameObjects == null)
        {
            return;
        }
        foreach (GameObject g in Selection.gameObjects)
        {
            Ray r = new Ray(g.transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, float.MaxValue))
            {
                g.transform.position = hit.point;
            }
        }
    }
}
