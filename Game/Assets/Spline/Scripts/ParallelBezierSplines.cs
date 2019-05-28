using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ParallelBezierSplines : MonoBehaviour
{

    [SerializeField]
    private bool loop;

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    private Vector3[,] leftLanePoints;
    [SerializeField]
    private Vector3[,] rightLanePoints;


    [SerializeField]
    private float[] segmentLengths;
    
    [SerializeField]
    private float[,] leftSegmentLengths;
    [SerializeField]
    private float[,] rightSegmentLengths;
    
    [SerializeField]
    private float splineLength;
    
    [SerializeField]
    private float[] leftSplineLengths;
    [SerializeField]
    private float[] rightSplineLengths;
    
    
    [SerializeField]
    private float[,] leftSpacings;
    [SerializeField]
    private float[,] rightSpacings;
    
    [SerializeField]
    private BezierControlPointMode[] modes;
    
    [SerializeField]
    private BezierControlPointMode[,] leftModes;

    [SerializeField]
    private BezierControlPointMode[,] rightModes;

    public List<GameObject> wayPointsLeft1;
    public List<GameObject> wayPointsLeft2;
    public List<GameObject> wayPointsLeft3;
    public List<GameObject> wayPointsRight1;
    public List<GameObject> wayPointsRight2;
    public List<GameObject> wayPointsRight3;
    
    public GameObject waypointParent;
    
    public GameObject left1Parent;
    public GameObject left2Parent;
    public GameObject left3Parent;
    public GameObject right1Parent;
    public GameObject right2Parent;
    public GameObject right3Parent;

    [SerializeField]
    private int leftLaneCount;
    [SerializeField]
    private int rightLaneCount;
    
    [SerializeField]
    private float lanePositioning = 0f;

    public int LeftLaneCount
    {
        get { return leftLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (leftLaneCount != v)
            {
                leftLaneCount = v;
            }
        }
    }

    public int RightLaneCount
    {
        get { return rightLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (rightLaneCount != v)
            {
                rightLaneCount = v;
            }
        }
    }

    public float LanePositioning
    {
        get { return lanePositioning; }
        set
        {
            float v = Mathf.Clamp(value, 0, float.MaxValue);
            if (lanePositioning != v)
            {
                lanePositioning = v;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LeftLaneCount = leftLaneCount;
        RightLaneCount = rightLaneCount;
        LanePositioning = lanePositioning;
    }

#endif

    public Vector3 GetStartPoint()
    {
        return points[0];
    }

    public Vector3 GetLeftLaneStartPoint(int lane)
    {
        return leftLanePoints[lane, 0];
    }

    public Vector3 GetRightLaneStartPoint(int lane)
    {
        return rightLanePoints[lane, 0];
    }

    public Vector3 GetEndPoint()
    {
        return points[points.Length - 1];
    }

    public Vector3 GetLeftEndPoint(int lane)
    {
        if (lane < leftLaneCount)
        {
            return leftLanePoints[lane, leftLanePoints.GetLength(1) - 1];
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 GetRightEndPoint(int lane)
    {
        if (lane < rightLaneCount)
        {
            return rightLanePoints[lane, rightLanePoints.GetLength(1) - 1];
        }
        else
        {
            return Vector3.zero;
        }
    }

    public int SegmentCount
    {
        get
        {
            return segmentLengths.Length;
        }
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

    public float GetLeftLaneLength(int index)
    {
        if (index < leftLaneCount)
        {
            return leftSplineLengths[index];
        }
        else
        {
            return 0f;
        }
    }

    public float GetRightLaneLength(int index)
    {
        if (index < rightLaneCount)
        {
            return rightSplineLengths[index];
        }
        else
        {
            return 0f;
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
                for (int i=0; i < rightLaneCount; i++)
                {
                    SetControlPointRight(0, i, rightLanePoints[i, 0]);
                }
                for (int i=0; i < leftLaneCount; i++)
                {
                    int ind = leftLanePoints.GetLength(1) - 1;
                    SetControlPointLeft(ind, i, leftLanePoints[i, ind]);
                }

            }
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public Vector3 GetControlPointLeft(int lane, int index)
    {
        if (lane < leftLaneCount && index < leftLanePoints.GetLength(1))
        {
            return leftLanePoints[lane, index];
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 GetControlPointRight(int lane, int index)
    {
        if (lane < rightLaneCount && index < rightLanePoints.GetLength(1))
        {
            return rightLanePoints[lane, index];
        }
        else
        {
            return Vector3.zero;
        }
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

    public float GetLeftSegmentLength(int lane, int index)
    {
        if (lane >= LeftLaneCount)
        {
            return 0f;
        }
        if (index >= leftSegmentLengths.GetLength(1))
        {
            return 0f;
        }
        int segment = index / 3;
        if (segment == leftSegmentLengths.GetLength(1))
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
        return leftSegmentLengths[lane, segment];
    }

    public float GetRightSegmentLength(int lane, int index)
    {
        if (lane >= RightLaneCount)
        {
            return 0f;
        }
        if (index >= rightSegmentLengths.GetLength(1))
        {
            return 0f;
        }
        int segment = index / 3;
        if (segment == rightSegmentLengths.GetLength(1))
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
        return rightSegmentLengths[lane, segment];
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
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

    public void SetControlPointLeft(int index, int lane,  Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point - leftLanePoints[lane, index];
            if (loop)
            {
                if (index == 0)
                {
                    int pLength = leftLanePoints.GetLength(1);
                    leftLanePoints[lane, 1] += delta;
                    leftLanePoints[lane, pLength - 2] += delta;
                    leftLanePoints[lane, pLength - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    leftLanePoints[lane, 0] = point;
                    leftLanePoints[lane, 1] += delta;
                    leftLanePoints[lane, index - 1] += delta;
                }
                else
                {
                    leftLanePoints[lane, index - 1] += delta;
                    leftLanePoints[lane, index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    leftLanePoints[lane, index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    leftLanePoints[lane, index + 1] += delta;
                }
            }
        }
        //***********
        leftLanePoints[lane, index] = point;
        EnforceModeLeft(lane, index);
    }

    public void SetControlPointRight(int index, int lane, Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point - rightLanePoints[lane, index];
            if (loop)
            {
                if (index == 0)
                {
                    int pLength = rightLanePoints.GetLength(1);
                    rightLanePoints[lane, 1] += delta;
                    rightLanePoints[lane, pLength - 2] += delta;
                    rightLanePoints[lane, pLength - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    rightLanePoints[lane, 0] = point;
                    rightLanePoints[lane, 1] += delta;
                    rightLanePoints[lane, index - 1] += delta;
                }
                else
                {
                    rightLanePoints[lane, index - 1] += delta;
                    rightLanePoints[lane, index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    rightLanePoints[lane, index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    rightLanePoints[lane, index + 1] += delta;
                }
            }
        }
        //***********
        rightLanePoints[lane, index] = point;
        EnforceModeRight(lane, index);
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

    private void EnforceModeLeft(int lane, int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = leftModes[lane, modeIndex];
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == leftModes.GetLength(1)-1))
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
                fixedIndex = leftLanePoints.GetLength(1) - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= leftLanePoints.GetLength(1))
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= leftLanePoints.GetLength(1))
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = leftLanePoints.GetLength(1) - 2;
            }
        }

        Vector3 middle = leftLanePoints[lane, middleIndex];
        Vector3 enforcedTangent = middle - leftLanePoints[lane, fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, leftLanePoints[lane, enforcedIndex]);
        }
        leftLanePoints[lane, enforcedIndex] = middle + enforcedTangent;
    }

    private void EnforceModeRight(int lane, int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = rightModes[lane, modeIndex];
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == rightModes.GetLength(1) - 1))
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
                fixedIndex = rightLanePoints.GetLength(1) - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= rightLanePoints.GetLength(1))
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= rightLanePoints.GetLength(1))
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = rightLanePoints.GetLength(1) - 2;
            }
        }

        Vector3 middle = rightLanePoints[lane, middleIndex];
        Vector3 enforcedTangent = middle - rightLanePoints[lane, fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, rightLanePoints[lane, enforcedIndex]);
        }
        rightLanePoints[lane, enforcedIndex] = middle + enforcedTangent;
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
        float fraction = dist / segmentLengths[segment];
        return GetSegmentedDirection(segment, fraction);
    }

    public Vector3 GetDirectionWhenTraveledLeft(int lane, float distanceTraveled)
    {
        if (lane >= leftLaneCount)
        {
            return Vector3.zero;
        }
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = leftSegmentLengths.GetLength(1);
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - leftSegmentLengths[lane, i]  < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= leftSegmentLengths[lane, i];
                }
            }
        }
        float fraction = dist / leftSegmentLengths[lane, segment];
        return GetSegmentedDirectionLeft(lane, segment, fraction);
    }

    public Vector3 GetDirectionWhenTraveledRight(int lane, float distanceTraveled)
    {
        if (lane >= rightLaneCount)
        {
            return Vector3.zero;
        }
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = rightSegmentLengths.GetLength(1);
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - rightSegmentLengths[lane, i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= rightSegmentLengths[lane, i];
                }
            }
        }
        float fraction = dist / rightSegmentLengths[lane, segment];
        return GetSegmentedDirectionRight(lane, segment, fraction);
    }

    private Vector3 GetSegmentedDirection(int segment, float fraction)
    {

        return GetSegmentedVelocity(segment, fraction).normalized;
    }

    private Vector3 GetSegmentedDirectionLeft(int lane, int segment, float fraction)
    {

        return GetSegmentedVelocityLeft(lane, segment, fraction).normalized;
    }

    private Vector3 GetSegmentedDirectionRight(int lane, int segment, float fraction)
    {

        return GetSegmentedVelocityRight(lane, segment, fraction).normalized;
    }

    private Vector3 GetSegmentedVelocity(int segment, float fraction)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], fraction))
            - transform.position;
    }

    private Vector3 GetSegmentedVelocityLeft(int lane, int segment, float fraction)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(leftLanePoints[lane, i], leftLanePoints[lane, i + 1],
            leftLanePoints[lane, i + 2], leftLanePoints[lane, i + 3], fraction)) - transform.position;
    }

    private Vector3 GetSegmentedVelocityRight(int lane, int segment, float fraction)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(rightLanePoints[lane, i], rightLanePoints[lane, i + 1],
            rightLanePoints[lane, i + 2], rightLanePoints[lane, i + 3], fraction)) - transform.position;
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
        float fraction = dist / segmentLengths[segment];
        return GetSegmentedPoint(segment, fraction);
    }

    public Vector3 GetPointWhenTraveledLeft(int lane, float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = leftSegmentLengths.GetLength(1);
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - leftSegmentLengths[lane, i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= leftSegmentLengths[lane, i];
                }
            }
        }
        float fraction = dist / leftSegmentLengths[lane, segment];
        return GetSegmentedPointLeft(lane, segment, fraction);
    }

    public Vector3 GetPointWhenTraveledRight(int lane, float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = rightSegmentLengths.GetLength(1);
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - rightSegmentLengths[lane, i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= rightSegmentLengths[lane, i];
                }
            }
        }
        float fraction = dist / rightSegmentLengths[lane, segment];
        return GetSegmentedPointRight(lane, segment, fraction);
    }

    public Vector3 GetSegmentedPoint(int segment, float fraction)
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
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], fraction));
    }

    public Vector3 GetSegmentedPointLeft(int lane, int segment, float fraction)
    {
        if (segment == leftSegmentLengths.GetLength(1))
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
        return transform.TransformPoint(Bezier.GetPoint(leftLanePoints[lane, i], leftLanePoints[lane, i + 1],
            leftLanePoints[lane, i + 2], leftLanePoints[lane, i + 3], fraction));
    }

    public Vector3 GetSegmentedPointRight(int lane, int segment, float fraction)
    {
        if (segment == rightSegmentLengths.GetLength(1))
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
        return transform.TransformPoint(Bezier.GetPoint(rightLanePoints[lane, i], rightLanePoints[lane, i + 1],
            rightLanePoints[lane, i + 2], rightLanePoints[lane, i + 3], fraction));
    }

    public Vector3 GetPoint(float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetPointLeft(int lane, float t)
    {
        if (lane >= LeftLaneCount)
        {
            return Vector3.zero;
        }
        int i = GetI(ref t);
        return transform.TransformPoint(Bezier.GetPoint(leftLanePoints[lane, i], leftLanePoints[lane, i + 1],
            leftLanePoints[lane, i + 2], leftLanePoints[lane, i + 3], t));
    }

    public Vector3 GetPointRight(int lane, float t)
    {
        if (lane >= RightLaneCount)
        {
            return Vector3.zero;
        }
        int i = GetI(ref t);
        return transform.TransformPoint(Bezier.GetPoint(rightLanePoints[lane, i], rightLanePoints[lane, i + 1],
            rightLanePoints[lane, i + 2], rightLanePoints[lane, i + 3], t));
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public Vector3 GetDirectionLeft(int lane, float t)
    {
        return GetVelocityLeft(lane, t).normalized;
    }

    public Vector3 GetDirectionRight(int lane, float t)
    {
        return GetVelocityRight(lane, t).normalized;
    }

    public Vector3 GetVelocity(float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t))
            - transform.position;
    }

    public Vector3 GetVelocityLeft(int lane, float t)
    {
        if (lane >= LeftLaneCount)
        {
            return Vector3.zero;
        }
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(leftLanePoints[lane, i], leftLanePoints[lane, i + 1],
            leftLanePoints[lane, i + 2], leftLanePoints[lane, i + 3], t)) - transform.position;
    }

    public Vector3 GetVelocityRight(int lane, float t)
    {
        if (lane >= RightLaneCount)
        {
            return Vector3.zero;
        }
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(rightLanePoints[lane, i], rightLanePoints[lane, i + 1],
            rightLanePoints[lane, i + 2], rightLanePoints[lane, i + 3], t)) - transform.position;
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

    public void RecalculateLength(int index)
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
            if (segment == 0)
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

    public void RecalculateLengthLeft(int lane, int index)
    {
        int segment = index / 3;
        if (segment == leftSegmentLengths.GetLength(1))
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
        Vector3 prev = GetSegmentedPointLeft(lane, segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointLeft(lane, segment, i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        leftSegmentLengths[lane, segment] = dist;
        if (loop == false && segment == 0)
        {
            UpdateSplineLengthLeft(lane);
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = leftSegmentLengths.GetLength(1) - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPointLeft(lane, segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointLeft(lane, segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            leftSegmentLengths[lane, segment] = dist;
            UpdateSplineLengthLeft(lane);
        }
    }

    public void RecalculateLengthRight(int lane, int index)
    {
        int segment = index / 3;
        if (segment == rightSegmentLengths.GetLength(1))
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
        Vector3 prev = GetSegmentedPointRight(lane, segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointRight(lane, segment, i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        rightSegmentLengths[lane, segment] = dist;
        if (loop == false && segment == 0)
        {
            UpdateSplineLengthRight(lane);
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = rightSegmentLengths.GetLength(1) - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPointRight(lane, segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointRight(lane, segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            rightSegmentLengths[lane, segment] = dist;
            UpdateSplineLengthRight(lane);
        }
    }

    private void UpdateSplineLength()
    {
        float length = 0;
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            length += segmentLengths[i];
        }
        splineLength = length;
    }

    private void UpdateSplineLengthLeft(int lane)
    {
        float length = 0;
        int lLength = leftSegmentLengths.GetLength(1);
        for (int i = 0; i < lLength; i++)
        {
            length += leftSegmentLengths[lane, i];
        }
        leftSplineLengths[lane] = length;
    }

    private void UpdateSplineLengthRight(int lane)
    {
        float length = 0;
        int rLength = rightSegmentLengths.GetLength(1);
        for (int i = 0; i < rLength; i++)
        {
            length += rightSegmentLengths[lane, i];
        }
        rightSplineLengths[lane] = length;
    }

    // tähän jäin+++++++++++++++++++++++++++++++++++++++++++++++++++
    public void AddCurve()
    {
        // First, update the main spline
        Vector3 point = points[points.Length - 1];
        //use the length of the previous segment as a measure for the new one
        float length = segmentLengths[segmentLengths.Length - 1] / 3f;
        //continue to the direction of the previous segment
        Vector3 dir = GetSegmentedDirection(segmentLengths.Length - 1, 1f);
        Debug.Log(dir);
        // Array requires System-namespace. points is passed as a REFERENCE (not a copy)
        // 1. Add new points
        Array.Resize(ref points, points.Length + 3);
        point += length * dir;
        points[points.Length - 3] = point;
        point += length * dir;
        points[points.Length - 2] = point;
        point += length * dir;
        points[points.Length - 1] = point;
        // 2. Resize segmentLengths
        Array.Resize(ref segmentLengths, segmentLengths.Length + 1);
        segmentLengths[segmentLengths.Length - 1] = 3f;
        // 3. Resize modes
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        // 4. Add new right lane points
        int size = points.Length;
        Vector3 rightDir = new Vector3(dir.z, dir.y, -dir.x);

        Vector3[,] newRightPoints = new Vector3[RightLaneCount, size];
        for (int i = 0; i < RightLaneCount; i++)
        {
            for (int j = 0; j < size - 3; j++)
            {
                newRightPoints[i, j] = rightLanePoints[i, j];
            }
            Vector3 spacing = rightSpacings[i, rightSpacings.GetLength(1) - 1] * rightDir;
            newRightPoints[i, size - 3] = points[size - 3] + spacing;
            newRightPoints[i, size - 2] = points[size - 2] + spacing;
            newRightPoints[i, size - 1] = points[size - 1] + spacing;
        }
        rightLanePoints = newRightPoints;

        // 5. Add new left lane points
        Vector3 leftDir = new Vector3(-dir.z, dir.y, dir.x);
        Vector3[,] newLeftPoints = new Vector3[LeftLaneCount, size];

        for (int i = 0; i < LeftLaneCount; i++)
        {
            Vector3 spacing = leftSpacings[i, 0] * leftDir;
            newLeftPoints[i, 0] = points[size - 1] + spacing;
            newLeftPoints[i, 1] = points[size - 2] + spacing;
            newLeftPoints[i, 2] = points[size - 3] + spacing;

            for (int j = 0; j < size - 3; j++)
            {
                newLeftPoints[i, j + 3] = leftLanePoints[i, j];
            }
        }
        leftLanePoints = newLeftPoints;

        // Add new spacings, copy previous value
        // 6. Update right spacings
        size = SegmentCount;
        float[,] newRightSpacings = new float[RightLaneCount, size];
        for (int i = 0; i < RightLaneCount; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                newRightSpacings[i, j] = rightSpacings[i, j];
            }
            newRightSpacings[i, size - 1] = rightSpacings[i, size - 2];
        }
        rightSpacings = newRightSpacings;
        // 7. Update left spacings
        float[,] newLeftSpacings = new float[LeftLaneCount, size];
        for (int i = 0; i < LeftLaneCount; i++)
        {
            newLeftSpacings[i, 0] = leftSpacings[i, 0];
            for (int j = 0; j < size - 1; j++)
            {
                newLeftSpacings[i, j + 1] = leftSpacings[i, j];
            }
        }
        leftSpacings = newLeftSpacings;
        // 8. Resize right segments
        float[,] newRightSegments = new float[RightLaneCount, size];
        // 9. Resize left segments
        float[,] newLeftSegments = new float[LeftLaneCount, size];
        // 10. Update right modes
        BezierControlPointMode[,] newRightModes = new BezierControlPointMode[RightLaneCount, size];
        // 11. Update left modes
        BezierControlPointMode[,] newLeftModes = new BezierControlPointMode[LeftLaneCount, size];

        //***************Update following
        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
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
            else if (modeIndex == modes.Length - 1)
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

        splineLength = Vector3.Distance(points[0], points[3]);
        segmentLengths = new float[] { splineLength }; //jos ei alusteta suoralla pitää muuttaa

    }
}

