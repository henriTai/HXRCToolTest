using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    Transform cam_trans;
    private void Start()
    {
        cam_trans = GameData.Instance.IconCamera.transform;
    }
    void Update()
    {
        transform.LookAt(cam_trans.position);
    }
}
