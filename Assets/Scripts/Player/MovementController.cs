using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using WesleyDavies;
using WesleyDavies.UnityFunctions;

[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(InputManager))]
public class MovementController : MonoBehaviour
{
    public enum ForceEasing { Linear, Quadratic, Cubic }

    public float pushDistance;
    public Vector3 pushDirection;

    public Action JumpAction;

    [Header("Movement")]
    [Tooltip("How fast should this character move sideways (in units/sec)?")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("How fast should this character get pushed by another character (in units/sec)?")]
    [SerializeField] private float _opponentPushSpeed;

    public bool IsBeingPushed { get; private set; }

    [Header("Jump")]
    //[Tooltip("How high (in units) should the character jump on longest button press?")]
    //[SerializeField] private float _maxJumpHeight;
    //[Tooltip("How high (in units) should the character jump on shortest button press?")]
    //[SerializeField] private float _minJumpHeight;
    //[Tooltip("How long should the character jump (in sec)?")]
    //[SerializeField] private float _timeToJumpApex;
    [SerializeField] private Vector2 _jumpDistance;
    [SerializeField] private float _jumpDuration;
    [SerializeField] private float _gravity;
    private float _gravityModifier = 1f;
    private float _diagonalJumpDistance;
    private float _verticalJumpForce;
    private float _horizontalJumpForce;
    private float _diagonalJumpForce;

    [Header("Dash")]
    [SerializeField] private float _dashDistance;
    private float _dashForce;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _shortDashDistance;
    [SerializeField] private float _shortDashDuration;
    [SerializeField] private float _dashHangTime;
    [SerializeField] private bool _dashToZero;
    [SerializeField] private float _shortDashCooldownDuration;
    [SerializeField] private float _superDashCooldownDuration;
    [SerializeField] private ForceEasing _dashEasing;
    [SerializeField] private float _dashMinimumOverlap;
    //private int _airShortDashChargeCount = 1;
    //private int _airSuperDashChargeCount = 1;
    private float _shortDashForce = 0f;
    private bool _isShortDashing = false; //TODO: these variables are stupidd
    [SerializeField] private int _maxDashCharges;
    [SerializeField] private int _dashChargeCount;
    [SerializeField] private float _dashRechargeTime;

    [Header("Roll")]
    [SerializeField] private float _rollDistance;
    private float _rollForce;
    [SerializeField] private float _rollDuration;

    [Header("Collisions")]
    [Tooltip("What percentage of a force that hits this fighter should push it backwards?")]
    [SerializeField][Range(0f, 1f)] private float _secondaryForcePercentage;
    [Tooltip("What layer(s) should collisions be checked on?")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private LayerMask _playerMask;
    [Tooltip("How many rays should be shot out horizontally?")]
    private int _xAxisRayCount = 3;
    //[Tooltip("How many rays should be shot out forward?")]
    //private int _zAxisRayCount = 3;
    [Tooltip("How many rays should be shot out vertically?")]
    private int _yAxisRayCount = 3;
    [Tooltip("Should debug rays be drawn?")]
    [SerializeField] private bool _drawDebugRays;

    private Fighter _fighter;
    public BoxCollider BoxCollider { get; private set; }
    private PlayerInput _playerInput;
    private InputManager _inputManager;

    private Vector3 _netVelocity;
    private Vector3 _unforcedVelocity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private Vector2 _xAxisRaySpacing;
    private Vector2 _yAxisRaySpacing;
    //private Vector2 _zAxisRaySpacing;
    private Vector3 _forceVelocity;

    //private float _sidestepForce = 10f;

    private bool _isWallBounceable = false;
    private float _wallBounceDistance;
    private float _wallBounceDuration;
    private Vector3 _wallBounceDirection;
    private float _wallBounceHitStopDuration;

    private bool _isGroundBounceable = false;
    private float _groundBounceDistance;
    private float _groundBounceDuration;
    private Vector3 _groundBounceDirection;
    private float _groundBounceHitStopDuration;

    private bool _isAttacking;
    private bool _isGravityApplied = true;
    [SerializeField] private float _overlapResolutionSpeed;
    private bool _isResolvingOverlap = false;
    public Vector3 overlapResolutionVelocity = Vector3.zero;
    private Vector3 _overlapResolutionVelocity = Vector3.zero;
    public Fighter.Direction MovingDirection { get; private set; }

    [SerializeField] private float _mass = 1f;

    //private float _sidestepDuration;

    private RaycastOrigins _raycastOrigins;
    public CollisionInfo CollisionData
    {
        get => _collisionData;
        private set => _collisionData = value;
    }
    private CollisionInfo _collisionData;

    private readonly float _skinWidth = 0.1f;

    //private float _accelerationTimeAirborne = .2f;
    //private float _accelerationTimeGrounded = .1f;
    private Vector2 _horizontalVelocitySmoothing;
    private Vector2 _horizontalTargetVelocity;
    private Vector3 _cachedVelocity;

    public bool IsGrounded
    {
        get => CollisionData.y.isNegativeHit;
    }


    [SerializeField] private PhysicsManager _physicsManager;

    [SerializeField] private LayerMask _yMask;
    [SerializeField] private LayerMask _xMask;

    private List<IEnumerator> _forceCoroutines = new();

    private IEnumerator _flipCoroutine;
    [Tooltip("How long it takes before the fighter flips around after the opponent has switched sides.")]
    [SerializeField] private float _flipDelayTime;
    private bool _isFlipQueued = false;
    private Fighter.Direction _nextFlipDirection;

    /// <summary>
    /// Returns true if Opponent is behind this Fighter.
    /// </summary>
    private bool _isOpponentBehind = false;

    [Header("Base States Subscription")]
    [SerializeField] private BaseState _move;
    [SerializeField] private BaseState[] _dashes;
    [SerializeField] private BaseState _jump;

    public enum AugmentType { None, Self, Hit }
    private AugmentType _gravityAugmentType = AugmentType.None;
    
    
    private struct RaycastOrigins
    {
        public Axis x;
        //public Axis z;
        public Axis y;

        public struct Axis
        {
            public Vector3 negative;
            public Vector3 positive;
        }
    }

    public struct CollisionInfo
    {
        public Axis x;
        //public Axis z;
        public Axis y;
        public bool isAnyHit
        {
            get => x.isNegativeHit || x.isPositiveHit || y.isNegativeHit || y.isPositiveHit;
        }

        public struct Axis
        {
            [Tooltip("Is there a collision in the axis's negative direction?")]
            public bool isNegativeHit;
            [Tooltip("Is there a collision in the axis's positive direction?")]
            public bool isPositiveHit;
        }

        public void Reset()
        {
            x.isNegativeHit = x.isPositiveHit = y.isNegativeHit = y.isPositiveHit = false;
        }
    }

    private void Awake()
    {
        AssignComponents();
    }

    private void Start()
    {
        SpaceRays();
        SubscribeActions();

        //_gravity = (2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
        //_maxJumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
        //_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * _minJumpHeight);

        _diagonalJumpDistance = Vector2.Distance(Vector2.zero, _jumpDistance);
        _verticalJumpForce = _jumpDistance.y / (_jumpDuration + Time.fixedDeltaTime);
        _horizontalJumpForce = _jumpDistance.x / (_jumpDuration * 2f + Time.fixedDeltaTime);
        _diagonalJumpForce = _diagonalJumpDistance / (_jumpDuration + Time.fixedDeltaTime);

        AssignSpecialConditions();


        //_dashForce = CalculateForceFromDistance(_dashDistance, _physicsManager.deceleration);

        switch (_dashEasing)
        {
            case ForceEasing.Linear:
                //_dashForce = (_dashDistance * 2f) / (_dashDuration * Time.fixedDeltaTime + _dashDuration);
                _dashForce = (_dashDistance * 2f) / (_dashDuration + Time.fixedDeltaTime);
                _shortDashForce = (_shortDashDistance * 2f) / (_shortDashDuration + Time.fixedDeltaTime);
                _rollForce = (_rollDistance * 2f) / (_rollDuration + Time.fixedDeltaTime);
                break;
            case ForceEasing.Quadratic:
                //_dashForce = (_dashDistance * 3f) / (_dashDuration * Time.fixedDeltaTime + 1f + (Time.fixedDeltaTime / 2f));
                //_dashForce = ((_dashDistance * 3f) / (_dashDuration * Time.fixedDeltaTime + 1f)) - 4f * Time.fixedDeltaTime;
                //_dashForce = (_dashDistance * 3f) / (_dashDuration * Time.fixedDeltaTime + 1f) * (1f - Time.fixedDeltaTime / 2f) + Time.fixedDeltaTime / 4f;
                //_dashForce = (_dashDistance * 3f) / (_dashDuration * Time.fixedDeltaTime + 1f);
                //_dashForce = _dashDistance - _dashDuration;


                //float a = _dashDistance * Time.fixedDeltaTime / Mathf.Pow(_dashDuration / Time.fixedDeltaTime, 2f);
                //float c = (a * Mathf.Pow(_dashDuration / Time.fixedDeltaTime, 2f) + _dashDistance * Time.fixedDeltaTime - a) / (_dashDuration / Time.fixedDeltaTime + 1f);
                //float b = _dashDistance * Time.fixedDeltaTime - a - c;
                //_dashForce = _dashDistance * Time.fixedDeltaTime - _dashDuration / Time.fixedDeltaTime * b + c;

                //Debug.Log(a + ", " + b + ", " + c);

                ////float a = _dashDistance * Time.fixedDeltaTime / Mathf.Pow(_dashDuration / Time.fixedDeltaTime, 2f);
                //float a = 0.2f;
                ////float c = (a * Mathf.Pow(_dashDuration / Time.fixedDeltaTime, 2f) + _dashDuration / Time.fixedDeltaTime * _dashDistance * Time.fixedDeltaTime - _dashDuration / Time.fixedDeltaTime * a) / (_dashDuration / Time.fixedDeltaTime - 1f);
                //float c = ((a * _dashDuration / Time.fixedDeltaTime + _dashDistance * Time.fixedDeltaTime - a) / (_dashDuration / Time.fixedDeltaTime - 1f)) * _dashDuration / Time.fixedDeltaTime;
                //float b = _dashDistance - a - c;
                //_dashForce = _dashDistance * Time.fixedDeltaTime - _dashDuration / Time.fixedDeltaTime * b - c;

                //Debug.Log(a + ", " + b + ", " + c);

                //float a = _dashDistance / Mathf.Pow(_dashDuration, 2f);
                //float c = (a * Mathf.Pow(_dashDuration, 2f) + _dashDuration * _dashDistance - _dashDuration * a) / (_dashDuration - 1f);
                //float b = _dashDistance - a - c;
                //_dashForce = _dashDistance - _dashDuration * b + c;

                //Debug.Log(a + ", " + b + ", " + c);

                _dashForce = (_dashDistance * 3f) / (_dashDuration * _dashDuration * Time.fixedDeltaTime * Time.fixedDeltaTime + 1f);

                break;
            case ForceEasing.Cubic:
                _dashForce = (_dashDistance * 4f) / (_dashDuration * Time.fixedDeltaTime + 1f);
                break;
        }
        //_shortDashForce = _dashForce / 1.5f; //TODO: MAGIC NUMBER
        _dashChargeCount = _maxDashCharges;
    }

    private void AssignSpecialConditions()
    {
        _inputManager.Actions["Jump"].enableConditions.Add(() => _collisionData.y.isNegativeHit);
        //_inputManager.Disable(_inputManager.Actions["Roll"]);
    }

    private bool _isForcingOpponent = false;
    private Vector3 _secondaryForceVelocity = Vector3.zero;

    private void FixedUpdate()
    {
        SpaceRays();

        switch (_fighter.FacingDirection)
        {
            case Fighter.Direction.Left:
                if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
                {
                    _isOpponentBehind = true;
                    _nextFlipDirection = Fighter.Direction.Right;
                }
                break;
            case Fighter.Direction.Right:
                if (_fighter.OpposingFighter.transform.position.x < transform.position.x)
                {
                    _isOpponentBehind = true;
                    _nextFlipDirection = Fighter.Direction.Left;
                }
                break;
        }

        if (_isOpponentBehind)
        {
            if(_fighter.BaseStateMachine.IsIdle || _fighter.BaseStateMachine.IsCrouch)
            {
                _fighter.FlipCharacter(_nextFlipDirection);
                _isOpponentBehind = false;
            }
            else
            {
                if (!_isFlipQueued)
                {
                    StartCoroutine(QueueFlip(_nextFlipDirection));
                }
            }
        }
        Vector3 preGravityVelocity = _unforcedVelocity + _forceVelocity + _overlapResolutionVelocity;
        if (_isGravityApplied)
        {
            _unforcedVelocity.y -= _gravity * _gravityModifier * Time.fixedDeltaTime;
            //_unforcedVelocity.y -= _gravity * Time.fixedDeltaTime;
        }
        _netVelocity = _unforcedVelocity + _forceVelocity + overlapResolutionVelocity;
        _netVelocity.y *= _gravityModifier;
        Move(_netVelocity * Time.fixedDeltaTime);
        if (_collisionData.y.isNegativeHit || _collisionData.y.isPositiveHit)
        {
            ResetVelocityY();
            if (_gravityModifier != 1f)
            {
                RestoreGravity();
            }
        }
        if (!_isWallBounceable)
        {
            //if (_netVelocity.x > 0f)
            //{
            //    if (_collisionData.x.isPositiveHit)
            //    {
            //        KillAllForces();
            //    }
            //}
            //if (_netVelocity.x < 0f)
            //{
            //    if (_collisionData.x.isNegativeHit)
            //    {
            //        KillAllForces();
            //    }
            //}
            //if (_netVelocity.y > 0f)
            //{
            //    if (_collisionData.y.isPositiveHit)
            //    {
            //        KillAllForces();
            //    }
            //}
            if (preGravityVelocity.y < 0f)
            {
                if (_collisionData.y.isNegativeHit)
                {
                    KillAllForces();
                }
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
        StopAllCoroutines();
    }

    public void ResetVelocityY()
    {
        _unforcedVelocity.y = 0f;
    }

    public void ResetToStartingY()
    {
        //float newY = _fighter.PlayerId == 0 ? _fighter.FightersManager.player1StartPosition.y : _fighter.FightersManager.player2StartPosition.y;
        //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        ResetVelocityY();
    }

    public void MoveToX(float x)
    {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public void ResetValues()
    {
        KillAllForces();
        StopMoving();
        ResetVelocityY();
        ResetAllVelocities();
        //RestoreOpponentGravity();
        RestoreGravity();
    }

    private void ResetAllVelocities()
    {
        _unforcedVelocity = Vector3.zero;
        _forceVelocity = Vector3.zero;
        overlapResolutionVelocity = Vector3.zero;
    }

    private void KillAllForces()
    {
        int max = _forceCoroutines.Count;
        for (int i = 0; i < max; i++)
        {
            StopCoroutine(_forceCoroutines[i]);
        }
        _forceCoroutines.Clear();
        _forceVelocity = Vector3.zero;
    }

    private void StateChange(BaseState state)
    {
        if (_gravityAugmentType == AugmentType.Self)
        {
            RestoreGravity();
        }
        if (_canFlip)
        {
            _canFlip = false;
        }
    }

    private bool _canFlip = false;

    //TODO: use fixed update instead of waitforseconds?
    private IEnumerator QueueFlip(Fighter.Direction newDirection)
    {
        _isFlipQueued = true;
        _canFlip = true;
        //yield return new WaitUntil(() => _fighter.BaseStateMachine.IsIdle || _fighter.BaseStateMachine.IsCrouch);
        yield return new WaitUntil(() => !_canFlip);

        _isFlipQueued = false;
        if (_isOpponentBehind)
        {
            _fighter.FlipCharacter(_nextFlipDirection);
            _isOpponentBehind = false;
        }
        yield break;

        //_isFlipQueued = true;
        //_nextFlipDirection = newDirection;
        //yield return new WaitForSeconds(_flipDelayTime);

        //_fighter.FlipCharacter(_nextFlipDirection);
        //_isFlipQueued = false;
        //yield break;
    }

    public IEnumerator EnableWallBounce(float distance, float duration, Vector3 direction, float hitStopDuration)
    {
        _wallBounceDistance = distance;
        _wallBounceDuration = duration;
        _wallBounceDirection = direction;
        _wallBounceHitStopDuration = hitStopDuration;
        _isWallBounceable = true;
        yield return new WaitUntil(() => _forceVelocity == Vector3.zero);

        _isWallBounceable = false;
        yield break;
    }

    public IEnumerator EnableGroundBounce(float distance, float duration, Vector3 direction, float hitStopDuration)
    {
        _groundBounceDistance = distance;
        _groundBounceDuration = duration;
        _groundBounceDirection = direction;
        _groundBounceHitStopDuration = hitStopDuration;
        _isGroundBounceable = true;
        yield return new WaitUntil(() => _forceVelocity == Vector3.zero);

        _isGroundBounceable = false;
        yield break;
    }

    public IEnumerator DisableGravity(float duration)
    {
        _isGravityApplied = false;
        yield return new WaitForSeconds(duration);

        _isGravityApplied = true;
        yield break;
    }

    public IEnumerator DisableGravity(Func<bool> enableCondition)
    {
        Debug.Log("Func");
        _isGravityApplied = false;
        yield return new WaitUntil(enableCondition);

        _isGravityApplied = true;
        yield break;
    }

    private IEnumerator WallBounce(Vector3 direction, float magnitude, float duration)
    {
        _fighter.Events.wallBounce?.Invoke();
        yield return null;

        StartCoroutine(Juice.FreezeTime(_wallBounceHitStopDuration));
        _isWallBounceable = false; //TODO: instead, stop EnableWallBounce coroutine
        ApplyForce(direction, magnitude, duration, true);
        yield break;
    }

    private IEnumerator GroundBounce(Vector3 direction, float magnitude, float duration)
    {
        _fighter.Events.groundBounce?.Invoke();
        yield return null;

        StartCoroutine(Juice.FreezeTime(_groundBounceHitStopDuration));
        _isGroundBounceable = false; //TODO: instead, stop EnableGroundBounce coroutine
        ApplyForce(direction, magnitude, duration, true);
        yield break;
    }

    public void ApplyForce(Vector3 direction, float magnitude, float duration, bool isMomentumReset = false, float collideDecay = 0f, ForceEasing easingFunction = ForceEasing.Linear)
    {
        //collide decay is how much the force decays when it collides into something

        if (isMomentumReset) //TODO: do this for ApplyForcePolar as well
        {
            KillAllForces();
        }
        IEnumerator forceCoroutine = Force(direction, magnitude, duration, easingFunction);
        _forceCoroutines.Add(forceCoroutine);
        StartCoroutine(forceCoroutine);

        IEnumerator Force(Vector3 direction, float magnitude, float duration, ForceEasing easingFunction = ForceEasing.Linear)
        {
            direction.Normalize();
            float timer = 0f;
            Easing function;
            switch (easingFunction)
            {
                case ForceEasing.Linear:
                    function = Easing.CreateEasingFunc(Easing.Funcs.Linear);
                    break;
                case ForceEasing.Quadratic:
                    function = Easing.CreateEasingFunc(Easing.Funcs.QuadraticOut);
                    break;
                case ForceEasing.Cubic:
                    function = Easing.CreateEasingFunc(Easing.Funcs.CubicOut);
                    break;
                default:
                    throw new System.Exception("Invalid easing function provided.");
            }
            while (timer < duration)
            {
                if (_isWallBounceable)
                {
                    if (_collisionData.x.isNegativeHit || _collisionData.x.isPositiveHit)
                    {
                        ResetVelocityY();
                        if (_wallBounceDistance != 0f)
                        {
                            Vector3 bounceDirection = Mathf.Sign(direction.x) == 1f ? _wallBounceDirection : new Vector3(-_wallBounceDirection.x, _wallBounceDirection.y, _wallBounceDirection.z);
                            float bounceMagnitude = (_wallBounceDistance * 2f) / (_wallBounceDuration + Time.fixedDeltaTime);
                            StartCoroutine(WallBounce(bounceDirection, bounceMagnitude, _wallBounceDuration));
                            yield break;
                        }
                        direction.x *= -1f;
                    }
                    //if (_collisionData.z.isNegativeHit || _collisionData.z.isPositiveHit)
                    //{
                    //    ResetVelocityY();
                    //    if (_wallBounceDistance != 0f)
                    //    {
                    //        Vector3 bounceDirection = Mathf.Sign(direction.z) == 1f ? _wallBounceDirection : new Vector3(_wallBounceDirection.x, _wallBounceDirection.y, -_wallBounceDirection.z);
                    //        float bounceMagnitude = (_wallBounceDistance * 2f) / (_wallBounceDuration + Time.fixedDeltaTime);
                    //        StartCoroutine(WallBounce(bounceDirection, bounceMagnitude, _wallBounceDuration));
                    //        yield break;
                    //    }
                    //    direction.z *= -1f;
                    //}
                }
                if (_isGroundBounceable)
                {
                    if (_collisionData.y.isNegativeHit)
                    {
                        ResetVelocityY();
                        if (_groundBounceDistance != 0f)
                        {
                            Vector3 bounceDirection = Mathf.Sign(direction.x) == 1f ? _groundBounceDirection : new Vector3(-_groundBounceDirection.x, _groundBounceDirection.y, _groundBounceDirection.z);
                            float bounceMagnitude = (_groundBounceDistance * 2f) / (_groundBounceDuration + Time.fixedDeltaTime);
                            StartCoroutine(GroundBounce(bounceDirection, bounceMagnitude, _groundBounceDuration));
                            yield break;
                        }
                        direction.y *= -1f;
                    }
                }
                float forceMagnitude = function.Ease(magnitude, 0f, timer / duration);
                Vector3 force = direction * forceMagnitude;
                _forceVelocity += force;
                yield return new WaitForFixedUpdate();

                _forceVelocity -= force;
                timer += Time.fixedDeltaTime;
            }
            //TODO: move somewhere else?
            _inputManager.Actions["Roll"].finish.Invoke(_inputManager.Actions["Roll"]);
            if (!_inputManager.Actions["Move"].isBeingPerformed)
            {
                _unforcedVelocity.x = 0f;
                //_unforcedVelocity.z = 0f;
            }
            else
            {
                Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<float>() < 0f ? Vector2.left : Vector2.right;
                inputVector *= _moveSpeed;
                _unforcedVelocity.x = inputVector.x;
                //_unforcedVelocity.z = inputVector.y;
            }
            yield break;
        }
    }

    //private float CalculateDecelerationDuration(float forceMagnitude, float deceleration)
    //{
    //    int i = 0;
    //    float totalDecelerationForce = 0f;
    //    float timeStep = Time.fixedDeltaTime;
    //    while (totalDecelerationForce < forceMagnitude)
    //    {
    //        totalDecelerationForce += deceleration * i * i;
    //        i++;
    //    }
    //    i++;
    //    return timeStep * i;
    //}

    //private int CalculateDecelerationStepCount(float forceMagnitude, float deceleration)
    //{
    //    int i = 0;
    //    float totalDecelerationForce = 0f;
    //    while (totalDecelerationForce < forceMagnitude)
    //    {
    //        totalDecelerationForce += deceleration * i * i;
    //        i++;
    //    }
    //    i++;
    //    return i;
    //}

    //private float CalculateForceDistance(float magnitude, float deceleration, int stepCount)
    //{
    //    float distance = 0f;
    //    float forceMagnitude = magnitude;
    //    for (int i = 0; i < stepCount; i++)
    //    {
    //        distance += forceMagnitude;
    //        forceMagnitude -= deceleration * i * i;
    //    }
    //    return distance;
    //}

    struct Force
    {
        public float magnitude;
        public float distance;
        public float duration;
        public float deceleration;
        public Vector3 direction;
    }

    private float CalculateForceFromDistance(float distance, float deceleration)
    {
        int i = 0;
        float totalDistance = 0f;
        //float timeStep = Time.fixedDeltaTime;
        while (totalDistance < distance)
        {
            i++;
            float decelerationI = deceleration * i * Time.fixedDeltaTime;
            totalDistance += decelerationI * decelerationI * decelerationI;
        }
        float distanceOffset = Mathf.Pow(totalDistance - distance, 1f / (float)i);
        float startValue = 0f;
        for (int j = 1; j < i + 1; j++)
        {
            float decelerationI = deceleration * j * Time.fixedDeltaTime;
            startValue += decelerationI * decelerationI;
        }
        //Debug.Log(i);
        startValue -= distanceOffset;
        return startValue;
    }

    //public IEnumerator ApplyForcePolar(Vector3 direction, float magnitude)
    //{
    //    direction.Normalize();
    //    //magnitude /= _mass;
    //    //float acceleration = magnitude / _mass;
    //    //float acceleration = _physicsManager.deceleration * Time.fixedDeltaTime;
    //    float acceleration = _physicsManager.deceleration;
    //    float deceleration = 0f;
    //    float forceMagnitude = magnitude;
    //    float initialMagnitude = magnitude;
    //    int i = 0;
    //    while (forceMagnitude > 0f)
    //    {
    //        if (_isWallBounceable)
    //        {
    //            if (_collisionData.x.isNegativeHit || _collisionData.x.isPositiveHit)
    //            {
    //                ResetVelocityY();
    //                if (_wallBounceDistance != 0f)
    //                {
    //                    Vector3 bounceDirection = Mathf.Sign(direction.x) == 1f ? _wallBounceDirection : new Vector3(-_wallBounceDirection.x, _wallBounceDirection.y, _wallBounceDirection.z);
    //                    float bounceMagnitude = (_wallBounceDistance * 2f) / (_wallBounceDuration + Time.fixedDeltaTime);
    //                    StartCoroutine(WallBounce(bounceDirection, bounceMagnitude, _wallBounceDuration));
    //                    yield break;
    //                }
    //                direction.x *= -1f;
    //            }
    //        }
    //        Vector3 force = direction * forceMagnitude;
    //        _forceVelocity += force;
    //        i++;
    //        yield return new WaitForFixedUpdate();

    //        _forceVelocity -= force;
    //        float accelerationI = acceleration * i * Time.fixedDeltaTime;
    //        deceleration = accelerationI * accelerationI;
    //        //forceMagnitude = initialMagnitude - deceleration;
    //        forceMagnitude -= deceleration;
    //    }

    //    if (!_inputManager.Actions["Move"].isBeingPerformed)
    //    {
    //        _unforcedVelocity.x = 0f;
    //        _unforcedVelocity.z = 0f;
    //    }
    //    else
    //    {
    //        Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<float>() < 0f ? Vector2.left : Vector2.right;
    //        inputVector *= _moveSpeed;
    //        _unforcedVelocity.x = inputVector.x;
    //    }
    //    yield break;
    //}

    private void AssignComponents()
    {
        _fighter = GetComponent<Fighter>();
        _playerInput = GetComponent<PlayerInput>();
        BoxCollider = GetComponent<BoxCollider>();
        _inputManager = GetComponent<InputManager>();
    }
    
    private void SubscribeActions()
    {
        try
        {
            _fighter.BaseStateMachine.States[_move].execute += StartMoving;
            _fighter.BaseStateMachine.States[_move].stop += StopMoving;

            foreach (var dash in _dashes)
            {
                _fighter.BaseStateMachine.States[dash].execute += Dash;
            }

            if(_inputManager.Actions.ContainsKey("Dash Left"))
            {
                _inputManager.Actions["Dash Left"].perform += DashLeft;
            }

            if (_inputManager.Actions.ContainsKey("Dash Right"))
            {
                _inputManager.Actions["Dash Right"].perform += DashRight;
            }

            _inputManager.Actions["Jump"].perform += Jump;
            _inputManager.Actions["Side Jump"].perform += SideJump;
            _inputManager.Actions["Side Jump"].stop += StopSideJump;
            _inputManager.Actions["Jump"].stop += StopJumping;
            _inputManager.Actions["Roll"].perform += Roll;
            // _fighter.BaseStateMachine.States[_jump].execute += Jump;
            // _fighter.BaseStateMachine.States[_jump].stop += StopJumping;
        }
        catch (Exception e)
        {
            Debug.LogError($"movement states missing from controller {name}");
        }

        _fighter.Events.onStateChange += StateChange;
    }

    private void UnsubscribeActions()
    {
        _fighter.BaseStateMachine.States[_move].execute -= StartMoving;
        _fighter.BaseStateMachine.States[_move].stop -= StopMoving;
        foreach (var dash in _dashes)
        {
            _fighter.BaseStateMachine.States[dash].execute -= Dash;
        }
        
        if(_inputManager.Actions.ContainsKey("Dash Left"))
        {
            _inputManager.Actions["Dash Left"].perform -= DashLeft;
        }

        if (_inputManager.Actions.ContainsKey("Dash Right"))
        {
            _inputManager.Actions["Dash Right"].perform -= DashRight;
        }
        
        _inputManager.Actions["Jump"].perform -= Jump;
        _inputManager.Actions["Side Jump"].perform -= SideJump;
        _inputManager.Actions["Side Jump"].stop -= StopSideJump;
        _inputManager.Actions["Jump"].stop -= StopJumping;
        _inputManager.Actions["Roll"].perform -= Roll;
        // _fighter.BaseStateMachine.States[_jump].execute -= Jump;
        // _fighter.BaseStateMachine.States[_jump].stop -= StopJumping;

        _fighter.Events.onStateChange -= StateChange;
    }

    private void SpaceRays()
    {
        Bounds bounds = BoxCollider.bounds;
        Bounds internalBounds = bounds;
        internalBounds.Expand(_skinWidth * -2f);

        _xAxisRayCount = Mathf.Clamp(_xAxisRayCount, 2, int.MaxValue);
        //_zAxisRayCount = Mathf.Clamp(_zAxisRayCount, 2, int.MaxValue);
        _yAxisRayCount = Mathf.Clamp(_yAxisRayCount, 2, int.MaxValue);

        _xAxisRaySpacing.x = internalBounds.size.z / (_xAxisRayCount - 1);
        _xAxisRaySpacing.y = internalBounds.size.y / (_xAxisRayCount - 1);
        //_zAxisRaySpacing.x = internalBounds.size.x / (_zAxisRayCount - 1);
        //_zAxisRaySpacing.y = internalBounds.size.y / (_zAxisRayCount - 1);
        _yAxisRaySpacing.x = internalBounds.size.x / (_yAxisRayCount - 1);
        _yAxisRaySpacing.y = internalBounds.size.z / (_yAxisRayCount - 1);
    }

    private void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        _collisionData.Reset();

        if (velocity.x != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionData.x, Axis.x);
        }
        if (velocity.y != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionData.y, Axis.y);
        }

        transform.Translate(velocity);
    }

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = BoxCollider.bounds;
        Bounds internalBounds = bounds;
        internalBounds.Expand(_skinWidth * -2f);

        _raycastOrigins.x.negative = new Vector3(internalBounds.min.x, bounds.min.y, bounds.center.z);
        _raycastOrigins.x.positive = new Vector3(internalBounds.max.x, bounds.min.y, bounds.center.z);
        //_raycastOrigins.z.negative = new Vector3(bounds.min.x, bounds.min.y, internalBounds.min.z);
        //_raycastOrigins.z.positive = new Vector3(bounds.min.x, bounds.min.y, internalBounds.max.z);
        _raycastOrigins.y.negative = new Vector3(internalBounds.min.x, internalBounds.min.y, internalBounds.center.z);
        _raycastOrigins.y.positive = new Vector3(internalBounds.min.x, internalBounds.max.y, internalBounds.center.z);
    }

    /// <summary>
    /// Fire rays in axis direction and modify velocity based on RaycastHits.
    /// </summary>
    /// <param name="velocity">The velocity to modify.</param>
    /// <param name="collisionAxis">The axis of CollisionInfo to modify.</param>
    /// <param name="axis">The axis to calculate collisions on.</param>
    /// <exception cref="System.Exception"></exception>
    private void CalculateCollisions(ref Vector3 velocity, ref CollisionInfo.Axis collisionAxis, Axis axis)
    {
        float axisVelocity;
        float iOffset = 0f;
        float jOffset = 0f;
        float rayCount;
        Vector2 raySpacing;
        RaycastOrigins.Axis originAxis;
        Vector3 iDirection;
        Vector3 jDirection;
        Vector3 rayDirection;
        LayerMask collisionMask;

        //assign fields based on axis
        switch (axis)
        {
            case Axis.x:
                axisVelocity = velocity.x;
                rayCount = _xAxisRayCount;
                raySpacing = _xAxisRaySpacing;
                originAxis = _raycastOrigins.x;
                iDirection = Vector3.up;
                jDirection = Vector3.forward;
                rayDirection = Vector3.right;
                collisionAxis = _collisionData.x;
                collisionMask = _xMask;
                break;
            case Axis.y:
                axisVelocity = velocity.y;
                iOffset = velocity.z;
                jOffset = velocity.x;
                rayCount = _yAxisRayCount;
                raySpacing = _yAxisRaySpacing;
                originAxis = _raycastOrigins.y;
                iDirection = Vector3.right;
                jDirection = Vector3.forward;
                rayDirection = Vector3.up;
                collisionAxis = _collisionData.y;
                collisionMask = _yMask;
                break;
            default:
                throw new System.Exception("Invalid axis provided.");
        }

        int axisDirection = (int)Mathf.Sign(axisVelocity);
        if (axis == Axis.x)
        {
            MovingDirection = axisDirection == -1 ? Fighter.Direction.Left : Fighter.Direction.Right;
        }

        float rayLength = Mathf.Abs(axisVelocity) + _skinWidth;

        for (int i = 0; i < rayCount; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                Vector3 rayOrigin = axisDirection == -1f ? originAxis.negative : originAxis.positive;
                rayOrigin += iDirection * (raySpacing.y * i + iOffset) + jDirection * (raySpacing.x * j + jOffset);

                if (!_fighter.OpposingFighter.MovementController.IsBeingPushed)
                {
                    if (axis == Axis.x)
                    {
                        if (Mathf.Abs(_unforcedVelocity.x) > 0f)
                        {
                            if (Physics.Raycast(rayOrigin, rayDirection * axisDirection, out RaycastHit overlapHit, rayLength, _playerMask))
                            {
                                if (_fighter.OpposingFighter.BaseStateMachine.IsIdle)
                                {
                                    float pushSpeed = _fighter.OpposingFighter.MovementController._opponentPushSpeed < _moveSpeed ? _fighter.OpposingFighter.MovementController._opponentPushSpeed : _moveSpeed;
                                    StartCoroutine(_fighter.OpposingFighter.MovementController.GetPushedByOpponent(pushSpeed, axisDirection, () => _unforcedVelocity.x == 0f || !_fighter.OpposingFighter.BaseStateMachine.IsIdle));
                                    _unforcedVelocity = new Vector3(pushSpeed * axisDirection, _unforcedVelocity.y, _unforcedVelocity.z);
                                }
                            }
                        }
                    }
                }

                //if (axis == Axis.x) //TODO: check on y axis too
                //{
                //    if (_forceVelocity != Vector3.zero)
                //    {
                //        if (Physics.Raycast(rayOrigin, rayDirection * axisDirection, out RaycastHit overlapHit, rayLength, _playerMask))
                //        {
                //            _isForcingOpponent = true;
                //            _fighter.OpposingFighter.MovementController._secondaryForceVelocity = _forceVelocity * _fighter.OpposingFighter.MovementController._secondaryForcePercentage;
                //            Debug.Log(_fighter.OpposingFighter.MovementController._netVelocity);
                //        }
                //        else
                //        {
                //            _isForcingOpponent = false;
                //            _fighter.OpposingFighter.MovementController._secondaryForceVelocity = Vector3.zero;
                //        }
                //    }
                //    else
                //    {
                //        _isForcingOpponent = false;
                //        _fighter.OpposingFighter.MovementController._secondaryForceVelocity = Vector3.zero;
                //    }
                //}

                //check collisions
                if (Physics.Raycast(rayOrigin, rayDirection * axisDirection, out RaycastHit hit, rayLength, collisionMask))
                {
                    switch (axis)
                    {
                        case Axis.x:
                            velocity.x = (hit.distance - _skinWidth) * axisDirection;
                            break;
                        case Axis.z:
                            velocity.z = (hit.distance - _skinWidth) * axisDirection;
                            break;
                        case Axis.y:
                            velocity.y = (hit.distance - _skinWidth) * axisDirection;
                            break;
                    }
                    rayLength = hit.distance;

                    collisionAxis.isNegativeHit = axisDirection == -1f;
                    collisionAxis.isPositiveHit = axisDirection == 1f;
                }

                if (_drawDebugRays)
                {
                    Debug.DrawRay(rayOrigin, axisDirection * rayLength * rayDirection, Color.red);
                }
            }
        }
    }

    public IEnumerator GetPushedByOpponent(float speed, int axisDirection, Func<bool> stopCondition)
    {
        IsBeingPushed = true;
        _unforcedVelocity = new Vector3(speed * axisDirection, _unforcedVelocity.y, _unforcedVelocity.z);
        yield return new WaitUntil(stopCondition);

        _unforcedVelocity= new Vector3(0f, _unforcedVelocity.y, _unforcedVelocity.z);
        IsBeingPushed = false;
        yield break;
    }

    //TODO: this is deprecated
    public IEnumerator ResolveOverlap()
    {
        _isResolvingOverlap = true;
        //bool opponentIsRight = false;
        if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
        {
            //opponentIsRight = true;
            _overlapResolutionVelocity.x = -_overlapResolutionSpeed;
        }
        else
        {
            _overlapResolutionVelocity.x = _overlapResolutionSpeed;
        }
        //yield return new WaitUntil(opponentIsRight ? () => _collisionData.x.isPositiveHit : () => _collisionData.x.isNegativeHit);
        yield return new WaitForSeconds(0.1f);

        _overlapResolutionVelocity = Vector3.zero;
        _isResolvingOverlap = false;
        yield break;
    }

    private void StartMoving()
    {
        Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<float>() < 0f ? Vector2.left : Vector2.right;
        inputVector *= _moveSpeed;
        _unforcedVelocity.x = inputVector.x;
    }

    private void StopMoving()
    {
        _unforcedVelocity.x = 0f;
        //_unforcedVelocity.z = 0f;
    }

    private bool _landingIsAwaited = false;

    public void Push(float duration)
    {
        float magnitude = (pushDistance * 2f) / (duration + Time.fixedDeltaTime);
        Vector3 direction = _fighter.FacingDirection == Fighter.Direction.Right ? pushDirection : new Vector3(-pushDirection.x, pushDirection.y, pushDirection.z);
        ApplyForce(direction, magnitude, duration, true);
    }

    //public void AugmentOpponentGravity(float factor)
    //{
    //    _awaitHitOpponent = AwaitHitOpponent(factor);
    //    StartCoroutine(_awaitHitOpponent);
    //}

    private bool _isAwaitingHit = false;
    private IEnumerator _awaitHitOpponent;

    //private IEnumerator AwaitHitOpponent(float factor) //TODO: needs a better name
    //{
    //    _isAwaitingHit = true;
    //    yield return new WaitUntil(() => _fighter.BaseStateMachine.HitOpponent);
    //    _fighter.OpposingFighter.MovementController._gravityModifier = factor;
    //    _isAwaitingHit = false;
    //    yield break;
    //}

    //public void RestoreOpponentGravity()
    //{
    //    if (_isAwaitingHit)
    //    {
    //        StopCoroutine(_awaitHitOpponent);
    //    }
    //    _fighter.OpposingFighter.MovementController._gravityModifier = 1f;
    //}

    private IEnumerator _gravityAugmenter;

    public void AugmentGravity(float factor)
    {
        AugmentGravity(factor, Mathf.Infinity, AugmentType.Self, false);
    }

    public void RestoreGravity()
    {
        if (_gravityAugmentType != AugmentType.None)
        {
            StopCoroutine(_gravityAugmenter);
            _gravityAugmentType = AugmentType.None;
        }
        _gravityModifier = 1f;
    }

    public void AugmentGravity(float factor, float duration, AugmentType gravityAugmentType = AugmentType.Hit, bool isComingFromGround = true)
    {
        if (_gravityAugmentType != AugmentType.None)
        {
            StopCoroutine(_gravityAugmenter);
            _gravityAugmentType = AugmentType.None;
        }
        _gravityAugmenter = GravityAugmenter(factor, duration, gravityAugmentType, isComingFromGround);
        StartCoroutine(_gravityAugmenter);
    }

    private IEnumerator GravityAugmenter(float factor, float duration, AugmentType gravityAugmentType, bool isComingFromGround)
    {
        _gravityAugmentType = gravityAugmentType;
        if (isComingFromGround)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => _netVelocity.y < 0f);
        }
        _gravityModifier = factor;
        yield return new WaitForSeconds(duration);
        _gravityModifier = 1f; //TODO: maybe only reset this if they're not being augmented elsewhere
        _gravityAugmentType = AugmentType.None;
        yield break;
    }

    private Vector3 _dashDirection = Vector3.zero;
    private bool _isDashing = false;

    private void Dash()
    {
        _isDashing = true;
        StartCoroutine(_inputManager.Actions["Jump"].AddOneShotEnableCondition(() => !_isDashing));
        //_unforcedVelocity.x = 0f;
        //_unforcedVelocity.z = 0f;
        //TODO: end the dash if player hits an obstacle
        //IF PLAYER USES BUTTON
        Vector3 dashDirection = Vector3.zero;
        if (_dashDirection != Vector3.zero)
        {
            dashDirection = _dashDirection;
            _dashDirection = Vector3.zero;
        }
        else
        {
            if (_inputManager.Actions["Move"].isBeingInput)
            {
                Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<float>() < 0f ? Vector2.left : Vector2.right;
                dashDirection = new Vector3(inputVector.x, 0f, 0f);
                if (_dashToZero)
                {
                    _unforcedVelocity.x = 0f;
                    //_unforcedVelocity.z = 0f;
                }
            }
            else
            {
                dashDirection = _fighter.FacingDirection == Fighter.Direction.Left ? Vector3.left : Vector3.right;
                MovingDirection = dashDirection == Vector3.left ? Fighter.Direction.Left : Fighter.Direction.Right;
            }
        }
        //Debug.Log(action.inputAction.ReadValue<float>());
        //Vector3 dashDirection = _inputManager.Actions["Dash"].inputAction.ReadValue<float>() < 0 ? Vector2.left : Vector2.right;
        //MovingDirection = dashDirection == Vector3.left ? Fighter.Direction.Left : Fighter.Direction.Right;
        //if (_dashToZero)
        //{
        //    _unforcedVelocity.x = 0f;
        //    _unforcedVelocity.z = 0f;
        //}
        float dashForce;
        if (_isShortDashing)
        {
            dashForce = _shortDashForce;
            if (!IsGrounded)
            {
                StartCoroutine(_inputManager.Actions["Dash Left"].AddOneShotEnableCondition(() => IsGrounded));
                StartCoroutine(_inputManager.Actions["Dash Right"].AddOneShotEnableCondition(() => IsGrounded));
            }
            StartCoroutine(Dash(_inputManager.Actions["Dash"], _dashDuration, false));
        }
        else
        {
            if (!IsGrounded)
            {
                StartCoroutine(_inputManager.Actions["Dash"].AddOneShotEnableCondition(() => IsGrounded));
            }
            StartCoroutine(Dash(_inputManager.Actions["Dash"], _dashDuration, true));
            dashForce = _dashForce;
            _dashChargeCount--;
            if (_dashChargeCount <= 0)
            {
                StartCoroutine(_inputManager.Actions["Dash"].AddOneShotEnableCondition(() => _dashChargeCount > 0));
            }
        }
        //ApplyForce(dashDirection, dashForce, _dashDuration, true, _dashEasing);
        ApplyForce(dashDirection, dashForce, _dashDuration, true);
        //StartCoroutine(ApplyForcePolar(dashDirection, _dashForce));
        if (!CollisionData.y.isNegativeHit)
        {
            StartCoroutine(DisableGravity(_dashHangTime));
            ResetVelocityY();
        }
        StartCoroutine(_inputManager.Disable(_dashDuration, _inputManager.Actions["Move"], _inputManager.Actions["Dash"]));
        if (!_isShortDashing)
        {
            //dash through opponent
            if (_fighter.FacingDirection == Fighter.Direction.Right)
            {
                if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
                {
                    if (transform.position.x + _dashDistance > _fighter.OpposingFighter.transform.position.x + _fighter.OpposingFighter.MovementController.BoxCollider.bounds.size.x)
                    {
                        //RemoveCollisionLayer(ref _xMask, 9);
                        StartCoroutine(DisableXCollisionLayers(_dashDuration, 9));
                        StartCoroutine(DisableOverlapXLayers(_dashDuration, 9));
                    }
                    else if (transform.position.x + _dashDistance > _fighter.OpposingFighter.transform.position.x + _dashMinimumOverlap)
                    {
                        //RemoveCollisionLayer(ref _xMask, 9);
                        StartCoroutine(DisableXCollisionLayers(_dashDuration, 9));
                        StartCoroutine(DisableOverlapXLayers(_dashDuration, 9));
                        //if (!_isResolvingOverlap)
                        //{
                        //    StartCoroutine(ResolveOverlap());
                        //}
                    }
                }
            }
            if (_fighter.FacingDirection == Fighter.Direction.Left)
            {
                if (_fighter.OpposingFighter.transform.position.x < transform.position.x)
                {
                    if (transform.position.x - _dashDistance < _fighter.OpposingFighter.transform.position.x - _fighter.OpposingFighter.MovementController.BoxCollider.bounds.size.x)
                    {
                        //RemoveCollisionLayer(ref _xMask, 9);
                        StartCoroutine(DisableXCollisionLayers(_dashDuration, 9));
                        StartCoroutine(DisableOverlapXLayers(_dashDuration, 9));
                    }
                    else if (transform.position.x - _dashDistance < _fighter.OpposingFighter.transform.position.x - _dashMinimumOverlap)
                    {
                        //RemoveCollisionLayer(ref _xMask, 9);
                        StartCoroutine(DisableXCollisionLayers(_dashDuration, 9));
                        StartCoroutine(DisableOverlapXLayers(_dashDuration, 9));
                        //if (!_isResolvingOverlap)
                        //{
                        //    StartCoroutine(ResolveOverlap());
                        //}
                    }
                }
            }
        }
        if (_isShortDashing)
        {
            _isShortDashing = false;
        }
        StartCoroutine(RechargeDash());
    }

    private void DashLeft(InputManager.Action action)
    {
        _dashDirection = Vector3.left;
        MovingDirection = Fighter.Direction.Left;
        _isShortDashing = true;
        _inputManager.Actions["Dash"].perform?.Invoke(action);
    }

    private void DashRight(InputManager.Action action)
    {
        _dashDirection = Vector3.right;
        MovingDirection = Fighter.Direction.Right;
        _isShortDashing = true;
        _inputManager.Actions["Dash"].perform?.Invoke(action);
    }

    private void Jump(InputManager.Action action)
    {
        //_unforcedVelocity.y = _maxJumpVelocity;
        StartCoroutine(DisableGravity(0f));
        ApplyForce(Vector3.up, _verticalJumpForce, _jumpDuration, true);
        StartCoroutine(CheckSideJump());
        StartCoroutine(_inputManager.Disable(() => _collisionData.y.isNegativeHit, _inputManager.Actions["Jump"], _inputManager.Actions["Move"]));
        JumpAction?.Invoke();
    }

    private void Roll(InputManager.Action action)
    {
        //Debug.Log("roll");
        Vector2 rollDirection = _inputManager.Actions["Roll"].inputAction.ReadValue<float>() < 0f ? Vector2.left : Vector2.right;
        ApplyForce(rollDirection, _rollForce, _rollDuration, true);
    }

    private Vector3 _sideJumpInputVector = Vector3.zero;

    private void SideJump(InputManager.Action action)
    {
        _sideJumpInputVector = _inputManager.Actions["Side Jump"].inputAction.ReadValue<float>() < 0f ? Vector3.left : Vector3.right;
    }

    private void StopSideJump(InputManager.Action action)
    {
        _sideJumpInputVector = Vector3.zero;
    }

    private IEnumerator CheckSideJump()
    {
        float timer = 0f;
        float duration = 0.1f;
        while (timer < duration)
        {
            if (_sideJumpInputVector != Vector3.zero)
            {
                ApplyForce(_sideJumpInputVector, _horizontalJumpForce, _jumpDuration * 2f, false);
                StartCoroutine(DisableXCollisionLayers(_jumpDuration, 9));
                StartCoroutine(DisableOverlapXLayers(_jumpDuration / 2, 9));
                yield break;
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    private void StopJumping(InputManager.Action action)
    {
        //if (_unforcedVelocity.y > _minJumpVelocity)
        //{
        //    _unforcedVelocity.y = _minJumpVelocity;
        //}
    }

    private IEnumerator Dash(InputManager.Action action, float duration, bool isSuperDash)
    {
        yield return new WaitForSeconds(duration);
        _inputManager.Actions["Dash"].finish?.Invoke(action);
        if (!isSuperDash)
        {
            //StartCoroutine(_inputManager.Disable(_superDashCooldownDuration, _inputManager.Actions["Dash"]));
        }
        else
        {
            StartCoroutine(_inputManager.Disable(_shortDashCooldownDuration, _inputManager.Actions["Dash Left"], _inputManager.Actions["Dash Right"]));
        }
        _isDashing = false;
        yield break;
    }

    private IEnumerator RechargeDash()
    {
        yield return new WaitForSeconds(_dashRechargeTime);
        if (_dashChargeCount < _maxDashCharges)
        {
            _dashChargeCount++;
        }
        yield break;
    }
    
    public void EnableAttackStop()
    {
        _isAttacking = true;
        StopMoving();
        
        InputManager.Action[] actions =
        {
            _inputManager.Actions["Move"],
            _inputManager.Actions["Jump"],
            _inputManager.Actions["Dash Left"],
            _inputManager.Actions["Dash Right"],
        };
        StartCoroutine(_inputManager.Disable(() => _isAttacking == false, actions));
    }
    
    public void DisableAttackStop()
    {
        _isAttacking = false;
    }

    //private IEnumerator DisableCollisionLayers(float duration, params int[] layers)
    //{
    //    _collisionMask.RemoveLayers(layers);
    //    yield return new WaitForSeconds(duration);

    //    _collisionMask.AddLayers(layers);
    //    yield break;
    //}

    private IEnumerator DisableXCollisionLayers(float duration, int layer)
    {
        RemoveCollisionLayer(ref _xMask, layer);
        yield return new WaitForSeconds(duration);

        AddCollisionLayer(ref _xMask, layer);
        yield break;
    }

    private IEnumerator DisableOverlapXLayers(float duration, int layer)
    {
        RemoveCollisionLayer(ref Services.CollisionsManager.fightersMask, layer);
        yield return new WaitForSeconds(duration);

        AddCollisionLayer(ref Services.CollisionsManager.fightersMask, layer);
        yield break;
    }

    private void RemoveCollisionLayer(ref LayerMask layerMask, int layer)
    {
        layerMask &= ~(1 << layer);
    }

    private void AddCollisionLayer(ref LayerMask layerMask, int layer)
    {
        layerMask |= (1 << layer);
    }
}
