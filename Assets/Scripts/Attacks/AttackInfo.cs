using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class AttackInfo
{
    public int idSO;
    public int idManager;

    public enum AttackType {None, Light, Medium, Heavy, Special}
    public enum SpecialType {None, Side, Neutral, Air, Crouch}
    public AttackType attackType = AttackType.None;
    public SpecialType specialType = SpecialType.None;

    public AK.Wwise.Event hitSfxEvent;

    public float knockbackDuration;
    public float knockbackDistance;
    public Vector3 knockbackDirection;
    public float reverseKnockbackDuration;
    public float reverseKnockbackDistance;
    [Tooltip("The direction to knock the attacking player back. Assume the fighter is facing right.")]
    public Vector3 reverseKnockbackDirection;
    public float hitStunDuration;
    public bool causesWallBounce;
    public float hitStopDuration;

    [Header("Wall Bounce")]
    public float wallBounceDuration;
    [Tooltip("Leave this at 0 if wall bounce should not create a new force.")]
    public float wallBounceDistance;
    [Tooltip("Assume the hit fighter collides with the wall while going right.")]
    public Vector3 wallBounceDirection;
    public float wallBounceHitStopDuration;

    [Header("Ground Bounce")]
    public bool causesGroundBounce;
    public float groundBounceDuration;
    [Tooltip("Leave this at 0 if ground bounce should not create a new force.")]
    public float groundBounceDistance;
    [Tooltip("Assume the hit fighter collides with the ground while going right.")]
    public Vector3 groundBounceDirection;
    public float groundBounceHitStopDuration;

    [Header("Gravity Modifiers")]
    public float hangTime; //TODO: should this just be replaced with gravity augment?
    [Tooltip("The percentage by which to augment gravity when the hit opponent starts falling. Ex: 0.25 will cause gravity to be quartered.")]
    [Range(0f, 1f)] public float gravityAugmentFactor = 1f;
    [Tooltip("The duration to augment hit opponent gravity for.")]
    public float gravityAugmentDuration;

    [Header("Favor Meter")]
    [Tooltip("How much favor is gained upon hitting the opponent.")]
    public float favorReward;

    //[NonSerialized] public float outputReward = 1;
    //public float outputHitStunDuration;

    [PolarArrow(100f)]
    [Tooltip("X is the force angle in degrees. Y is the force magnitude.")]
    public Vector2 knockbackForce;

    [Header("Special Meter")]
    [Tooltip("Referenced is a single bar. For example, 0.2 means this state increments special bar by 0.2 bars.")]
    public float incrementBarAmount;

    [Header("Multi-Hit")]
    [Tooltip("Set >1 to create multi-hit attack.")]
    public int multiHitCount;
    [Tooltip("The duration between multi-hits.")]
    public float multiHitInterval;

    //public void Initialize(AttackInfoManager manager, int id)
    //{
    //    idSO = id;
    //    //outputReward = favorReward;
    //    //outputHitStunDuration = hitStunDuration;
    //    //manager.values[id] = new(favorReward, hitStunDuration);
    //    manager.values[idSO].outputReward = favorReward;
    //    manager.values[idSO].outputHitStunDuration = hitStunDuration;
    //}

    ////[SerializeField]
    //public IEnumerator Decay()
    //{
    //    float preDecayReward = OutputReward;
    //    OutputReward = Mathf.Clamp(OutputReward - (favorReward * Services.FavorManager.DecayValue), 0f, favorReward);
    //    OutputHitStunDuration = Mathf.Clamp(OutputHitStunDuration - (hitStunDuration * Services.FavorManager.DecayValue), 0f, hitStunDuration);
    //    float timer = 0f;
    //    float timerDelta = Time.fixedDeltaTime / Services.FavorManager.DecayResetDuration;
    //    while (OutputReward < preDecayReward && timer < Services.FavorManager.DecayResetDuration)
    //    {
    //        OutputReward += favorReward * Services.FavorManager.DecayValue * timerDelta;
    //        OutputHitStunDuration += hitStunDuration * Services.FavorManager.DecayValue * timerDelta;
    //        timer += Time.fixedDeltaTime;
    //        yield return new WaitForFixedUpdate();
    //    }
    //    yield break;
    //}
}
