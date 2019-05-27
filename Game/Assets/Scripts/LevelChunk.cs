using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChunk : MonoBehaviour
{
    public bool IsInside { get; set; } = false;
    public GameObject[] m_chunks = null;
    public Collider m_collider = null;

    // for half space check ---------------------------------------------------------------------
    public Ray[] m_planes;
    // ------------------------------------------------------------------------------------------

    private void Awake()
    {
        m_collider = GetComponent<Collider>();

        // For half space check -----------------------------------------------------------------
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh m = mf.sharedMesh;
        int tris = m.triangles.Length / 3;
        m_planes = new Ray[tris];
        for (int i=0; i< tris; i++)
        {
            Ray r = new Ray(this.transform.TransformPoint(m.vertices[m.triangles[i * 3]]),
                this.transform.TransformDirection(m.normals[m.triangles[i * 3]]));
            m_planes[i] = r;
        }
        // --------------------------------------------------------------------------------------
    }
    // for half space check ---------------------------------------------------------------------
    public bool PointInside(Vector3 p)
    {
        foreach (Ray r in m_planes)
        {
            Vector3 d = (p - r.origin).normalized;
            if (Vector3.Dot(d, r.direction) > 0f)
            {
                return false;
            }
        }
        return true;
    }
    // -------------------------------------------------------------------------------------------
}
