using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;

public class HitBox : MonoBehaviour
{
    private BaseStateMachine _baseStateMachine;
    private Fighter _fighter;

    private void Awake()
    {
        _baseStateMachine = GetComponentInParent<BaseStateMachine>();
        _fighter = GetComponentInParent<Fighter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        //if (other.gameObject.layer != Services.FightersManager.hurtboxLayer)
        //{
        //    return;
        //}
        if (!other.GetComponent<Fighter>())
        {
            return;
        }
        AttackInfo attackInfo = _baseStateMachine.CurrentState.GetAttackInfo();

        Fighter hitFighter = other.GetComponent<Fighter>();
        hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);
        Vector3 forceAngle = attackInfo.knockBackAngle;
        forceAngle.x = _fighter.FacingDirection == Fighter.Direction.Right ? attackInfo.knockBackAngle.x : -attackInfo.knockBackAngle.x;
        float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        StartCoroutine(hitFighter.MovementController.ApplyForce(forceAngle, forceMagnitude, attackInfo.knockbackDuration));
        StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
    }
}
