using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private Camera m_mainCamera = null;
    private LevelChunk[] m_chunks = null;
    private LevelChunk m_currentChunk = null;

    private void Awake()
    {
        m_chunks = GameObject.FindObjectsOfType<LevelChunk>();
        m_mainCamera = Camera.main;
    }

    private void Start()
    {
        foreach (LevelChunk l in m_chunks)
        {
            /*
            if (l.m_collider.bounds.Contains(m_mainCamera.transform.position))
            {
                m_currentChunk = l;
            }*/
            if (l.PointInside(m_mainCamera.transform.position))
            {
                m_currentChunk = l;
            }
            foreach (GameObject g in l.m_chunks)
            {
                g.SetActive(false);
            }
        }
        foreach (GameObject g in m_currentChunk.m_chunks)
        {
            g.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        //bool inSameRoom = m_currentChunk.m_collider.bounds.Contains(m_mainCamera.transform.position);
        bool inSameRoom = m_currentChunk.PointInside(m_mainCamera.transform.position);
        if (inSameRoom) return;
        m_currentChunk.IsInside = false;
        foreach (GameObject g in m_currentChunk.m_chunks)
        {
            g.SetActive(false);
        }
        foreach (LevelChunk l in m_chunks)
        {
            //if (l.m_collider.bounds.Contains(m_mainCamera.transform.position))
            if (l.PointInside(m_mainCamera.transform.position))
            {
                l.IsInside = true;
                m_currentChunk = l;
                foreach (GameObject g in l.m_chunks)
                {
                    g.SetActive(true);
                }
            }
        }
    }
}
