using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CircularRangeAttribute : PropertyAttribute
{
    public readonly float Min;
    public readonly float Max;

    public CircularRangeAttribute(float max = 360f, float min = 0f)
    {
        Min = min;
        Max = max;
    }
}
