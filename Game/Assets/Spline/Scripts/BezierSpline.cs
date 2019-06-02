using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BezierSpline : MonoBehaviour
{
    [SerializeField]
    private bool loop;

    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private float[] segmentLengths;

    [SerializeField]
    private float splineLength;

    [SerializeField]
    private BezierControlPointMode[] modes;

    public List<GameObject> wayPoints;
    public GameObject waypointParent;

    public Vector3 GetStartPoint()
    {
        return points[0];
    }

    public Vector3 GetEndPoint()
    {
        return points[points.Length - 1];
    }

    public int GetSegmentCount()
    {
        return segmentLengths.Length;
    }

    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }

    public float SplineLength
    {
        get
        {
            return splineLength;
        }
    }

    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
            if (value)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public Vector3 GetControlPoint (int index)
    {
        return points[index];
    }

    public float GetSegmentLength(int index)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            if (loop)
            {
                segment = 0;
            }
            else
            {
                segment -= 1;
            }
        }
        return segmentLengths[segment];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }
        //***********
        points[index] = point;
        EnforceMode(index);
    }

    public Vector3 GetDirectionWhenTraveled(float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - segmentLengths[i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= segmentLengths[i];
                }
            }
        }
        float fraq = dist / segmentLengths[segment];
        return GetSegmentedDirection(segment, fraq);
    }

    private Vector3 GetSegmentedDirection(int segment, float fraq)
    {

        return GetSegmentedVelocity(segment, fraq).normalized;
    }

    private Vector3 GetSegmentedVelocity(int segment, float fraq)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], fraq))
            - transform.position;
    }

    public Vector3 GetPointWhenTraveled(float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - segmentLengths[i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= segmentLengths[i];
                }
            }
        }
        float fraq = dist / segmentLengths[segment];
        return GetSegmentedPoint(segment, fraq);

    }

    public Vector3 GetSegmentedPoint(int segment, float fraq)
    {
        if (segment == segmentLengths.Length)
        {
            if (loop)
            {
                segment = 0;
            }
            else
            {
                segment -= 1;
            }
        }
        int i = segment * 3;
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], fraq));
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public Vector3 GetPoint (float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i+1], points[i+2], points[i+3], t));
    }

    public Vector3 GetVelocity (float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t))
            - transform.position;
    }

    public void RecalculateLength (int index)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            if (loop)
            {
                segment = 0;
            }
            else
            {
                segment -= 1;
            }
        }
        float dist = 0f;
        Vector3 prev = GetSegmentedPoint(segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPoint(segment, i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        segmentLengths[segment] = dist;
        if (loop == false && segment == 0)
        {
            UpdateSplineLength();
            return;
        }
        else
        {
            if (segment==0)
            {
                segment = segmentLengths.Length - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPoint(segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPoint(segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            segmentLengths[segment] = dist;
            UpdateSplineLength();
        }
    }

    private void UpdateSplineLength()
    {
        float length = 0;
        for (int i=0; i< segmentLengths.Length; i++)
        {
            length += segmentLengths[i];
        }
        splineLength = length;
    }

    private int GetI(ref float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return i;
    }

    public Vector3 GetDirection (float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve ()
    {
        Vector3 point = points[points.Length - 1];
        float length = segmentLengths[segmentLengths.Length - 1] / 3f;
        Vector3 dir = GetSegmentedDirection(segmentLengths.Length -1, 1f);
        // Array requires System-namespace. points is passed as a REFERENCE (not a copy)
        Array.Resize(ref points, points.Length + 3);
        point += length * dir;
        points[points.Length - 3] = point;
        point += length * dir;
        points[points.Length - 2] = point;
        point += length * dir;
        points[points.Length - 1] = point;
        Array.Resize(ref segmentLengths, segmentLengths.Length + 1);
        segmentLengths[segmentLengths.Length - 1] = 3f;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (loop)
        {
            if (modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if (modeIndex == modes.Length -1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    public void Reset()
    {
        points = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };
        modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
        wayPoints = new List<GameObject>();
        splineLength = Vector3.Distance(points[0], points[3]);
        segmentLengths = new float[] {splineLength }; //jos ei alusteta suoralla pitää muuttaa
        
    }
}
