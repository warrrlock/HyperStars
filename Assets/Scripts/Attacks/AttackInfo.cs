using System;
using UnityEngine;

[Serializable]
public class AttackInfo
{
    public float knockbackDuration;
    public float knockbackDistance;
    public Vector3 knockbackDirection;
    public float hitStunDuration;
    public float damage;
    public bool causesWallBounce;
    public float hitStopDuration;
    public float wallBounceDuration;
    [Tooltip("Leave this at 0 if wall bounce should not create a new force.")]
    public float wallBounceDistance;
    [Tooltip("Assume the hit fighter collides with the wall while going right.")]
    public Vector3 wallBounceDirection;
    public float wallBounceHitStopDuration;
    public float hangTime;
    [Tooltip("")]
    public float favorReward;

    [PolarArrow(100f)]
    [Tooltip("X is the force angle in degrees. Y is the force magnitude.")]
    public Vector2 knockbackForce;

    [Header("Special Meter")]
    [Tooltip("Referenced is a single bar. For example, 0.2 means this state increments special bar by 0.2 bars.")]
    public float incrementBarAmount;
}
