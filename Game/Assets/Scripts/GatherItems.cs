using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherItems : MonoBehaviour
{
    GameData data;
    private void Start()
    {
        data = GameData.Instance;
    }
    private bool isInRadius (GameItem gi)
    {
        float d = Vector3.Distance(this.transform.position, gi.transform.position);
        if (d < data.PickRadius)
        {
            if (gi.m_picked || gi.m_pickable == false)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        for (int i=0; i < data.ItemCount; i++)
        {
            GameItem gi = data.GetItem(i);
            if (isInRadius(gi))
            {
                gi.m_icon.SetActive(true);
            }
            else
            {
                gi.m_icon.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            for (int i=0; i< data.ItemCount; i++)
            {
                GameItem gi = data.GetItem(i);
                if (gi.m_pickable == false)
                {
                    continue;
                }
                if (gi.m_picked == true)
                {
                    continue;
                }
                if (gi.m_icon.gameObject.activeSelf == false)
                {
                    continue;
                }
                gi.m_picked = true;
                gi.gameObject.SetActive(false);
            }
        }
    }
}
