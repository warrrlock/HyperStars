using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AttackInfo _attackInfo;
    public AttackInfo AttackInfo => _attackInfo;
    
    enum SpawnDirection {Right, Top, Bottom}
    [SerializeField] private SpawnDirection _spawnDirection;

    [SerializeField] private List<ProjectileAction> _onSpawnActions;
    [SerializeField] private List<ProjectileAction> _onTriggerEnterActions;
    [SerializeField] private List<ProjectileAction> _onInvisibleActions;
    
    [Header("Movement")]
    [SerializeField] private float _speed;
    [Tooltip("Angle of 0 is horizontal, angle of -90 is up, 90 down.")] [Range(-90, 90)]
    [SerializeField] private int _angle;

    public Fighter Owner => _owner;
    private Fighter _owner;
    private string _input;
    private int _xDirection;
    private bool _hit; //TODO: change if there are projectiles that don't get destroyed on hit

    public void Spawn(Fighter origin, Bounds bounds)
    {
        _owner = origin;
        _input = _owner.BaseStateMachine.LastExecutedInput;
        _xDirection = _owner.FacingDirection == Fighter.Direction.Left ? -1 : 1;
        
        Vector3 spawnDirectionVector = new Vector3(
            _spawnDirection == SpawnDirection.Right ? 1*_xDirection : 0, 
            _spawnDirection == SpawnDirection.Top ? 1 : _spawnDirection == SpawnDirection.Bottom ? -1 : 0, 
            0);
        transform.position = bounds.ClosestPoint(bounds.center + spawnDirectionVector*100 );
        // Debug.Log(transform.position);

        if (_speed != 0) MoveIn(_speed);
        
        foreach (ProjectileAction action in _onSpawnActions)
            action.Execute(this);
    }

    public void DestroyIn(float time)
    {
        Destroy(gameObject, time);
    }

    public void MakeInvisible()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (!renderer) renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        renderer.enabled = false;
        ParticleSystem pc = GetComponent<ParticleSystem>();
        if (pc != null) pc.Stop();
    }

    private void MoveIn(float speed)
    {
        Quaternion angle = Quaternion.AngleAxis(_angle, Vector3.right);
        Vector3 direction = angle * Vector3.right * _xDirection;
        StartCoroutine(MoveCoroutine(direction, speed));
    }

    private IEnumerator MoveCoroutine(Vector3 direction, float speed)
    {
        while (true)
        {
            transform.Translate(direction * (speed * Time.deltaTime));
            yield return null;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_hit || (other.gameObject.layer != 7 && other.gameObject.layer != 13)) return;
        
        Fighter hitFighter = other.GetComponentInParent<Fighter>();
        if (hitFighter == null || hitFighter == _owner) return; //TODO: don't hit self ?
        _hit = true;

        if (other.gameObject.layer == 7)
            HandleDamageTo(hitFighter);
        else HandleBlockBy(hitFighter);
        
        foreach (ProjectileAction action in _onTriggerEnterActions)
            action.Execute(this);
    }
    
    private void OnBecameInvisible()
    {
        foreach (ProjectileAction action in _onInvisibleActions)
            action.Execute(this);
    }

    private void HandleBlockBy(Fighter hitFighter)
    {
        AttackInfo attackInfo = hitFighter.BaseStateMachine.AttackInfo;
        if (attackInfo == null) return;

        Vector3 hitPoint = hitFighter.GetComponent<Collider>().ClosestPoint(transform.position);

        // Debug.Log($"attacker {_owner.name}, attacked {hitFighter.name}, hit point {hitPoint}, input {_input}");
        hitFighter.Events.onBlockHit?.Invoke(new Dictionary<string, object>
            {
                {"attacker", _owner},
                {"attacked", hitFighter}, 
                {"hit point", hitPoint},
                {"attacker input", hitFighter.BaseStateMachine.LastExecutedInput},
                {"attackInfo", attackInfo},
                {"attack type", attackInfo.attackType},
            }
        );
        
    }
    
    private void HandleDamageTo(Fighter hitFighter)
    {
        if (_attackInfo == null) return;

        Vector3 hitPoint = hitFighter.GetComponent<Collider>().ClosestPoint(transform.position);

        // Debug.Log($"attacker {_owner.name}, attacked {hitFighter.name}, hit point {hitPoint}, input {_input}");
        _owner.Events.onAttackHit?.Invoke(new Dictionary<string, object>
            {
                {"attacker", _owner},
                {"attacked", hitFighter}, 
                {"hit point", hitPoint},
                {"attacker input", _input},
                {"attack info", _attackInfo},
                {"attack type", _attackInfo.attackType},
            }
        );

        float forceMagnitude = (_attackInfo.knockbackDistance * 2f) / (_attackInfo.knockbackDuration + Time.fixedDeltaTime);
        Vector3 forceDirection = _attackInfo.knockbackDirection;
        
        forceDirection.x *= _xDirection;
        hitFighter.MovementController.ApplyForce(forceDirection, forceMagnitude, _attackInfo.knockbackDuration, true);
        
        StartCoroutine(hitFighter.BaseStateMachine.SetHurtState(
            !hitFighter.MovementController.CollisionData.y.isNegativeHit 
                ? KeyHurtStatePair.HurtStateName.AirKnockBack
                : (_attackInfo.knockbackForce.x is > 0f and < 180f 
                    ? KeyHurtStatePair.HurtStateName.KnockBack 
                    : KeyHurtStatePair.HurtStateName.HitStun)
            , _attackInfo.hitStunDuration,
            _attackInfo.hardKnockdown
        ));

        hitFighter.MovementController.ResetVelocityY();
        if (_attackInfo.causesWallBounce)
        {
            StartCoroutine(hitFighter.MovementController.EnableWallBounce(_attackInfo.wallBounceDistance, _attackInfo.wallBounceDuration, _attackInfo.wallBounceDirection, _attackInfo.wallBounceHitStopDuration));
        }
        StartCoroutine(hitFighter.MovementController.DisableGravity(_attackInfo.hangTime));
        Services.FavorManager?.IncreaseFavor(_owner.PlayerId, _attackInfo.favorReward);

        // StartCoroutine(Juice.FreezeTime(_attackInfo.hitStopDuration));
        
    }
}
