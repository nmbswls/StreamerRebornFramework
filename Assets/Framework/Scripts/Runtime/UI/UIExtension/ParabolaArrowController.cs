using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaArrowController : MonoBehaviour
{
    public Vector3 FromPos;
    public Vector3 EndPos;
    public float launchAngle = 45.0f;
    public float launchSpeed = 10.0f;

    [Header("抛物线节点数")]
    public int Resolution = 50;

    public float HeightValue = 0.05f;
    public Vector3 HightDirection = new Vector3(10, -2, 10);

    public LineRenderer lineRenderer;
    private float gravity;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        gravity = 8;
    }

    private void OnValidate()
    {
        SetParabolaPoints(FromPos, EndPos, HeightValue);
    }


    public void SetParabolaPoints(Vector3 startPoint, Vector3 endPoint, float height)
    {
        lineRenderer.positionCount = Resolution + 1;
        Vector3[] points = new Vector3[Resolution + 1];

        Vector3 apexPoint = CalculateApexPoint(startPoint, endPoint, height, HightDirection);

        for (int i = 0; i <= Resolution; i++)
        {
            float t = (float)i / Resolution;
            points[i] = CalculateParabolaPoint(startPoint, apexPoint, endPoint, t);
        }

        lineRenderer.SetPositions(points);
    }

    Vector3 CalculateApexPoint(Vector3 startPoint, Vector3 endPoint, float height, Vector3 gravityDirection)
    {
        Vector3 middlePoint = (startPoint + endPoint) / 2;
        Vector3 heightDirection = gravityDirection;
        return middlePoint + heightDirection * height;
    }

    Vector3 CalculateParabolaPoint(Vector3 startPoint, Vector3 apexPoint, Vector3 endPoint, float t)
    {
        return (1 - t) * (1 - t) * startPoint + 2 * t * (1 - t) * apexPoint + t * t * endPoint;
    }
}