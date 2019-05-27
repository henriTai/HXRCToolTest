using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class GatherEverything : ScriptableWizard
{
    public string m_folder = "Assets/";

    [MenuItem("Metropolia/GatherEverything")]
    private static void GatherEverythingMenu()
    {
        //Tittle, Button text (regardless the button name, OnWizardCreate() refers to THIS button)
        ScriptableWizard.DisplayWizard<GatherEverything>("Gather Everything", "Gather"); 
    }

    private void OnWizardCreate()
    {
        StreamWriter sw = new StreamWriter(Application.dataPath + "/data.txt");
        string targetFolder = m_folder;
        // Unity uses guids to locate items. Because this, one can move items from folder to folder without consequences.
        // first parameter is used filter
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { targetFolder });

        foreach (string g in guids)
        {
            string relative_path = AssetDatabase.GUIDToAssetPath(g);
            string full_path = Application.dataPath.Replace("Assets", relative_path);
            string log_entry = full_path;
            //lataa objektin muistiin, ei kuitenkaan skeneen huom.
            GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(relative_path, typeof(GameObject));
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                continue;
            }
            bool writeMat = true;
            Material m = mr.sharedMaterial;
            if (m==null)
            {
                writeMat = false;
            }
            Texture t = m.mainTexture;
            if (t==null)
            {
                writeMat = false;
            }
            if (writeMat)
            {
                string asset_path = AssetDatabase.GetAssetPath(t);
                string full_asset_path = Application.dataPath.Replace("Assets", asset_path);
                log_entry = string.Format("{0}|{1}", log_entry, full_asset_path);
            }

            sw.WriteLine(log_entry);
            sw.Flush();
        }
        sw.Close();
    }
}
