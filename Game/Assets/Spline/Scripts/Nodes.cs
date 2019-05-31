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
    private Nodes startNode;
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

    public Nodes StartNode
    {
        get
        {
            return startNode;
        }
        set
        {
            if (value is Nodes)
            {
                Nodes n = value;
                n.AddNextNode(this);
                startNode = n;
            }
        }
    }

    public Nodes[] NextNodes
    {
        get
        {
            return next;
        }
    }


    public void AddNextNode(Nodes n)
    {
        if (!IsInNextNodes(n))
        {
            Array.Resize(ref next, next.Length + 1);
            next[next.Length - 1] = n;
        }
    }

    private bool IsInNextNodes(Nodes n)
    {
        bool isNext = false;
        for (int i = 0; i < next.Length; i++)
        {
            if (n == next[i])
            {
                isNext = true;
                break;
            }
        }
        return isNext;

    }

    public bool GetDirection(out Vector3 direction)
    {
        if (startNode== null)
        {
            direction = Vector3.right;
            return false;
        }
        else
        {
            direction = (transform.position - startNode.transform.position).normalized;
            return true;
        }
    }

    private void Reset()
    {
        startNode = null;
        next = new Nodes[0];
    }
}
