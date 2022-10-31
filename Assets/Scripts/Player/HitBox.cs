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
        // Debug.Log(other.name);
        //if (other.gameObject.layer != Services.FightersManager.hurtboxLayer)
        //{
        //    return;
        //}
        if (other.gameObject.layer == 13)
        {
            //has been parried
            AttackInfo parryInfo = _fighter.OpposingFighter.BaseStateMachine.AttackInfo;
            StartCoroutine(_fighter.InputManager.Disable(parryInfo.hitStunDuration, _fighter.InputManager.Actions["Move"]));
            StartCoroutine(_baseStateMachine.SetHurtState(KeyHurtStatePair.HurtStateName.HitStun));
            return;
        }
        
        if (other.gameObject.layer != 7)
        {
            return;
        }

        AttackInfo attackInfo = _baseStateMachine.AttackInfo;
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Fighter hitFighter = _fighter.OpposingFighter;
        
        if (!hitFighter.canBeHurt || attackInfo == null)
        {
            return;
        }

        _fighter.Events.onAttackHit?.Invoke(_fighter, hitFighter, hitPoint);
        
        //hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);

        hitFighter.canBeHurt = false;

        StartCoroutine(hitFighter.BaseStateMachine.SetHurtState(
            attackInfo.knockbackForce.x is > 0f and < 180f
            ? KeyHurtStatePair.HurtStateName.KnockBack
            : KeyHurtStatePair.HurtStateName.HitStun));
        
        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        Vector3 forceDirection = attackInfo.knockbackDirection;
        forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfo.knockbackDuration));
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, attackInfo.knockbackForce.y, attackInfo.knockbackDuration));
        StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
        hitFighter.MovementController.ResetVelocityY();
        if (attackInfo.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(attackInfo.wallBounceDistance, attackInfo.wallBounceDuration, attackInfo.wallBounceDirection, attackInfo.wallBounceHitStopDuration));
        }
        StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfo.hangTime));
        Services.FavorManager?.IncreaseFavor(_fighter.PlayerId, attackInfo.favorReward);

        StartCoroutine(Juice.FreezeTime(attackInfo.hitStopDuration));
    }
}
