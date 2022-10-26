using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PolarArrowAttribute : PropertyAttribute
{
    public readonly float MinAngle;
    public readonly float MaxAngle;
    public readonly float MinDistance;
    public readonly float MaxDistance;

    public PolarArrowAttribute(float maxDistance, float minAngle = 0f, float maxAngle = 360f, float minDistance = 0f)
    {
        MinAngle = minAngle;
        MaxAngle = maxAngle;
        MinDistance = minDistance;
        MaxDistance = maxDistance;
    }
}
