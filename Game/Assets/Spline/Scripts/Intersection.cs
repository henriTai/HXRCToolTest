using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Intersection : MonoBehaviour
{
    [SerializeField]
    private Vector3 centerPoint;
    [SerializeField]
    private float frameWidth;
    [SerializeField]
    private float frameHeight;
    private const float defaultFrameMeasure = 5f;
    public bool framed;
    public bool allNodesSet;
    public GameObject roadNetwork;
    [SerializeField]
    private TrafficSize traffic;
    [SerializeField]
    private SpeedLimits speedLimit;
    //for selecting /deselcting in and out nodes in set up
    public NodeInfo[] nodesInBox;
    public bool nodesInBoxSet = false;
    [SerializeField]
    private int infoIndex = -1;
    [SerializeField]
    public List<HelperLine> helperLines;
    [SerializeField]
    public List<ExistingLane> existingLanes;
    public int existingLaneIndex = 0;
    public bool existingLanesChecked = false;

    public int inIndex = 0;
    public int outIndex = 0;

    public Vector3 CenterPoint
    {
        get
        {
            return centerPoint;
        }
        set
        {
            centerPoint = value;
        }
    }

    public float FrameWidth
    {
        get
        {
            return frameWidth;
        }
        set
        {
            frameWidth = value;
        }
    }

    public float FrameHeight
    {
        get
        {
            return frameHeight;
        }
        set
        {
            frameHeight = value;
        }
    }

    public TrafficSize Traffic
    {
        get
        {
            return traffic;
        }
        set
        {
            traffic = value;
        }
    }

    public SpeedLimits SpeedLimit
    {
        get
        {
            return speedLimit;
        }
        set
        {
            speedLimit = value;
        }
    }
    
    public int GetInfoIndex
    {
        get
        {
            return infoIndex;
        }
    }
    
    public void SetInfoIndexToFirst()
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            infoIndex = -1;
        }
        else
        {
            infoIndex = 0;
        }
    }

    public void SetInOutAll(NodeInOut inOut)
    {
        if (nodesInBox != null)
        {
            for (int i = 0; i < nodesInBox.Length; i++)
            {
                nodesInBox[i].inOut = inOut;
            }
        }
    }

    public void SetInOut(NodeInOut inOut)
    {
        if (nodesInBox == null || nodesInBox.Length != 0)
        {
            nodesInBox[infoIndex].inOut = inOut;
        }
    }

    public void MoveInfoIndex (int val)
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            return;
        }
        int index = infoIndex + val;
        if (index < 0)
        {
            infoIndex = nodesInBox.Length - 1;
        }
        else if (index > nodesInBox.Length - 1)
        {
            infoIndex = 0;
        }
        else
        {
            infoIndex = index;
        }
    }

    public int GetInfoSize()
    {
        if (nodesInBox == null)
        {
            return 0;
        }
        else
        {
            return nodesInBox.Length;
        }
    }

    public int SelectNextAvailable()
    {
        if (infoIndex == nodesInBox.Length - 1)
        {
            bool found = false;
            for (int i = 0; i < nodesInBox.Length - 1; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
                if (!found)
                {
                    infoIndex = -1;
                }
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex + 1; i < nodesInBox.Length; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= infoIndex; i++)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }

    public int SelectPreviousAvailable()
    {
        if (infoIndex == 0)
        {
            bool found = false;
            for (int i = nodesInBox.Length - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
            }
            if (!found)
            {
                infoIndex = -1;
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = nodesInBox.Length - 1; i >= infoIndex; i--)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }

    public void SelectAdjacents()
    {
        NodeInOut inOut = nodesInBox[infoIndex].inOut;
        Nodes n = nodesInBox[infoIndex].node;
        if (n.ParallelRight)
        {
            Nodes r1 = n.ParallelRight;
            int index = FindNodeInfoIndex(r1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (r1.ParallelRight)
                {
                    Nodes r2 = r1.ParallelRight;
                    index = FindNodeInfoIndex(r2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
        if (n.ParallelLeft)
        {
            Nodes l1 = n.ParallelLeft;
            int index = FindNodeInfoIndex(l1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (l1.ParallelLeft)
                {
                    Nodes l2 = l1.ParallelLeft;
                    index = FindNodeInfoIndex(l2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
    }

    private int FindNodeInfoIndex(Nodes node)
    {
        int ind = -1;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].node == node)
            {
                ind = i;
                break;
            }
        }
        return ind;
    }

    public Nodes GetSelectedNodeInfo(out NodeInOut inOut)
    {
        if (infoIndex == -1)
        {
            inOut = NodeInOut.NotUsed;
            return null;
        }
        else
        {
            inOut = nodesInBox[infoIndex].inOut;
            return nodesInBox[infoIndex].node;
        }
    }

    public void RemoveHelperLines()
    {
        helperLines = new List<HelperLine>();
        inIndex = 0;
        outIndex = 0;
    }

    public int GetInOutPositions(out List<Vector3> ins, out List<Vector3> outs)
    {
        ins = new List<Vector3>();
        outs = new List<Vector3>();
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            NodeInfo n = nodesInBox[i];
            if (n.inOut== NodeInOut.InNode)
            {
                ins.Add(n.node.transform.position);
            }
            else if (n.inOut == NodeInOut.OutNode)
            {
                outs.Add(n.node.transform.position);
            }
        }
        for (int i = 0; i < helperLines.Count; i++)
        {
            HelperLine h = helperLines[i];
            Vector3 p0 = h.startPoint;
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                if (h.inOut[j] == NodeInOut.InNode)
                {
                    ins.Add(pnt);
                }
                else
                {
                    outs.Add(pnt);
                }
            }
        }
        return ins.Count + outs.Count;
    }

    public ExistingLane GetCurrentExistingLane()
    {
        return existingLanes[existingLaneIndex];
    }

    public void MoveExistingLaneIndex(int val)
    {
        int v = existingLaneIndex + val;
        if (v < 0)
        {
            existingLaneIndex = existingLanes.Count - 1;
        }
        else if (v > existingLanes.Count - 1)
        {
            existingLaneIndex = 0;
        }
        else
        {
            existingLaneIndex = v;
        }
    }

    public void SetCurrentExistingLane(ExistingLane ex)
    {
        existingLanes[existingLaneIndex] = ex;
    }

    public int GetUnconfirmedExistingLaneCount()
    {
        int v = 0;
        for (int i=0; i < existingLanes.Count; i++)
        {
            if (!existingLanes[i].confirmed)
            {
                v++;
            }
        }
        return v;
    }

    public void Reset()
    {
        CenterPoint = transform.position;
        FrameWidth = defaultFrameMeasure;
        FrameHeight = defaultFrameMeasure;
        framed = false;
        traffic = TrafficSize.Average;
        speedLimit = SpeedLimits.KMH_30;
    }
}

[Serializable]
public class NodeInfo
{
    public Nodes node;
    public NodeInOut inOut;
}

[Serializable]
public class HelperLine
{
    public Vector3 startPoint;
    public Vector3 direction;
    public float lenght;
    public List<float> nodePoints;
    public List<NodeInOut> inOut;
}

[Serializable]
public class ExistingLane
{
    public List<Nodes> nodes;
    public int inNodeIndex;
    public int outNodeIndex;
    public DriverYield laneYield;
    public IntersectionDirection turnDirection;
    public bool confirmed;
}

[Serializable]
public class CreatedSplines
{

}