using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadHelper : MonoBehaviour
{
    public enum Directions
    {
        North,
        NothWest,
        West,
        SouthWest,
        South,
        SouthEast,
        East,
        NorthEast,
    };

    public GameObject[] inputNodes;
    public Directions[] directions;
    public int[] splinesOnNodes;


}

