using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItemCreator : MonoBehaviour
{
    public bool m_pickable = false;
    public GameObject m_icon = null;

    void Start()
    {
        GameObject root = this.transform.parent.gameObject;
        GameItem gi = root.AddComponent<GameItem>();
        GameObject icon = GameObject.Instantiate<GameObject>(m_icon);
        gi.m_icon = icon;
        gi.m_icon.transform.parent = root.transform;
        gi.m_icon.transform.localPosition = Vector3.zero;
        gi.m_icon.transform.localRotation = Quaternion.identity;
        gi.m_icon.AddComponent<LookAt>();
        SetLayerToAll(gi.m_icon, LayerMask.NameToLayer("Icons"));
        gi.m_pickable = m_pickable;

        GameData.Instance.AddItem(gi);
        GameObject.DestroyImmediate(this.gameObject);
    }

    private void SetLayerToAll(GameObject g, int i)
    {
        g.layer = i;
        foreach(Transform t in g.transform)
        {
            SetLayerToAll(t.gameObject, i);
        }
    }

}
