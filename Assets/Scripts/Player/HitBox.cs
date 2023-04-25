using System;
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

    //private AttackInfo _lastAttackInfo;

    private void OnTriggerEnter(Collider other)
    {
        //if (_isMultiHitting)
        //{
        //    StopCoroutine(_multiHit);
        //    _canMultiHit = false; //TODO: make this its own StopMultiHit() function
        //    Debug.Log("stopped multihit");
        //}
        
        if (_fighter.OpposingFighter.AlreadyHitByAttack) return;
        if (_fighter.OpposingFighter.Parried) return;
        
        AttackInfo attackInfoSO = _baseStateMachine.AttackInfo;
        if (other.gameObject.layer == 7) _fighter.OpposingFighter.AlreadyHitByAttack = true;


        AttackInfo attackInfo;
        //if (_fighter.PlayerId == 0)
        //{
        //    attackInfo = Services.AttackInfoManager.attackInfos0.Find(x => x.attackType == attackInfoSO.attackType && x.hitStunDuration == attackInfoSO.hitStunDuration && x.favorReward == attackInfoSO.favorReward && x.knockbackDirection == attackInfoSO.knockbackDirection);
        //}
        //else
        //{
        //    attackInfo = Services.AttackInfoManager.attackInfos1.Find(x => x.attackType == attackInfoSO.attackType && x.hitStunDuration == attackInfoSO.hitStunDuration && x.favorReward == attackInfoSO.favorReward && x.knockbackDirection == attackInfoSO.knockbackDirection);
        //}

        if (_fighter.PlayerId == 0)
        {
            attackInfo = Services.AttackInfoManager.attackInfos0.Find(x => x.idSO == attackInfoSO.idSO);
        }
        else
        {
            attackInfo = Services.AttackInfoManager.attackInfos1.Find(x => x.idSO == attackInfoSO.idSO);
        }
        //Debug.Log(attackInfo.idManager);

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Fighter hitFighter = _fighter.OpposingFighter;
        
        if (other.gameObject.layer == 13)
        {
            _fighter.OpposingFighter.Parried = true;
            //has been parried
            AttackInfo parryInfo = hitFighter.BaseStateMachine.AttackInfo;
            
            StartCoroutine(_baseStateMachine.SetHurtState(KeyHurtStatePair.HurtStateName.HitStun, parryInfo.hitStunDuration, parryInfo.hardKnockdown));

            _fighter.Events.onBlockHit?.Invoke(new Dictionary<string, object>
                {
                    {"attacker", _fighter},
                    {"attacked", hitFighter}, 
                    {"hit point", hitPoint},
                    {"attacker input", hitFighter.BaseStateMachine.LastExecutedInput},
                    {"attack info", parryInfo},
                    {"attack type", attackInfoSO.attackType},
                }
            );
            return;
        }

        if (other.gameObject.layer != 7)
        {
            // Debug.Log($"gameobject layer is wrong, it is {other.gameObject.layer}");
            return;
        }
        
        if (attackInfoSO == null)
        {
            Debug.Log("attack info is null, resetting already hit");
            _fighter.OpposingFighter.AlreadyHitByAttack = false;
            return;
        }

        if (_isMultiHitting)
        {
            StopCoroutine(_multiHit);
            _canMultiHit = false; //TODO: make this its own StopMultiHit() function
            Debug.Log("stopped multihit");
        }

        // Debug.Log(_baseStateMachine.CurrentState.name);
        // Debug.Log(hitFighter.invulnerabilityCount);

        _fighter.Events.onAttackHit?.Invoke(new Dictionary<string, object>
            {
                {"attacker", _fighter},
                {"attacked", hitFighter}, 
                {"hit point", hitPoint},
                {"attacker input", _fighter.BaseStateMachine.LastExecutedInput},
                {"attack info", attackInfoSO},
                {"attack type", attackInfoSO.attackType},
            }
        );
        //hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);

        //if (attackInfoSO.multiHitCount <= 1)
        //{
        //    hitFighter.invulnerabilityCount++;
        //}
        //TODO: did this fuck anything up
        // hitFighter.invulnerabilityCount++;
        // Debug.Log(hitFighter.invulnerabilityCount);

        AttackInfoManager.Values attackInfoValues = Services.AttackInfoManager.values[attackInfo.idManager];
        
        StartCoroutine(hitFighter.BaseStateMachine.SetHurtState(
            !hitFighter.MovementController.CollisionData.y.isNegativeHit 
            ? KeyHurtStatePair.HurtStateName.AirKnockBack
            : (attackInfoSO.knockbackForce.x is > 0f and < 180f 
                ? KeyHurtStatePair.HurtStateName.KnockBack 
                : KeyHurtStatePair.HurtStateName.HitStun)
            , attackInfoValues.outputHitStunDuration, attackInfoSO.hardKnockdown));

        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        float forceMagnitude = (attackInfoValues.outputKnockbackDistance * 2f) / (attackInfoSO.knockbackDuration + Time.fixedDeltaTime);
        Vector3 forceDirection = attackInfoSO.knockbackDirection;

        float reverseForceMagnitude = (attackInfoSO.reverseKnockbackDistance * 2f) / (attackInfoSO.reverseKnockbackDuration + Time.fixedDeltaTime);
        Vector3 reverseForceDirection = attackInfoSO.reverseKnockbackDirection;

        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        reverseForceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? reverseForceDirection.x : -reverseForceDirection.x;
        hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfoSO.knockbackDuration, true);
        if (attackInfoSO.reverseKnockbackDistance > 0f)
        {
            _fighter.MovementController.ApplyForce(reverseForceDirection, reverseForceMagnitude, attackInfoSO.reverseKnockbackDuration, true);
        }
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, attackInfo.knockbackForce.y, attackInfo.knockbackDuration));

        //float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        //Vector3 forceDirection = attackInfo.knockbackDirection;
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfo.knockbackDuration));
        //StartCoroutine(hitFighter.MovementController.ApplyForcePolar(forceDirection, attackInfo.knockbackForce.y));
        //StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
        hitFighter.MovementController.ResetVelocityY();
        //_fighter.MovementController.ResetVelocityY();
        if (attackInfoSO.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(attackInfoSO.wallBounceDistance, attackInfoSO.wallBounceDuration, attackInfoSO.wallBounceDirection, attackInfoSO.wallBounceHitStopDuration));
        }
        if (attackInfoSO.causesGroundBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableGroundBounce(attackInfoSO.groundBounceDistance, attackInfoSO.groundBounceDuration, attackInfoSO.groundBounceDirection, attackInfoSO.groundBounceHitStopDuration));
        }
        if (attackInfoSO.hangTime > 0f && (forceDirection.y > 0f || !hitFighter.MovementController.IsGrounded))
        {
            StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfoSO.hangTime));
        }

        if (attackInfoSO.gravityAugmentDuration > 0f)
        {
            hitFighter.MovementController.AugmentGravity(attackInfoSO.gravityAugmentFactor, attackInfoSO.gravityAugmentDuration);
        }

        //StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfo.hangTime));
        //Services.FavorManager?.IncreaseFavor(_fighter.PlayerId, attackInfo.favorReward);
        Services.FavorManager.IncreaseFavor(_fighter.PlayerId, attackInfoValues.outputReward);
        //num++;
        //Services.AttackInfoManager.debugText.text = num.ToString();

        //TODO: multi-hit attacks should only decay after the entire attack has finished
        if (attackInfoSO.multiHitCount <= 1)
        {
            StartCoroutine(Services.AttackInfoManager.DecayValues(attackInfo));
        }
        else if (!_isMultiHitting)
        {
            _multiHit = MultiHit(attackInfoSO.multiHitCount, attackInfoSO.multiHitInterval);
            //StartCoroutine(MultiHit(attackInfoSO.multiHitCount, attackInfoSO.multiHitInterval));
            StartCoroutine(_multiHit);
        }

        StartCoroutine(Juice.FreezeTime(attackInfoSO.hitStopDuration));

        //Debug.Log("hit");
    }

    private IEnumerator _multiHit;

    private IEnumerator MultiHit(int maxHitCount, float hitInterval)
    {
        _isMultiHitting = true;
        for (int i = 0; i < maxHitCount - 1; i++)
        {
            yield return new WaitForSeconds(hitInterval);
            _canMultiHit = true;
        }
        yield return new WaitForSeconds(hitInterval);
        _canMultiHit = false;
        _isMultiHitting = false;
        yield break;
    }

    private bool _canMultiHit = false;
    private bool _isMultiHitting = false;

    private void OnTriggerStay(Collider other)
    {
        if (!_canMultiHit) return;
        if (_fighter.OpposingFighter.Parried) return;
        AttackInfo attackInfoSO = _baseStateMachine.AttackInfo;

        AttackInfo attackInfo;
        //if (_fighter.PlayerId == 0)
        //{
        //    attackInfo = Services.AttackInfoManager.attackInfos0.Find(x => x.attackType == attackInfoSO.attackType && x.hitStunDuration == attackInfoSO.hitStunDuration && x.favorReward == attackInfoSO.favorReward && x.knockbackDirection == attackInfoSO.knockbackDirection);
        //}
        //else
        //{
        //    attackInfo = Services.AttackInfoManager.attackInfos1.Find(x => x.attackType == attackInfoSO.attackType && x.hitStunDuration == attackInfoSO.hitStunDuration && x.favorReward == attackInfoSO.favorReward && x.knockbackDirection == attackInfoSO.knockbackDirection);
        //}

        if (_fighter.PlayerId == 0)
        {
            attackInfo = Services.AttackInfoManager.attackInfos0.Find(x => x.idSO == attackInfoSO.idSO);
        }
        else
        {
            attackInfo = Services.AttackInfoManager.attackInfos1.Find(x => x.idSO == attackInfoSO.idSO);
        }
        //Debug.Log(attackInfo.idManager);

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Fighter hitFighter = _fighter.OpposingFighter;

        if (other.gameObject.layer == 13)
        {
            _fighter.OpposingFighter.Parried = true;
            //has been parried
            AttackInfo parryInfo = hitFighter.BaseStateMachine.AttackInfo;

            StartCoroutine(_baseStateMachine.SetHurtState(KeyHurtStatePair.HurtStateName.HitStun, parryInfo.hitStunDuration, parryInfo.hardKnockdown));

            _fighter.Events.onBlockHit?.Invoke(new Dictionary<string, object>
                {
                    {"attacker", _fighter},
                    {"attacked", hitFighter},
                    {"hit point", hitPoint},
                    {"attacker input", hitFighter.BaseStateMachine.LastExecutedInput},
                    {"attack info", parryInfo},
                    {"attack type", attackInfoSO.attackType},
                }
            );
            return;
        }

        if (other.gameObject.layer != 7)
        {
            return;
        }

        //TODO: this is stupid
        //if (hitFighter.invulnerabilityCount > 0f || attackInfoSO == null)
        //{
        //    Debug.Log("eureka");
        //    return;
        //}

        // Debug.Log(_baseStateMachine.CurrentState.name);
        // Debug.Log(hitFighter.invulnerabilityCount);

        _fighter.Events.onAttackHit?.Invoke(new Dictionary<string, object>
            {
                {"attacker", _fighter},
                {"attacked", hitFighter},
                {"hit point", hitPoint},
                {"attacker input", _fighter.BaseStateMachine.LastExecutedInput},
                {"attack info", attackInfoSO},
                {"attack type", attackInfoSO.attackType},
            }
        );
        //hitFighter.FighterHealth.ApplyDamage(attackInfo.damage);

        //if (attackInfoSO.multiHitCount <= 1)
        //{
        //    hitFighter.invulnerabilityCount++;
        //}
        //TODO: idk i commented this out, not sure if itll break anything
        // hitFighter.invulnerabilityCount++;
        // Debug.Log(hitFighter.invulnerabilityCount);

        AttackInfoManager.Values attackInfoValues = Services.AttackInfoManager.values[attackInfo.idManager];

        StartCoroutine(hitFighter.BaseStateMachine.SetHurtState(
            !hitFighter.MovementController.CollisionData.y.isNegativeHit
                ? KeyHurtStatePair.HurtStateName.AirKnockBack
                : (attackInfoSO.knockbackForce.x is > 0f and < 180f
                    ? KeyHurtStatePair.HurtStateName.KnockBack
                    : KeyHurtStatePair.HurtStateName.HitStun)
            , attackInfoValues.outputHitStunDuration, attackInfoSO.hardKnockdown));
        Debug.Log("set the hurt state");

            //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
            //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
            float forceMagnitude = (attackInfoValues.outputKnockbackDistance * 2f) / (attackInfoSO.knockbackDuration + Time.fixedDeltaTime);
            Vector3 forceDirection = attackInfoSO.knockbackDirection;

        float reverseForceMagnitude = (attackInfoSO.reverseKnockbackDistance * 2f) / (attackInfoSO.reverseKnockbackDuration + Time.fixedDeltaTime);
        Vector3 reverseForceDirection = attackInfoSO.reverseKnockbackDirection;

        //Vector3 forceDirection = new Vector3(attackInfo.knockbackForce.x.ToDirection(false).x, attackInfo.knockbackForce.x.ToDirection(false).y, 0f);
        forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        reverseForceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfoSO.knockbackDuration, true);
        if (attackInfoSO.reverseKnockbackDistance > 0f)
        {
            _fighter.MovementController.ApplyForce(reverseForceDirection, reverseForceMagnitude, attackInfoSO.reverseKnockbackDuration, true);
        }
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, attackInfo.knockbackForce.y, attackInfo.knockbackDuration));

        //float forceMagnitude = (attackInfo.knockbackDistance * 2f) / (attackInfo.knockbackDuration + Time.fixedDeltaTime);
        //Vector3 forceDirection = attackInfo.knockbackDirection;
        //forceDirection.x = _fighter.FacingDirection == Fighter.Direction.Right ? forceDirection.x : -forceDirection.x;
        //StartCoroutine(hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, attackInfo.knockbackDuration));
        //StartCoroutine(hitFighter.MovementController.ApplyForcePolar(forceDirection, attackInfo.knockbackForce.y));
        //StartCoroutine(hitFighter.InputManager.Disable(attackInfo.hitStunDuration, hitFighter.InputManager.Actions["Move"]));
        hitFighter.MovementController.ResetVelocityY();
        //_fighter.MovementController.ResetVelocityY();
        if (attackInfoSO.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(attackInfoSO.wallBounceDistance, attackInfoSO.wallBounceDuration, attackInfoSO.wallBounceDirection, attackInfoSO.wallBounceHitStopDuration));
        }
        if (attackInfoSO.causesGroundBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableGroundBounce(attackInfoSO.groundBounceDistance, attackInfoSO.groundBounceDuration, attackInfoSO.groundBounceDirection, attackInfoSO.groundBounceHitStopDuration));
        }
        if (forceDirection.y > 0f || !hitFighter.MovementController.IsGrounded)
        {
            StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfoSO.hangTime));
        }
        //StartCoroutine(hitFighter.MovementController.DisableGravity(attackInfo.hangTime));
        //Services.FavorManager?.IncreaseFavor(_fighter.PlayerId, attackInfo.favorReward);
        Services.FavorManager.IncreaseFavor(_fighter.PlayerId, attackInfoValues.outputReward);
        //num++;
        //Services.AttackInfoManager.debugText.text = num.ToString();

        //TODO: multi-hit attacks should only decay after the entire attack has finished
        if (attackInfoSO.multiHitCount <= 1)
        {
            StartCoroutine(Services.AttackInfoManager.DecayValues(attackInfo));
        }

        StartCoroutine(Juice.FreezeTime(attackInfoSO.hitStopDuration));

        //Debug.Log("multihit");
        _canMultiHit = false;
    }

    //static int num = 0;
}
