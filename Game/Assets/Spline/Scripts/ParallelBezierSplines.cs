using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ParallelBezierSplines : MonoBehaviour
{

    [SerializeField]
    private bool loop;
    [SerializeField]
    private bool initialized;

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    private Vector3[] leftLanePoints1;
    [SerializeField]
    private Vector3[] leftLanePoints2;
    [SerializeField]
    private Vector3[] leftLanePoints3;
    [SerializeField]
    private Vector3[] rightLanePoints1;
    [SerializeField]
    private Vector3[] rightLanePoints2;
    [SerializeField]
    private Vector3[] rightLanePoints3;


    [SerializeField]
    private float[] segmentLengths;
    
    [SerializeField]
    private float[] leftSegmentLengths1;
    [SerializeField]
    private float[] leftSegmentLengths2;
    [SerializeField]
    private float[] leftSegmentLengths3;
    [SerializeField]
    private float[] rightSegmentLengths1;
    [SerializeField]
    private float[] rightSegmentLengths2;
    [SerializeField]
    private float[] rightSegmentLengths3;

    [SerializeField]
    private float splineLength;
    
    [SerializeField]
    private float[] leftSplineLengths;
    [SerializeField]
    private float[] rightSplineLengths;
    
    
    [SerializeField]
    private float[] leftSpacings1;
    [SerializeField]
    private float[] leftSpacings2;
    [SerializeField]
    private float[] leftSpacings3;
    [SerializeField]
    private float[] rightSpacings1;
    [SerializeField]
    private float[] rightSpacings2;
    [SerializeField]
    private float[] rightSpacings3;

    [SerializeField]
    private BezierControlPointMode[] modes;
    
    [SerializeField]
    private BezierControlPointMode[] leftModes1;
    [SerializeField]
    private BezierControlPointMode[] leftModes2;
    [SerializeField]
    private BezierControlPointMode[] leftModes3;

    [SerializeField]
    private BezierControlPointMode[] rightModes1;
    [SerializeField]
    private BezierControlPointMode[] rightModes2;
    [SerializeField]
    private BezierControlPointMode[] rightModes3;

    [SerializeField]
    public List<GameObject> wayPointsLeft1;
    [SerializeField]
    public List<GameObject> wayPointsLeft2;
    [SerializeField]
    public List<GameObject> wayPointsLeft3;
    [SerializeField]
    public List<GameObject> wayPointsRight1;
    [SerializeField]
    public List<GameObject> wayPointsRight2;
    [SerializeField]
    public List<GameObject> wayPointsRight3;
    [SerializeField]
    public GameObject waypointParent;
    [SerializeField]
    public GameObject leftParent1;
    [SerializeField]
    public GameObject leftParent2;
    [SerializeField]
    public GameObject leftParent3;
    [SerializeField]
    public GameObject rightParent1;
    [SerializeField]
    public GameObject rightParent2;
    [SerializeField]
    public GameObject rightParent3;

    [SerializeField]
    private int leftLaneCount;
    [SerializeField]
    private int rightLaneCount;
    


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

    public bool Initialized
    {
        get
        {
            return initialized;
        }
        set
        {
            initialized = value;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LeftLaneCount = leftLaneCount;
        RightLaneCount = rightLaneCount;
        //LanePositioning = lanePositioning;
    }

#endif

    public Vector3 GetStartPoint()
    {
        return points[0];
    }

    public Vector3 GetLeftLaneStartPoint(int lane)
    {
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = leftLanePoints1[0];
                break;
            case 1:
                v = leftLanePoints2[0];
                break;
            case 2:
                v = leftLanePoints3[0];
                break;
        }
        return v;
    }

    public Vector3 GetRightLaneStartPoint(int lane)
    {
        Vector2 v = Vector3.zero;
        switch(lane)
        {
            case 0:
                v = rightLanePoints1[0];
                break;
            case 1:
                v = rightLanePoints2[0];
                break;
            case 2:
                v = rightLanePoints3[0];
                break;
        }
        return v;
    }

    public Vector3 GetEndPoint()
    {
        return points[points.Length - 1];
    }

    public Vector3 GetLeftEndPoint(int lane)
    {
        Vector3 v = Vector3.zero;
        switch(lane)
        {
            case 0:
                v = leftLanePoints1[leftLanePoints1.Length - 1];
                break;
            case 1:
                v = leftLanePoints2[leftLanePoints2.Length - 1];
                break;
            case 2:
                v = leftLanePoints3[leftLanePoints3.Length - 1];
                break;
        }
        return v;
    }

    public Vector3 GetRightEndPoint(int lane)
    {
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = rightLanePoints1[rightLanePoints1.Length - 1];
                break;
            case 1:
                v = rightLanePoints2[rightLanePoints2.Length - 1];
                break;
            case 2:
                v = rightLanePoints3[rightLanePoints3.Length - 1];
                break;
        }
        return v;
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
                SetControlPointRight(0, 0, rightLanePoints1[0]);
                SetControlPointRight(1, 0, rightLanePoints2[0]);
                SetControlPointRight(2, 0, rightLanePoints3[0]);
                int ind = ControlPointCount - 1;
                SetControlPointLeft(0, ind, leftLanePoints1[ind]);
                SetControlPointLeft(1, ind, leftLanePoints2[ind]);
                SetControlPointLeft(2, ind, leftLanePoints3[ind]);

            }
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public Vector3 GetControlPointLeft(int lane, int index)
    {
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = leftLanePoints1[index];
                break;
            case 1:
                v = leftLanePoints2[index];
                break;
            case 2:
                v = leftLanePoints2[index];
                break;
        }
        return v;
    }

    public Vector3 GetControlPointRight(int lane, int index)
    {
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = rightLanePoints1[index];
                break;
            case 1:
                v = rightLanePoints2[index];
                break;
            case 2:
                v = rightLanePoints2[index];
                break;
        }
        return v;
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
        float f = 0f;
        switch (lane)
        {
            case 0:
                f = leftSegmentLengths1[segment];
                break;
            case 1:
                f = leftSegmentLengths2[segment];
                break;
            case 2:
                f = leftSegmentLengths3[segment];
                break;
        }
        return f;
    }

    public float GetRightSegmentLength(int lane, int index)
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
        float f = 0f;
        switch (lane)
        {
            case 0:
                f = rightSegmentLengths1[segment];
                break;
            case 1:
                f = rightSegmentLengths2[segment];
                break;
            case 2:
                f = rightSegmentLengths3[segment];
                break;
        }
        return f;
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public float GetRightSpacing(int lane, int node)
    {
        float sp = 0f;
        switch(lane)
        {
            case 0:
                sp = rightSpacings1[node / 3];
                break;
            case 1:
                sp = rightSpacings2[node / 3];
                break;
            case 2:
                sp = rightSpacings3[node / 3];
                break;
        }
        return sp;
    }

    public void SetRightSpacing(int lane, int node, float spacing)
    {
        switch (lane)
        {
            case 0:
                rightSpacings1[node / 3] = spacing;
                break;
            case 1:
                rightSpacings2[node / 3] = spacing;
                break;
            case 2:
                rightSpacings3[node / 3] = spacing;
                break;
        }
    }

    public float GetLeftSpacing(int lane, int node)
    {
        float sp = 0f;
        switch (lane)
        {
            case 0:
                sp = leftSpacings1[node / 3];
                break;
            case 1:
                sp = leftSpacings2[node / 3];
                break;
            case 2:
                sp = leftSpacings3[node / 3];
                break;
        }
        return sp;
    }

    public void SetLeftSpacing(int lane, int node, float spacing)
    {
        switch (lane)
        {
            case 0:
                leftSpacings1[node / 3] = spacing;
                break;
            case 1:
                leftSpacings2[node / 3] = spacing;
                break;
            case 2:
                leftSpacings3[node / 3] = spacing;
                break;
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

    public void SetControlPointLeft(int lane, int index,  Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point;
            switch (lane)
            {
                case 0:
                    delta -= leftLanePoints1[index];
                    break;
                case 1:
                    delta -= leftLanePoints2[index];
                    break;
                case 2:
                    delta -= leftLanePoints3[index];
                    break;
            }
            if (loop)
            {
                if (index == 0)
                {
                    int pLength = ControlPointCount;
                    switch (lane)
                    {
                        case 0:
                            leftLanePoints1[1] += delta;
                            leftLanePoints1[pLength - 2] += delta;
                            leftLanePoints1[pLength - 1] = point;
                            break;
                        case 1:
                            leftLanePoints2[1] += delta;
                            leftLanePoints2[pLength - 2] += delta;
                            leftLanePoints2[pLength - 1] = point;
                            break;
                        case 2:
                            leftLanePoints3[1] += delta;
                            leftLanePoints3[pLength - 2] += delta;
                            leftLanePoints3[pLength - 1] = point;
                            break;
                    }
                }
                else if (index == points.Length - 1)
                {
                    switch (lane)
                    {
                        case 0:
                            leftLanePoints1[0] = point;
                            leftLanePoints1[1] += delta;
                            leftLanePoints1[index - 1] += delta;
                            break;
                        case 1:
                            leftLanePoints2[0] = point;
                            leftLanePoints2[1] += delta;
                            leftLanePoints2[index - 1] += delta;
                            break;
                        case 2:
                            leftLanePoints3[0] = point;
                            leftLanePoints3[1] += delta;
                            leftLanePoints3[index - 1] += delta;
                            break;
                    }
                }
                else
                {
                    switch (lane)
                    {
                        case 0:
                            leftLanePoints1[index - 1] += delta;
                            leftLanePoints1[index + 1] += delta;
                            break;
                        case 1:
                            leftLanePoints2[index - 1] += delta;
                            leftLanePoints2[index + 1] += delta;
                            break;
                        case 2:
                            leftLanePoints3[index - 1] += delta;
                            leftLanePoints3[index + 1] += delta;
                            break;
                    }
                }
            }
            else
            {
                if (index > 0)
                {
                    switch (lane)
                    {
                        case 0:
                            leftLanePoints1[index - 1] += delta;
                            break;
                        case 1:
                            leftLanePoints2[index - 1] += delta;
                            break;
                        case 2:
                            leftLanePoints3[index - 1] += delta;
                            break;
                    }
                }
                if (index + 1 < points.Length)
                {
                    switch (lane)
                    {
                        case 0:
                            leftLanePoints1[index + 1] += delta;
                            break;
                        case 1:
                            leftLanePoints2[index + 1] += delta;
                            break;
                        case 2:
                            leftLanePoints3[index + 1] += delta;
                            break;
                    }
                }
            }
        }
        //***********
        switch (lane)
        {
            case 0:
                leftLanePoints1[index] = point;
                break;
            case 1:
                leftLanePoints2[index] = point;
                break;
            case 2:
                leftLanePoints3[index] = point;
                break;
        }
        EnforceModeLeft(lane, index);
    }

    public void SetControlPointRight(int lane, int index, Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point;
            switch (lane)
            {
                case 0:
                    delta -= rightLanePoints1[index];
                    break;
                case 1:
                    delta -= rightLanePoints2[index];
                    break;
                case 2:
                    delta -= rightLanePoints3[index];
                    break;
            }
            if (loop)
            {
                if (index == 0)
                {
                    int pLength = ControlPointCount;
                    switch (lane)
                    {
                        case 0:
                            rightLanePoints1[1] += delta;
                            rightLanePoints1[pLength - 2] += delta;
                            rightLanePoints1[pLength - 1] = point;
                            break;
                        case 1:
                            rightLanePoints2[1] += delta;
                            rightLanePoints2[pLength - 2] += delta;
                            rightLanePoints2[pLength - 1] = point;
                            break;
                        case 2:
                            rightLanePoints3[1] += delta;
                            rightLanePoints3[pLength - 2] += delta;
                            rightLanePoints3[pLength - 1] = point;
                            break;
                    }
                }
                else if (index == points.Length - 1)
                {
                    switch (lane)
                    {
                        case 0:
                            rightLanePoints1[0] = point;
                            rightLanePoints1[1] += delta;
                            rightLanePoints1[index - 1] += delta;
                            break;
                        case 1:
                            rightLanePoints2[0] = point;
                            rightLanePoints2[1] += delta;
                            rightLanePoints2[index - 1] += delta;
                            break;
                        case 2:
                            rightLanePoints3[0] = point;
                            rightLanePoints3[1] += delta;
                            rightLanePoints3[index - 1] += delta;
                            break;
                    }
                }
                else
                {
                    switch (lane)
                    {
                        case 0:
                            rightLanePoints1[index - 1] += delta;
                            rightLanePoints1[index + 1] += delta;
                            break;
                        case 1:
                            rightLanePoints2[index - 1] += delta;
                            rightLanePoints2[index + 1] += delta;
                            break;
                        case 2:
                            rightLanePoints3[index - 1] += delta;
                            rightLanePoints3[index + 1] += delta;
                            break;
                    }
                }
            }
            else
            {
                if (index > 0)
                {
                    switch (lane)
                    {
                        case 0:
                            rightLanePoints1[index - 1] += delta;
                            break;
                        case 1:
                            rightLanePoints2[index - 1] += delta;
                            break;
                        case 2:
                            rightLanePoints3[index - 1] += delta;
                            break;
                    }
                }
                if (index + 1 < points.Length)
                {
                    switch (lane)
                    {
                        case 0:
                            rightLanePoints1[index + 1] += delta;
                            break;
                        case 1:
                            rightLanePoints2[index + 1] += delta;
                            break;
                        case 2:
                            rightLanePoints3[index + 1] += delta;
                            break;
                    }
                }
            }
        }
        //***********
        switch (lane)
        {
            case 0:
                rightLanePoints1[index] = point;
                break;
            case 1:
                rightLanePoints2[index] = point;
                break;
            case 2:
                rightLanePoints3[index] = point;
                break;
        }
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
        BezierControlPointMode mode = BezierControlPointMode.Aligned;
        switch (lane)
        {
            case 0:
                mode = leftModes1[modeIndex];
                break;
            case 1:
                mode = leftModes2[modeIndex];
                break;
            case 2:
                mode = leftModes3[modeIndex];
                break;
        }
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
                fixedIndex = ControlPointCount - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= ControlPointCount)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= ControlPointCount)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = ControlPointCount - 2;
            }
        }
        Vector3 middle = Vector3.zero;
        Vector3 enforcedTangent = Vector3.zero;
        switch (lane)
        {
            case 0:
                middle = leftLanePoints1[middleIndex];
                enforcedTangent = middle - leftLanePoints1[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, leftLanePoints1[enforcedIndex]);
                }
                leftLanePoints1[enforcedIndex] = middle + enforcedTangent;
                break;
            case 1:
                middle = leftLanePoints2[middleIndex];
                enforcedTangent = middle - leftLanePoints2[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, leftLanePoints2[enforcedIndex]);
                }
                leftLanePoints2[enforcedIndex] = middle + enforcedTangent;
                break;
            case 2:
                middle = leftLanePoints3[middleIndex];
                enforcedTangent = middle - leftLanePoints3[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, leftLanePoints3[enforcedIndex]);
                }
                leftLanePoints1[enforcedIndex] = middle + enforcedTangent;
                break;
        }
    }

    private void EnforceModeRight(int lane, int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = BezierControlPointMode.Aligned;
        switch (lane)
        {
            case 0:
                mode = rightModes1[modeIndex];
                break;
            case 1:
                mode = rightModes2[modeIndex];
                break;
            case 2:
                mode = rightModes3[modeIndex];
                break;
        }
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
                fixedIndex = ControlPointCount - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= ControlPointCount)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= ControlPointCount)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = ControlPointCount - 2;
            }
        }
        Vector3 middle = Vector3.zero;
        Vector3 enforcedTangent = Vector3.zero;
        switch (lane)
        {
            case 0:
                middle = rightLanePoints1[middleIndex];
                enforcedTangent = middle - rightLanePoints1[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, rightLanePoints1[enforcedIndex]);
                }
                rightLanePoints1[enforcedIndex] = middle + enforcedTangent;
                break;
            case 1:
                middle = rightLanePoints2[middleIndex];
                enforcedTangent = middle - rightLanePoints2[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, rightLanePoints2[enforcedIndex]);
                }
                rightLanePoints2[enforcedIndex] = middle + enforcedTangent;
                break;
            case 2:
                middle = rightLanePoints3[middleIndex];
                enforcedTangent = middle - rightLanePoints3[fixedIndex];
                if (mode == BezierControlPointMode.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized *
                        Vector3.Distance(middle, rightLanePoints3[enforcedIndex]);
                }
                rightLanePoints3[enforcedIndex] = middle + enforcedTangent;
                break;
        }
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
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float length = 0f;
                switch (lane)
                {
                    case 0:
                        length = leftSegmentLengths1[i];
                        break;
                    case 1:
                        length = leftSegmentLengths2[i];
                        break;
                    case 2:
                        length = leftSegmentLengths3[i];
                        break;
                }
                if (dist - length  < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= length;
                }
            }
        }
        float fraction = 0f;
        switch (lane)
        {
            case 0:
                fraction = dist / leftSegmentLengths1[segment];
                break;
            case 1:
                fraction = dist / leftSegmentLengths2[segment];
                break;
            case 2:
                fraction = dist / leftSegmentLengths3[segment];
                break;
        }
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
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float length = 0f;
                switch (lane)
                {
                    case 0:
                        length = rightSegmentLengths1[i];
                        break;
                    case 1:
                        length = rightSegmentLengths2[i];
                        break;
                    case 2:
                        length = rightSegmentLengths3[i];
                        break;
                }
                if (dist - length < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= length;
                }
            }
        }
        float fraction = 0f;
        switch (lane)
        {
            case 0:
                fraction = dist / rightSegmentLengths1[segment];
                break;
            case 1:
                fraction = dist / rightSegmentLengths2[segment];
                break;
            case 2:
                fraction = dist / rightSegmentLengths3[segment];
                break;
        }
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
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints1[i],
                    leftLanePoints1[i + 1], leftLanePoints1[i + 2], leftLanePoints1[i + 3],
                    fraction)) - transform.position;
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints2[i],
                    leftLanePoints2[i + 1], leftLanePoints2[i + 2], leftLanePoints2[i + 3],
                    fraction)) - transform.position;
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints3[i],
                    leftLanePoints3[i + 1], leftLanePoints3[i + 2], leftLanePoints3[i + 3],
                    fraction)) - transform.position;
                break;
        }
        return v;
    }

    private Vector3 GetSegmentedVelocityRight(int lane, int segment, float fraction)
    {
        int i = segment * 3;
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints1[i],
                    rightLanePoints1[i + 1], rightLanePoints1[i + 2], rightLanePoints1[i + 3],
                    fraction)) - transform.position;
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints2[i],
                    rightLanePoints2[i + 1], rightLanePoints2[i + 2], rightLanePoints2[i + 3],
                    fraction)) - transform.position;
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints3[i],
                    rightLanePoints3[i + 1], rightLanePoints3[i + 2], rightLanePoints3[i + 3],
                    fraction)) - transform.position;
                break;
        }
        return v;
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
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float length = 0f;
                switch(lane)
                {
                    case 0:
                        length = leftSegmentLengths1[i];
                        break;
                    case 1:
                        length = leftSegmentLengths2[i];
                        break;
                    case 2:
                        length = leftSegmentLengths3[i];
                        break;
                }
                if (dist - length < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= length;
                }
            }
        }
        float fraction = 0f;
        switch(lane)
        {
            case 0:
                fraction = dist / leftSegmentLengths1[segment];
                break;
            case 1:
                fraction = dist / leftSegmentLengths2[segment];
                break;
            case 2:
                fraction = dist / leftSegmentLengths2[segment];
                break;
        }
        return GetSegmentedPointLeft(lane, segment, fraction);
    }

    public Vector3 GetPointWhenTraveledRight(int lane, float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float length = 0f;
                switch (lane)
                {
                    case 0:
                        length = rightSegmentLengths1[i];
                        break;
                    case 1:
                        length = rightSegmentLengths2[i];
                        break;
                    case 2:
                        length = rightSegmentLengths3[i];
                        break;
                }
                if (dist - length < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= length;
                }
            }
        }
        float fraction = 0f;
        switch (lane)
        {
            case 0:
                fraction = dist / rightSegmentLengths1[segment];
                break;
            case 1:
                fraction = dist / rightSegmentLengths2[segment];
                break;
            case 2:
                fraction = dist / rightSegmentLengths3[segment];
                break;
        }
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
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints1[i], leftLanePoints1[i + 1],
                    leftLanePoints1[i + 2], leftLanePoints1[i + 3], fraction));
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints2[i], leftLanePoints2[i + 1],
                    leftLanePoints2[i + 2], leftLanePoints2[i + 3], fraction));
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints3[i], leftLanePoints3[i + 1],
                    leftLanePoints3[i + 2], leftLanePoints3[i + 3], fraction));
                break;
        }
        return v;
    }

    public Vector3 GetSegmentedPointRight(int lane, int segment, float fraction)
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
        Vector3 v = Vector3.zero;
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints1[i], rightLanePoints1[i + 1],
                    rightLanePoints1[i + 2], rightLanePoints1[i + 3], fraction));
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints2[i], rightLanePoints2[i + 1],
                    rightLanePoints2[i + 2], rightLanePoints2[i + 3], fraction));
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints3[i], rightLanePoints3[i + 1],
                    rightLanePoints3[i + 2], rightLanePoints3[i + 3], fraction));
                break;
        }
        return v;
    }

    public Vector3 GetPoint(float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetPointLeft(int lane, float t)
    {
        Vector3 v = Vector3.zero;
        if (lane >= LeftLaneCount)
        {
            return v;
        }
        int i = GetI(ref t);
        switch(lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints1[i],
                    leftLanePoints1[i + 1], leftLanePoints1[i + 2], leftLanePoints1[i + 3], t));
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints2[i],
                    leftLanePoints2[i + 1], leftLanePoints2[i + 2], leftLanePoints2[i + 3], t));
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints1[i],
                    leftLanePoints3[i + 1], leftLanePoints3[i + 2], leftLanePoints3[i + 3], t));
                break;
        }
        return v;
    }

    public Vector3 GetPointRight(int lane, float t)
    {
        Vector3 v = Vector3.zero;
        if (lane >= RightLaneCount)
        {
            return v;
        }
        int i = GetI(ref t);
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints1[i], rightLanePoints1[1 + 1],
                    rightLanePoints1[i + 2], rightLanePoints1[i + 3], t));
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints2[i], rightLanePoints2[1 + 1],
                    rightLanePoints2[i + 2], rightLanePoints2[i + 3], t));
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints3[i], rightLanePoints3[1 + 1],
                    rightLanePoints3[i + 2], rightLanePoints3[i + 3], t));
                break;
        }
        return v;
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
        Vector3 v = Vector3.zero;
        if (lane >= LeftLaneCount)
        {
            return v;
        }
        int i = GetI(ref t);
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints1[i],
                    leftLanePoints1[i + 1], leftLanePoints1[i + 2], leftLanePoints1[i + 3], t))
                    - transform.position;
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints2[i],
                    leftLanePoints2[i + 1], leftLanePoints2[i + 2], leftLanePoints2[i + 3], t))
                    - transform.position;
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints3[i],
                    leftLanePoints3[i + 1], leftLanePoints3[i + 2], leftLanePoints3[i + 3], t))
                    - transform.position;
                break;
        }
        return v;
    }

    public Vector3 GetVelocityRight(int lane, float t)
    {
        Vector3 v = Vector3.zero;
        if (lane >= RightLaneCount)
        {
            return v;
        }
        int i = GetI(ref t);
        switch (lane)
        {
            case 0:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints1[i],
                    rightLanePoints1[i + 1], rightLanePoints1[i + 2], rightLanePoints1[i + 3], t))
                    - transform.position;
                break;
            case 1:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints2[i],
                    rightLanePoints2[i + 1], rightLanePoints2[i + 2], rightLanePoints2[i + 3], t))
                    - transform.position;
                break;
            case 2:
                v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints3[i],
                    rightLanePoints3[i + 1], rightLanePoints3[i + 2], rightLanePoints3[i + 3], t))
                    - transform.position;
                break;
        }
        return v;
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
            Vector3 next = GetSegmentedPoint(segment, (float)i / 1000f);
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
                Debug.Log("segment: " + segment);
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
        Vector3 prev = GetSegmentedPointLeft(lane, segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointLeft(lane, segment, i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        switch (lane)
        {
            case 0:
                leftSegmentLengths1[segment] = dist;
                break;
            case 1:
                leftSegmentLengths2[segment] = dist;
                break;
            case 2:
                leftSegmentLengths3[segment] = dist;
                break;
        }
        if (loop == false && segment == 0)
        {
            UpdateSplineLengthLeft(lane);
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
            prev = GetSegmentedPointLeft(lane, segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointLeft(lane, segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            switch (lane)
            {
                case 0:
                    leftSegmentLengths1[segment] = dist;
                    break;
                case 1:
                    leftSegmentLengths2[segment] = dist;
                    break;
                case 2:
                    leftSegmentLengths3[segment] = dist;
                    break;
            }
            UpdateSplineLengthLeft(lane);
        }
    }

    public void RecalculateLengthRight(int lane, int index)
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
        Vector3 prev = GetSegmentedPointRight(lane, segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointRight(lane, segment, i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        switch (lane)
        {
            case 0:
                rightSegmentLengths1[segment] = dist;
                break;
            case 1:
                rightSegmentLengths2[segment] = dist;
                break;
            case 2:
                rightSegmentLengths3[segment] = dist;
                break;
        }
        if (loop == false && segment == 0)
        {
            UpdateSplineLengthRight(lane);
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
            prev = GetSegmentedPointRight(lane, segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointRight(lane, segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            switch (lane)
            {
                case 0:
                    rightSegmentLengths1[segment] = dist;
                    break;
                case 1:
                    rightSegmentLengths2[segment] = dist;
                    break;
                case 2:
                    rightSegmentLengths2[segment] = dist;
                    break;
            }
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
        switch(lane)
        {
            case 0:
                for (int i = 0; i < leftSegmentLengths1.Length; i++)
                {
                    length += leftSegmentLengths1[i];
                }
                leftSplineLengths[lane] = length;
                break;
            case 1:
                for (int i = 0; i < leftSegmentLengths2.Length; i++)
                {
                    length += leftSegmentLengths2[i];
                }
                leftSplineLengths[lane] = length;
                break;
            case 2:
                for (int i = 0; i < leftSegmentLengths3.Length; i++)
                {
                    length += leftSegmentLengths3[i];
                }
                leftSplineLengths[lane] = length;
                break;
        }
    }

    private void UpdateSplineLengthRight(int lane)
    {
        float length = 0;
        switch (lane)
        {
            case 0:
                for (int i = 0; i < rightSegmentLengths1.Length; i++)
                {
                    length += rightSegmentLengths1[i];
                }
                rightSplineLengths[lane] = length;
                break;
            case 1:
                for (int i = 0; i < rightSegmentLengths2.Length; i++)
                {
                    length += rightSegmentLengths2[i];
                }
                rightSplineLengths[lane] = length;
                break;
            case 2:
                for (int i = 0; i < rightSegmentLengths3.Length; i++)
                {
                    length += rightSegmentLengths3[i];
                }
                rightSplineLengths[lane] = length;
                break;
        }
    }

    public void AddCurve()
    {
        // First, update the main spline
        Vector3 point = points[points.Length - 1];
        //use the length of the previous segment as a measure for the new one
        float length = segmentLengths[segmentLengths.Length - 1] / 3f;
        //continue to the direction of the previous segment
        Vector3 dir = GetSegmentedDirection(segmentLengths.Length - 1, 1f);
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
        float lastSegmentLegth = Vector3.Distance(points[points.Length - 4], points[points.Length - 1]);
        segmentLengths[segmentLengths.Length - 1] = lastSegmentLegth;
        // 3. Resize modes
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        // 4. Add new right lane points
        //int size = points.Length;
        Vector3 rightDir = new Vector3(dir.z, dir.y, -dir.x);
        //***********
        float space = 0f;
        Vector3 spacing;
        //rightLanePoints1
        Array.Resize(ref rightLanePoints1, rightLanePoints1.Length + 3);
        space += rightSpacings1[rightSpacings1.Length - 1];
        spacing = space * rightDir;
        rightLanePoints1[rightLanePoints1.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints2
        Array.Resize(ref rightLanePoints2, rightLanePoints2.Length + 3);
        space += rightSpacings2[rightSpacings2.Length - 1];
        spacing = space * rightDir;
        rightLanePoints2[rightLanePoints2.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints3
        Array.Resize(ref rightLanePoints3, rightLanePoints3.Length + 3);
        space += rightSpacings3[rightSpacings3.Length - 1];
        spacing = space * rightDir;
        rightLanePoints3[rightLanePoints3.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 1] = points[points.Length - 1] + spacing;

        // 5. Add new left lane points
        space = 0f;
        //leftLanePoints1
        Array.Resize(ref leftLanePoints1, leftLanePoints1.Length + 3);
        for (int i = leftLanePoints1.Length - 4; i >= 0; i--)
        {
            leftLanePoints1[i + 3] = leftLanePoints1[i];
        }
        space += leftSpacings1[leftSpacings1.Length - 1];
        spacing = rightDir * space;
        leftLanePoints1[0] = points[points.Length - 1] - spacing;
        leftLanePoints1[1] = points[points.Length - 2] - spacing;
        leftLanePoints1[2] = points[points.Length - 3] - spacing;
        //leftLanePoints2
        Array.Resize(ref leftLanePoints2, leftLanePoints2.Length + 3);
        for (int i = leftLanePoints2.Length - 4; i >= 0; i--)
        {
            leftLanePoints2[i + 3] = leftLanePoints2[i];
        }
        space += leftSpacings2[leftSpacings2.Length - 1];
        spacing = rightDir * space;
        leftLanePoints2[0] = points[points.Length - 1] - spacing;
        leftLanePoints2[1] = points[points.Length - 2] - spacing;
        leftLanePoints2[2] = points[points.Length - 3] - spacing;
        //leftLanePoints3
        Array.Resize(ref leftLanePoints3, leftLanePoints3.Length + 3);
        for (int i = leftLanePoints3.Length - 4; i >= 0; i--)
        {
            leftLanePoints3[i + 3] = leftLanePoints3[i];
        }
        space += leftSpacings3[leftSpacings3.Length - 1];
        spacing = rightDir * space;
        leftLanePoints3[0] = points[points.Length - 1] - spacing;
        leftLanePoints3[1] = points[points.Length - 2] - spacing;
        leftLanePoints3[2] = points[points.Length - 3] - spacing;

        // Add new spacings, copy previous value
        // 6. Update right spacings
        //rightSpacings1
        Array.Resize(ref rightSpacings1, rightSpacings1.Length + 1);
        rightSpacings1[rightSpacings1.Length - 1] = rightSpacings1[rightSpacings1.Length - 2];
        //rightSpacings2
        Array.Resize(ref rightSpacings2, rightSpacings2.Length + 1);
        rightSpacings2[rightSpacings2.Length - 1] = rightSpacings2[rightSpacings2.Length - 2];
        //rightSpacings3
        Array.Resize(ref rightSpacings3, rightSpacings3.Length + 1);
        rightSpacings3[rightSpacings3.Length - 1] = rightSpacings3[rightSpacings3.Length - 2];

        // 7. Update left spacings
        //leftSpacings1
        Array.Resize(ref leftSpacings1, leftSpacings1.Length + 1);
        for (int i = leftSpacings1.Length - 2; i >= 0; i--)
        {
            leftSpacings1[i + 1] = leftSpacings1[i];
        }
        leftSpacings1[0] = leftSpacings1[1];
        //leftSpacings2
        Array.Resize(ref leftSpacings2, leftSpacings2.Length + 1);
        for (int i = leftSpacings2.Length - 2; i >= 0; i--)
        {
            leftSpacings2[i + 1] = leftSpacings2[i];
        }
        leftSpacings2[0] = leftSpacings2[1];
        //leftSpacings3
        Array.Resize(ref leftSpacings3, leftSpacings3.Length + 1);
        for (int i = leftSpacings3.Length - 2; i >= 0; i--)
        {
            leftSpacings3[i + 1] = leftSpacings3[i];
        }
        leftSpacings3[0] = leftSpacings3[1];

        // 8. Resize right segments
        //rightSegmentLengths1
        Array.Resize(ref rightSegmentLengths1, rightSegmentLengths1.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints1[rightLanePoints1.Length - 4],
            rightLanePoints1[rightLanePoints1.Length - 1]);
        rightSegmentLengths1[rightSegmentLengths1.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths2
        Array.Resize(ref rightSegmentLengths2, rightSegmentLengths2.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints2[rightLanePoints2.Length - 4],
            rightLanePoints2[rightLanePoints2.Length - 1]);
        rightSegmentLengths2[rightSegmentLengths2.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths3
        Array.Resize(ref rightSegmentLengths3, rightSegmentLengths3.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints3[rightLanePoints3.Length - 4],
            rightLanePoints3[rightLanePoints3.Length - 1]);
        rightSegmentLengths3[rightSegmentLengths3.Length - 1] = lastSegmentLegth;

        // 9. Resize left segments
        //leftSegmentLengths1
        Array.Resize(ref leftSegmentLengths1, leftSegmentLengths1.Length + 1);
        for (int i = leftSegmentLengths1.Length -1; i > 0; i--)
        {
            leftSegmentLengths1[i] = leftSegmentLengths1[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints1[0], leftLanePoints1[3]);
        leftSegmentLengths1[0] = lastSegmentLegth;
        //leftSegmentlengths2
        Array.Resize(ref leftSegmentLengths2, leftSegmentLengths2.Length + 1);
        for (int i = leftSegmentLengths2.Length - 1; i > 0; i--)
        {
            leftSegmentLengths2[i] = leftSegmentLengths2[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints2[0], leftLanePoints2[3]);
        leftSegmentLengths2[0] = lastSegmentLegth;
        //leftSegmentLengths3
        Array.Resize(ref leftSegmentLengths3, leftSegmentLengths3.Length + 1);
        for (int i = leftSegmentLengths3.Length - 1; i > 0; i--)
        {
            leftSegmentLengths3[i] = leftSegmentLengths3[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints3[0], leftLanePoints3[3]);
        leftSegmentLengths3[0] = lastSegmentLegth;

        // 10. Update right modes
        //rightModes1
        Array.Resize(ref rightModes1, rightModes1.Length + 1);
        rightModes1[rightModes1.Length - 1] = rightModes1[rightModes1.Length - 2];
        //rightModes2
        Array.Resize(ref rightModes2, rightModes2.Length + 1);
        rightModes2[rightModes2.Length - 1] = rightModes2[rightModes2.Length - 2];
        //rightModes3
        Array.Resize(ref rightModes3, rightModes3.Length + 1);
        rightModes1[rightModes3.Length - 1] = rightModes3[rightModes3.Length - 2];

        // 11. Update left modes
        //leftModes1
        Array.Resize(ref leftModes1, leftModes1.Length + 1);
        for (int i = leftModes1.Length - 1; i > 0; i--)
        {
            leftModes1[i] = leftModes1[i - 1];
        }
        leftModes1[0] = leftModes1[1];
        //leftModes2
        Array.Resize(ref leftModes2, leftModes2.Length + 1);
        for (int i = leftModes2.Length - 1; i > 0; i--)
        {
            leftModes2[i] = leftModes2[i - 1];
        }
        leftModes2[0] = leftModes2[1];
        //leftModes1
        Array.Resize(ref leftModes3, leftModes3.Length + 1);
        for (int i = leftModes3.Length - 1; i > 0; i--)
        {
            leftModes3[i] = leftModes3[i - 1];
        }
        leftModes3[0] = leftModes3[1];
        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
            RecalculateLength(ControlPointCount - 1);

            rightLanePoints1[ControlPointCount - 1] = rightLanePoints1[0];
            EnforceModeRight(0, 0);
            RecalculateLengthRight(0, ControlPointCount - 1);

            rightLanePoints2[ControlPointCount - 1] = rightLanePoints2[0];
            EnforceModeRight(1, 0);
            RecalculateLengthRight(1, ControlPointCount - 1);

            rightLanePoints3[ControlPointCount - 1] = rightLanePoints3[0];
            EnforceModeRight(2, 0);
            RecalculateLengthRight(2, ControlPointCount - 1);

            leftLanePoints1[0] = leftLanePoints1[leftLanePoints1.Length - 1];
            EnforceModeLeft(0, leftLanePoints1.Length - 1);
            RecalculateLengthLeft(0, 0);

            leftLanePoints2[0] = leftLanePoints2[leftLanePoints2.Length - 1];
            EnforceModeLeft(1, leftLanePoints2.Length - 1);
            RecalculateLengthLeft(1, 0);

            leftLanePoints3[0] = leftLanePoints3[leftLanePoints3.Length - 1];
            EnforceModeLeft(2, leftLanePoints3.Length - 1);
            RecalculateLengthLeft(2, 0);
        }
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public BezierControlPointMode GetControlPointModeRight(int lane, int index)
    {
        BezierControlPointMode b = BezierControlPointMode.Aligned;
        switch (lane)
        {
            case 0:
                b = rightModes1[(index + 1) / 3];
                break;
            case 1:
                b = rightModes2[(index + 1) / 3];
                break;
            case 2:
                b = rightModes3[(index + 1) / 3];
                break;
        }
        return b;
    }

    public BezierControlPointMode GetControlPointModeLeft(int lane, int index)
    {
        BezierControlPointMode b = BezierControlPointMode.Aligned;
        switch (lane)
        {
            case 0:
                b = leftModes1[(index + 1) / 3];
                break;
            case 1:
                b = leftModes2[(index + 1) / 3];
                break;
            case 2:
                b = leftModes3[(index + 1) / 3];
                break;
        }
        return b;
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

    public void SetControlPointModeRight(int lane, int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        switch (lane)
        {
            case 0:
                rightModes1[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        rightModes1[rightModes1.Length - 1] = mode;
                    }
                    else if (modeIndex == rightModes1.Length - 1)
                    {
                        rightModes1[0] = mode;
                    }
                }
                break;
            case 1:
                rightModes2[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        rightModes2[rightModes2.Length - 1] = mode;
                    }
                    else if (modeIndex == rightModes2.Length - 1)
                    {
                        rightModes2[0] = mode;
                    }
                }
                break;
            case 2:
                rightModes3[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        rightModes3[rightModes3.Length - 1] = mode;
                    }
                    else if (modeIndex == rightModes3.Length - 1)
                    {
                        rightModes3[0] = mode;
                    }
                }
                break;
        }
        EnforceModeRight(lane, index);
    }

    public void SetControlPointModeLeft(int lane, int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        switch (lane)
        {
            case 0:
                leftModes1[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        leftModes1[leftModes1.Length - 1] = mode;
                    }
                    else if (modeIndex == leftModes1.Length - 1)
                    {
                        leftModes1[0] = mode;
                    }
                }
                break;
            case 1:
                leftModes2[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        leftModes2[leftModes2.Length - 1] = mode;
                    }
                    else if (modeIndex == leftModes2.Length - 1)
                    {
                        leftModes2[0] = mode;
                    }
                }
                break;
            case 2:
                leftModes3[modeIndex] = mode;
                if (loop)
                {
                    if (modeIndex == 0)
                    {
                        leftModes1[leftModes3.Length - 1] = mode;
                    }
                    else if (modeIndex == leftModes3.Length - 1)
                    {
                        leftModes3[0] = mode;
                    }
                }
                break;
        }
        EnforceModeLeft(lane, index);
    }

    public void Reset()
    {
        loop = false;
        initialized = false;
        points = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };
        modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Aligned,
            BezierControlPointMode.Aligned
        };

        splineLength = Vector3.Distance(points[0], points[3]);
        segmentLengths = new float[] { splineLength }; //jos ei alusteta suoralla pitää muuttaa

        LeftLaneCount = 0;
        RightLaneCount = 0;

        wayPointsLeft1 = new List<GameObject>();
        wayPointsLeft2 = new List<GameObject>();
        wayPointsLeft3 = new List<GameObject>();
        wayPointsRight1 = new List<GameObject>();
        wayPointsRight2 = new List<GameObject>();
        wayPointsRight3 = new List<GameObject>();

        leftLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftSegmentLengths1 = new float[] { 0 };
        leftSegmentLengths2 = new float[] { 0 };
        leftSegmentLengths3 = new float[] { 0 };
        rightSegmentLengths1 = new float[] { 0 };
        rightSegmentLengths2 = new float[] { 0 };
        rightSegmentLengths3 = new float[] { 0 };

        leftSplineLengths = new float[] {0f ,0f ,0f };
        rightSplineLengths = new float[] {0f ,0f ,0f };

        leftSpacings1 = new float[] { 0f, 0f };
        leftSpacings2 = new float[] { 0f, 0f };
        leftSpacings3 = new float[] { 0f, 0f };
        rightSpacings1 = new float[] { 0f, 0f };
        rightSpacings2 = new float[] { 0f, 0f };
        rightSpacings3 = new float[] { 0f, 0f };


        leftModes1 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        leftModes2 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        leftModes3 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes1 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes2 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes3 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };

    }
}

