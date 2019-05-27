using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineChanger : MonoBehaviour
{
    public BezierSpline leftLane;
    public BezierSpline rightLane;

    public float distanceTraveled;
    public float leftSplineLength;
    public float rightSplineLength;
    private float turnAngle = 20f;

    public float speed;

    public bool changeLane = false;
    // Fraqment of distance between two points (0-1)
    public int lateralSteps = 100;
    public int laterallyMoved;

    public enum Mode
    {
        SwitchRight,
        SwitchLeft,
        RightLane,
        LeftLane
    }

    public Mode currentMode;

    private void Awake()
    {
        leftSplineLength = leftLane.SplineLength;
        rightSplineLength = rightLane.SplineLength;
        distanceTraveled = 0f;
        currentMode = Mode.RightLane;    
    }

    private void Update()
    {
        if (changeLane)
        {
            changeLane = false;
            if (currentMode == Mode.RightLane)
            {
                currentMode = Mode.SwitchLeft;
                laterallyMoved = 0;
            }
            else if (currentMode == Mode.LeftLane)
            {
                currentMode = Mode.SwitchRight;
                laterallyMoved = 0;
            }
        }

        distanceTraveled += Time.deltaTime * speed;
        //tämä loopatessa, edetessä nodesta nodeen pitäisi päivittää current node jne.
        Vector3 position = Vector3.zero;
        Vector3 direction = Vector3.zero;
        Vector3 leftPos;
        Vector3 rightPos;
        float fraq;
        float angle;
        switch(currentMode)
        {
            case Mode.LeftLane:
                if (distanceTraveled > leftSplineLength)
                {
                    distanceTraveled -= leftSplineLength;
                }
                position = leftLane.GetPointWhenTraveled(distanceTraveled);
                direction = leftLane.GetDirectionWhenTraveled(distanceTraveled);

                transform.localPosition = position;
                transform.LookAt(position + direction);

                break;

            case Mode.RightLane:
                if (distanceTraveled > rightSplineLength)
                {
                    distanceTraveled -= rightSplineLength;
                }
                position = rightLane.GetPointWhenTraveled(distanceTraveled);
                direction = rightLane.GetDirectionWhenTraveled(distanceTraveled);

                transform.localPosition = position;
                transform.LookAt(position + direction);

                break;

            case Mode.SwitchLeft:
                if (distanceTraveled > leftSplineLength)
                {
                    distanceTraveled -= leftSplineLength;
                }
                rightPos = rightLane.GetPointWhenTraveled(distanceTraveled);
                fraq = distanceTraveled / rightSplineLength;
                float leftTraveled = fraq* leftSplineLength;
                leftPos = leftLane.GetPointWhenTraveled(leftTraveled);
                direction = leftLane.GetDirectionWhenTraveled(leftTraveled);
                laterallyMoved++;
                position = leftPos * (float)laterallyMoved / lateralSteps +
                    rightPos * ((float)lateralSteps - (float)laterallyMoved) / lateralSteps;
                if (laterallyMoved==lateralSteps)
                {
                    currentMode = Mode.LeftLane;
                    laterallyMoved = 0;
                }

                angle = (1f - Mathf.Abs(lateralSteps - laterallyMoved * 2) / (float)lateralSteps) * turnAngle;

                transform.localPosition = position;
                transform.LookAt(position + direction);
                Vector3 t = transform.eulerAngles;
                t.y -= angle;
                transform.eulerAngles = t;

                break;

            case Mode.SwitchRight:
                if (distanceTraveled > rightSplineLength)
                {
                    distanceTraveled -= rightSplineLength;
                }
                leftPos = leftLane.GetPointWhenTraveled(distanceTraveled);

                fraq = distanceTraveled / leftSplineLength;
                float rightTraveled = rightSplineLength * fraq;
                rightPos = rightLane.GetPointWhenTraveled(rightTraveled);
                direction = rightLane.GetDirectionWhenTraveled(rightTraveled);
                laterallyMoved++;
                position = rightPos * (float)laterallyMoved / lateralSteps +
                    leftPos * ((float)lateralSteps - (float)laterallyMoved) / lateralSteps;
                if (laterallyMoved == lateralSteps)
                {
                    currentMode = Mode.RightLane;
                    laterallyMoved = 0;
                }

                angle = (1f - Mathf.Abs(lateralSteps - laterallyMoved * 2) / (float)lateralSteps) * turnAngle;

                transform.localPosition = position;
                transform.LookAt(position + direction);

                Vector3 ty = transform.eulerAngles;
                ty.y += angle;
                transform.eulerAngles = ty;

                break;
        }


    }
}
