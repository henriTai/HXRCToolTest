using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHelper : MonoBehaviour
{
    public List<Vector3> positions;
    public List<Vector3> directions;
    public List<float> lengths;
    public int lines = 0;

    public void ResetHelper()
    {
        positions.Clear();
        directions.Clear();
        lengths.Clear();
        lines = 0;
    }

    public void AddLine(Vector3 position, Directions generalDirection, float length)
    {
        positions.Add(position);
        Vector3 dir = GeneralDirectionVector(generalDirection);
        directions.Add(dir);
        lengths.Add(length);
        lines++;
    }

    public Vector3 GetLine (int index, out Vector3 start, out Vector3 end)
    {
        Vector3 pos = positions[index];
        Vector3 half = directions[index] * 0.5f * lengths[index];
        start = pos - half;
        end = pos + half;
        return pos;
    }

    private static Vector3 GeneralDirectionVector(Directions direction)
    {
        Vector3 dir = Vector3.zero;
        switch (direction)
        {
            case Directions.North:
                dir = new Vector3(1f, 0f, 0f);
                break;
            case Directions.West:
                dir = new Vector3(0f, 0f, -1f);
                break;
            case Directions.South:
                dir = new Vector3(-1f, 0f, 0f);
                break;
            case Directions.East:
                dir = new Vector3(0f, 0f, 1f);
                break;
            case Directions.NorthWest:
                dir = new Vector3(1f, 0f, -1f).normalized;
                break;
            case Directions.SouthWest:
                dir = new Vector3(-1f, 0f, -1f).normalized;
                break;
            case Directions.SouthEast:
                dir = new Vector3(-1f, 0f, 1f).normalized;
                break;
            case Directions.NorthEast:
                dir = new Vector3(1f, 0f, 1f).normalized;
                break;
        }
        return dir;
    }

    public void Reset()
    {
        positions = new List<Vector3>();
        directions = new List<Vector3>();
        lengths = new List<float>();
        lines = 0;
        positions.Add(Vector3.zero);
        directions.Add(GeneralDirectionVector(Directions.SouthWest));
        lengths.Add(20f);
        lines = 1;
    }
}
