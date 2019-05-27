using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Nodes : MonoBehaviour
{
    public enum TrafficSize
    {
        High,
        MidHeigh,
        Average,
        BelowAverage,
        Low
    }
    [SerializeField]
    private Nodes[] next;
    [SerializeField]
    private TrafficSize trafficSize;

    public TrafficSize Traffic
    {
        get
        {
            return trafficSize;
        }
        set
        {
            trafficSize = value;
        }
    }

    public Nodes[] NextNodes
    {
        get
        {
            return next;
        }
    }


    public void AddNode(Nodes n)
    {
        Array.Resize(ref next, next.Length + 1);
        next[next.Length - 1] = n;
    }

    private void Reset()
    {
        next = new Nodes[0];
    }
}
