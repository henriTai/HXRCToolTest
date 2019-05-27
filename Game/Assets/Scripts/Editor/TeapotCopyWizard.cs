using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TeapotCopyWizard : ScriptableWizard
{
    public int m_teapotCount = 10;
    public float m_spacing = 0.5f;
    public GameObject m_teapot = null;

    [MenuItem("Metropolia/CopyTeapots")]
    public static void CopyTeapots()
    {
        // Create on buttonin nimi, vois antaa 2 buttonia
        ScriptableWizard.DisplayWizard<TeapotCopyWizard>("Copy Teapots", "Create");
        
    }

    private void OnWizardCreate()
    {
        if (m_teapot == null)
        {
            Debug.Log("m_teapot is not set.");
            return;
        }
        for (int i=0; i<m_teapotCount; i++)
        {
            GameObject g = GameObject.Instantiate<GameObject>(m_teapot);
            g.transform.position = new Vector3(i * m_spacing, 0f, 0f);
        }
    }
}
