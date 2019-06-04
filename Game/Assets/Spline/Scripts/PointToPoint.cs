using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToPoint : MonoBehaviour
{
    public Nodes current;
    public Nodes next;
    public float speed = 5f;

    Vector3 startPos;
    Vector3 endPos;
    Vector3 lookDirection;

    private float startTime;
    private float rotationSpeed;

    private float distance;


    private void Awake()
    {
        next = current.OutNodes[0];
        startTime = Time.time;
        startPos = current.transform.position;
        endPos = next.transform.position;
        distance = Vector3.Distance(startPos, endPos);
        rotationSpeed = speed / distance;
        transform.position = startPos;
        transform.LookAt(next.transform);
        lookDirection = (endPos - startPos).normalized;
    }

    private void Update()
    {

        float distCovered = (Time.time - startTime) * speed;
        float fragment = distCovered / distance;

        transform.position = Vector3.Lerp(startPos, endPos, fragment);
        Vector3 relativePos = endPos - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

        if (Vector3.Distance(transform.position, next.transform.position) <= 0f)
        {
            current = next;
            next = next.OutNodes[0];
            endPos = next.transform.position;
            startPos = transform.position;
            distance = Vector3.Distance(startPos, endPos);
            lookDirection = endPos - startPos;
            rotationSpeed = speed / distance;
            startTime = Time.time;
        }
    }



}
