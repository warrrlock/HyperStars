using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using WesleyDavies.UnityFunctions;

public class HitBox : MonoBehaviour
{
    private BaseStateMachine _baseStateMachine;
    private Fighter _fighter;
    private Collider _collider;
    private Collider[] _colliders;

    private void Awake()
    {
        _baseStateMachine = GetComponentInParent<BaseStateMachine>();
        _fighter = GetComponentInParent<Fighter>();
        _collider = GetComponent<Collider>();
        _colliders = _fighter.GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        foreach(Collider collider in _colliders)
        {
            Physics.IgnoreCollision(_collider, collider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        //if (other.gameObject.layer != Services.FightersManager.hurtboxLayer)
        //{
        //    return;
        //}
        if (other.gameObject.layer != 7)
        {
            return;
        }

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        AttackInfo attackInfo = _baseStateMachine.CurrentState.GetAttackInfo();

        Fighter hitFighter = other.GetComponentInParent<Fighter>();
        _fighter.onAttackHit?.Invoke(_fighter, hitFighter, hitPoint);
        hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);
        
        Vector3 forceAngle = attackInfo.knockBackAngle;
        forceAngle.x = _fighter.FacingDirection == Fighter.Direction.Right ? attackInfo.knockBackAngle.x : -attackInfo.knockBackAngle.x;
        float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        StartCoroutine(hitFighter.MovementController.ApplyForce(forceAngle, forceMagnitude, attackInfo.knockbackDuration));
        StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
        hitFighter.MovementController.ResetVelocityY();
        if (attackInfo.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(attackInfo.wallBounceDistance, attackInfo.wallBounceDuration, attackInfo.wallBounceDirection, attackInfo.wallBounceHitStopDuration));
        }
        StartCoroutine(Juice.FreezeTime(attackInfo.hitStopDuration));
    }
}
