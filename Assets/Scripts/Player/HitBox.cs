using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using WesleyDavies;
using WesleyDavies.UnityFunctions;

public class HitBox : MonoBehaviour
{
    private BaseStateMachine _baseStateMachine;
    private Fighter _fighter;
    private Collider _collider;
    private Collider[] _colliders;

    private void Awake()
    {
        _fighter = GetComponentInParent<Fighter>();
        _collider = GetComponent<Collider>();
        _colliders = _fighter.GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        _baseStateMachine = _fighter.BaseStateMachine;
        foreach(Collider collider in _colliders)
        {
            Physics.IgnoreCollision(_collider, collider);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fighter.OpposingFighter.Parried) return;
        AttackInfo attackInfo = _baseStateMachine.AttackInfo;
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Fighter hitFighter = _fighter.OpposingFighter;
        
        if (other.gameObject.layer == 13)
        {
            _fighter.OpposingFighter.Parried = true;
            //has been parried
            AttackInfo parryInfo = hitFighter.BaseStateMachine.AttackInfo;
            
            StartCoroutine(_baseStateMachine.SetHurtState(KeyHurtStatePair.HurtStateName.HitStun, parryInfo.hitStunDuration));

            _fighter.Events.onBlockHit?.Invoke(new Dictionary<string, object>
                {
                    {"attacker", _fighter},
                    {"attacked", hitFighter}, 
                    {"hit point", hitPoint},
                    {"attacker input", hitFighter.BaseStateMachine.LastExecutedInput},
                    {"attackInfo", parryInfo},
                    {"attack type", attackInfo.attackType},
                }
            );
            return;
        }
        
        if (other.gameObject.layer != 7)
        {
            return;
        }
        
        if (hitFighter.invulnerabilityCount > 0f || attackInfo == null)
        {
            return;
        }
        
        // Debug.Log(_baseStateMachine.CurrentState.name);
        // Debug.Log(hitFighter.invulnerabilityCount);

        _fighter.Events.onAttackHit?.Invoke(new Dictionary<string, object>
            {
                {"attacker", _fighter},
                {"attacked", hitFighter}, 
                {"hit point", hitPoint},
                {"attacker input", _fighter.BaseStateMachine.LastExecutedInput},
                {"attack info", attackInfo},
                {"attack type", attackInfo.attackType},
            }
        );
        //hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);

        hitFighter.invulnerabilityCount++;
        // Debug.Log(hitFighter.invulnerabilityCount);
        
        StartCoroutine(hitFighter.BaseStateMachine.SetHurtState(
            !hitFighter.MovementController.CollisionData.y.isNegativeHit 
            ? KeyHurtStatePair.HurtStateName.AirKnockBack
            : (attackInfo.knockbackForce.x is > 0f and < 180f 
                ? KeyHurtStatePair.HurtStateName.KnockBack 
                : KeyHurtStatePair.HurtStateName.HitStun)
            , attackInfo.hitStunDuration));

        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        Vector3 forceDirection = attackInfo.knockbackDirection;
        
        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfo.knockbackDuration, true);
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, attackInfo.knockbackForce.y, attackInfo.knockbackDuration));
        
            //float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        //Vector3 forceDirection = attackInfo.knockbackDirection;
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfo.knockbackDuration));
        //StartCoroutine(hitFighter.MovementController.ApplyForcePolar(forceDirection, attackInfo.knockbackForce.y));
        //StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
        hitFighter.MovementController.ResetVelocityY();
        if (attackInfo.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(attackInfo.wallBounceDistance, attackInfo.wallBounceDuration, attackInfo.wallBounceDirection, attackInfo.wallBounceHitStopDuration));
        }
        if (attackInfo.causesGroundBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableGroundBounce(attackInfo.groundBounceDistance, attackInfo.groundBounceDuration, attackInfo.groundBounceDirection, attackInfo.groundBounceHitStopDuration));
        }
        if (!hitFighter.MovementController.IsGrounded)
        {
            StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfo.hangTime));
        }
        Services.FavorManager?.IncreaseFavor(_fighter.PlayerId, attackInfo.favorReward);

        StartCoroutine(Juice.FreezeTime(attackInfo.hitStopDuration));
    }
}
