using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker2 : MonoBehaviour
{
    public BezierSpline spline;
    public bool lookForward;
    public float splineLength;
    public float distanceTraveled;
    public float speed;


    public enum SplineWalkerMode
    {
        Once,
        Loop,
        PingPong
    };

    public SplineWalkerMode mode;

    private bool goingForward = true;

    private void Awake()
    {
        splineLength = spline.SplineLength;
    }

    private void Update()
    {
        if (goingForward)
        {
            distanceTraveled += Time.deltaTime * speed;
            if (distanceTraveled > splineLength)
            {
                if (mode == SplineWalkerMode.Once)
                {
                    distanceTraveled = splineLength;
                }
                else if (mode == SplineWalkerMode.Loop)
                {
                    distanceTraveled -= splineLength;
                }
                else
                {
                    distanceTraveled = 2 * splineLength - distanceTraveled;
                    goingForward = false;
                }
            }
        }
        else
        {
            distanceTraveled -= Time.deltaTime * speed;
            if (distanceTraveled < 0f)
            {
                distanceTraveled = -distanceTraveled;
                goingForward = true;
            }
        }
        Vector3 position = spline.GetPointWhenTraveled(distanceTraveled);
        transform.localPosition = position;
        if (lookForward)
        {
            transform.LookAt(position + spline.GetDirectionWhenTraveled(distanceTraveled));
        }
    }
}
