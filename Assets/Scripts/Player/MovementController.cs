using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Movement")]
    [Tooltip("How fast should the character move sideways (in units/sec)?")]
    [SerializeField] private float _moveSpeed;

    [Header("Jump")]
    [Tooltip("How high (in units) should the character jump on longest button press?")]
    [SerializeField] private float _maxJumpHeight;
    [Tooltip("How high (in units) should the character jump on shortest button press?")]
    [SerializeField] private float _minJumpHeight;
    [Tooltip("How long should the character jump (in sec)?")]
    [SerializeField] private float _timeToJumpApex;

    [Header("Dash")]
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashDuration;
    [SerializeField] private bool _dashToZero;
    [SerializeField] private float _dashCooldownDuration;
    [SerializeField] private ForceEasing _dashEasing;

    [Header("Collisions")]
    [Tooltip("What layer(s) should collisions be checked on?")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private LayerMask _playerMask;
    [Tooltip("How many rays should be shot out horizontally?")]
    private int _xAxisRayCount = 3;
    [Tooltip("How many rays should be shot out forward?")]
    private int _zAxisRayCount = 3;
    [Tooltip("How many rays should be shot out vertically?")]
    private int _yAxisRayCount = 3;
    [Tooltip("Should debug rays be drawn?")]
    [SerializeField] private bool _drawDebugRays;

    private Fighter _fighter;
    public BoxCollider BoxCollider { get; private set; }
    private PlayerInput _playerInput;
    private InputManager _inputManager;

    private Vector3 _velocity;
    private Vector3 _unforcedVelocity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private float _gravity;
    private Vector2 _xAxisRaySpacing;
    private Vector2 _yAxisRaySpacing;
    private Vector2 _zAxisRaySpacing;
    private Vector3 _forceVelocity;

    private bool _isWallBounceable = false;
    private float _wallBounceDistance;
    private float _wallBounceDuration;
    private Vector3 _wallBounceDirection;
    private float _wallBounceHitStopDuration;

    private bool _isAttacking;
    private bool _isGravityApplied = true;
    [SerializeField] private float _overlapResolutionSpeed;
    private bool _isResolvingOverlap = false;
    private Vector3 _overlapResolutionVelocity = Vector3.zero;

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

    private struct RaycastOrigins
    {
        public Axis x;
        public Axis z;
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
        public Axis z;
        public Axis y;

        public struct Axis
        {
            [Tooltip("Is there a collision in the axis's negative direction?")]
            public bool isNegativeHit;
            [Tooltip("Is there a collision in the axis's positive direction?")]
            public bool isPositiveHit;
        }

        public void Reset()
        {
            x.isNegativeHit = x.isPositiveHit = z.isNegativeHit = z.isPositiveHit = y.isNegativeHit = y.isPositiveHit = false;
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

        _gravity = (2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
        _maxJumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * _minJumpHeight);


        switch (_dashEasing)
        {
            case ForceEasing.Linear:
                //_dashForce = (_dashDistance * 2f) / (_dashDuration * Time.fixedDeltaTime + _dashDuration);
                _dashForce = (_dashDistance * 2f) / (_dashDuration + Time.fixedDeltaTime);
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
    }

    private void FixedUpdate()
    {
        //float accelerationTime = _collisionData.y.isNegativeHit ? _accelerationTimeGrounded : _accelerationTimeAirborne;

        //_horizontalTargetVelocity += new Vector2(_forceVelocity.x, _forceVelocity.z);
        //_velocity += _forceVelocity;

        //_velocity.x = Mathf.SmoothDamp(_velocity.x, _horizontalTargetVelocity.x, ref _horizontalVelocitySmoothing.x, accelerationTime);
        //_velocity.z = Mathf.SmoothDamp(_velocity.z, _horizontalTargetVelocity.y, ref _horizontalVelocitySmoothing.y, accelerationTime);

        switch (_fighter.FacingDirection)
        {
            case Fighter.Direction.Left:
                if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
                {
                    _fighter.FlipCharacter(Fighter.Direction.Right);
                }
                break;
            case Fighter.Direction.Right:
                if (_fighter.OpposingFighter.transform.position.x < transform.position.x)
                {
                    _fighter.FlipCharacter(Fighter.Direction.Left);
                }
                break;
        }

        if (_isGravityApplied)
        {
            _unforcedVelocity.y -= _gravity * Time.fixedDeltaTime;
        }
        _velocity = _unforcedVelocity + _forceVelocity + _overlapResolutionVelocity;
        Move(_velocity * Time.fixedDeltaTime);
        if (_collisionData.y.isNegativeHit || _collisionData.y.isPositiveHit)
        {
            ResetVelocityY();
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
        //TODO: kill all the player's force when they land
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

    public IEnumerator DisableGravity(float duration)
    {
        _isGravityApplied = false;
        yield return new WaitForSeconds(duration);

        _isGravityApplied = true;
        yield break;
    }

    public IEnumerator DisableGravity(Func<bool> enableCondition)
    {
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
        StartCoroutine(ApplyForce(direction, magnitude, duration));
        yield break;
    }

    public IEnumerator ApplyForce(Vector3 direction, float magnitude, float duration, ForceEasing easingFunction = ForceEasing.Linear)
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
                if (_collisionData.z.isNegativeHit || _collisionData.z.isPositiveHit)
                {
                    ResetVelocityY();
                    if (_wallBounceDistance != 0f)
                    {
                        Vector3 bounceDirection = Mathf.Sign(direction.z) == 1f ? _wallBounceDirection : new Vector3(_wallBounceDirection.x, _wallBounceDirection.y, -_wallBounceDirection.z);
                        float bounceMagnitude = (_wallBounceDistance * 2f) / (_wallBounceDuration + Time.fixedDeltaTime);
                        StartCoroutine(WallBounce(bounceDirection, bounceMagnitude, _wallBounceDuration));
                        yield break;
                    }
                    direction.z *= -1f;
                }
            }
            float forceMagnitude = function.Ease(magnitude, 0f, timer / duration);
            Vector3 force = direction * forceMagnitude;
            _forceVelocity += force;
            yield return new WaitForFixedUpdate();

            _forceVelocity -= force;
            timer += Time.fixedDeltaTime;
        }

        if (!_inputManager.Actions["Move"].isBeingPerformed)
        {
            _unforcedVelocity.x = 0f;
            _unforcedVelocity.z = 0f;
        }
        else
        {
            Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
            _unforcedVelocity.x = inputVector.x;
            //_unforcedVelocity.z = inputVector.y;
        }
        yield break;
    }

    private void AssignComponents()
    {
        _fighter = GetComponent<Fighter>();
        _playerInput = GetComponent<PlayerInput>();
        BoxCollider = GetComponent<BoxCollider>();
        _inputManager = GetComponent<InputManager>();
    }

    private void SubscribeActions()
    {
        _inputManager.Actions["Move"].perform += StartMoving;
        _inputManager.Actions["Move"].stop += StopMoving;
        _inputManager.Actions["Dash"].perform += Dash;
        _inputManager.Actions["Jump"].perform += Jump;
        _inputManager.Actions["Jump"].stop += StopJumping;
    }

    private void UnsubscribeActions()
    {
        _inputManager.Actions["Move"].perform -= StartMoving;
        _inputManager.Actions["Move"].stop -= StopMoving;
        _inputManager.Actions["Dash"].perform -= Dash;
        _inputManager.Actions["Jump"].perform -= Jump;
        _inputManager.Actions["Jump"].stop -= StopJumping;
    }

    private void SpaceRays()
    {
        Bounds bounds = BoxCollider.bounds;
        Bounds internalBounds = bounds;
        internalBounds.Expand(_skinWidth * -2f);

        _xAxisRayCount = Mathf.Clamp(_xAxisRayCount, 2, int.MaxValue);
        _zAxisRayCount = Mathf.Clamp(_zAxisRayCount, 2, int.MaxValue);
        _yAxisRayCount = Mathf.Clamp(_yAxisRayCount, 2, int.MaxValue);

        _xAxisRaySpacing.x = internalBounds.size.z / (_xAxisRayCount - 1);
        _xAxisRaySpacing.y = internalBounds.size.y / (_xAxisRayCount - 1);
        _zAxisRaySpacing.x = internalBounds.size.x / (_zAxisRayCount - 1);
        _zAxisRaySpacing.y = internalBounds.size.y / (_zAxisRayCount - 1);
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
        if (velocity.z != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionData.z, Axis.z);
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

        _raycastOrigins.x.negative = new Vector3(internalBounds.min.x, bounds.min.y, bounds.min.z);
        _raycastOrigins.x.positive = new Vector3(internalBounds.max.x, bounds.min.y, bounds.min.z);
        _raycastOrigins.z.negative = new Vector3(bounds.min.x, bounds.min.y, internalBounds.min.z);
        _raycastOrigins.z.positive = new Vector3(bounds.min.x, bounds.min.y, internalBounds.max.z);
        _raycastOrigins.y.negative = new Vector3(internalBounds.min.x, internalBounds.min.y, internalBounds.min.z);
        _raycastOrigins.y.positive = new Vector3(internalBounds.min.x, internalBounds.max.y, internalBounds.min.z);
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
                break;
            case Axis.z:
                axisVelocity = velocity.z;
                jOffset = velocity.x;
                rayCount = _zAxisRayCount;
                raySpacing = _zAxisRaySpacing;
                originAxis = _raycastOrigins.z;
                iDirection = Vector3.up;
                jDirection = Vector3.right;
                rayDirection = Vector3.forward;
                collisionAxis = _collisionData.z;
                break;
            case Axis.y:
                axisVelocity = velocity.y;
                iOffset = velocity.z;
                jOffset = velocity.x;
                rayCount = _yAxisRayCount;
                raySpacing = _yAxisRaySpacing;
                originAxis = _raycastOrigins.y;
                iDirection = Vector3.forward;
                jDirection = Vector3.right;
                rayDirection = Vector3.up;
                collisionAxis = _collisionData.y;
                break;
            default:
                throw new System.Exception("Invalid axis provided.");
        }

        float axisDirection = Mathf.Sign(axisVelocity);

        //if (axis == Axis.x)
        //{
        //    if (axisDirection == -1f && _fighter.FacingDirection == Fighter.Direction.Right)
        //    {
        //        _fighter.FlipCharacter(Fighter.Direction.Left);
        //    }
        //    if (axisDirection == 1f && _fighter.FacingDirection == Fighter.Direction.Left)
        //    {
        //        _fighter.FlipCharacter(Fighter.Direction.Right);
        //    }
        //}

        float rayLength = Mathf.Abs(axisVelocity) + _skinWidth;

        if (axis == Axis.y)
        {
            RemoveCollisionLayers(9);
        }

        for (int i = 0; i < rayCount; i++)
        {
            for (int j = 0; j < rayCount; j++)
            {
                Vector3 rayOrigin = axisDirection == -1f ? originAxis.negative : originAxis.positive;
                rayOrigin += iDirection * (raySpacing.y * i + iOffset) + jDirection * (raySpacing.x * j + jOffset);

                if (axis == Axis.y)
                {

                }

                if (Physics.Raycast(rayOrigin, rayDirection * axisDirection, out RaycastHit hit, rayLength, _collisionMask))
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

                if (axis == Axis.y)
                {
                    if(Physics.Raycast(rayOrigin, rayDirection * axisDirection, out RaycastHit overlapHit, rayLength, _playerMask))
                    {
                        if (!_isResolvingOverlap)
                        {
                            StartCoroutine(ResolveOverlap());
                            StartCoroutine(_fighter.OpposingFighter.MovementController.ResolveOverlap());
                        }
                    }
                }

                if (_drawDebugRays)
                {
                    Debug.DrawRay(rayOrigin, axisDirection * rayLength * rayDirection, Color.red);
                }
            }
        }

        if (axis == Axis.y)
        {
            AddCollisionLayers(9);
        }
    }

    public IEnumerator ResolveOverlap()
    {
        _isResolvingOverlap = true;
        bool opponentIsRight = false;
        if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
        {
            opponentIsRight = true;
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

    private void StartMoving(InputManager.Action action)
    {
        Vector2 inputVector = action.inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
        _unforcedVelocity.x = inputVector.x;
        //_unforcedVelocity.z = inputVector.y;
    }

    private void StopMoving(InputManager.Action action)
    {
        _unforcedVelocity.x = 0f;
        _unforcedVelocity.z = 0f;
    }

    public void Push(float duration)
    {
        float magnitude = (pushDistance * 2f) / (duration + Time.fixedDeltaTime);
        Vector3 direction = _fighter.FacingDirection == Fighter.Direction.Right ? pushDirection : new Vector3(-pushDirection.x, pushDirection.y, pushDirection.z);
        StartCoroutine(ApplyForce(direction, magnitude, duration));
    }

    private void Dash(InputManager.Action action)
    {
        //TODO: end the dash if player hits an obstacle
        if (_inputManager.Actions["Move"].isBeingPerformed)
        {
            Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<Vector2>().normalized;
            StartCoroutine(ApplyForce(new Vector3(_unforcedVelocity.x, 0f, _unforcedVelocity.z), _dashForce, _dashDuration, _dashEasing));
            if (_dashToZero)
            {
                _unforcedVelocity.x = 0f;
                _unforcedVelocity.z = 0f;
            }
        }
        else
        {
            StartCoroutine(ApplyForce(_fighter.FacingDirection == Fighter.Direction.Left ? Vector3.left : Vector3.right, _dashForce, _dashDuration, _dashEasing));
        }
        if (!CollisionData.y.isNegativeHit)
        {
            StartCoroutine(DisableGravity(_dashDuration));
            ResetVelocityY();
        }
        StartCoroutine(_inputManager.Disable(_dashDuration, _inputManager.Actions["Move"]));
        StartCoroutine(_inputManager.Disable(_dashDuration, _inputManager.Actions["Dash"]));
        if (_fighter.FacingDirection == Fighter.Direction.Right)
        {
            if (_fighter.OpposingFighter.transform.position.x > transform.position.x)
            {
                if(transform.position.x + _dashDistance > _fighter.OpposingFighter.transform.position.x + _fighter.OpposingFighter.MovementController.BoxCollider.bounds.size.x)
                {
                    StartCoroutine(DisableCollisionLayers(_dashDuration, 9));
                }
            }
        }
        if (_fighter.FacingDirection == Fighter.Direction.Left)
        {
            if (_fighter.OpposingFighter.transform.position.x < transform.position.x)
            {
                if (transform.position.x - _dashDistance < _fighter.OpposingFighter.transform.position.x - _fighter.OpposingFighter.MovementController.BoxCollider.bounds.size.x)
                {
                    StartCoroutine(DisableCollisionLayers(_dashDuration, 9));
                }
            }
        }
        StartCoroutine(Dash(action, _dashDuration));
    }

    private void Jump(InputManager.Action action)
    {
        if (_collisionData.y.isNegativeHit)
        {
            _unforcedVelocity.y = _maxJumpVelocity;
            StartCoroutine(_inputManager.Disable(() => _collisionData.y.isNegativeHit, _inputManager.Actions["Move"]));
        }
    }

    private void StopJumping(InputManager.Action action)
    {
        if (_unforcedVelocity.y > _minJumpVelocity)
        {
            _unforcedVelocity.y = _minJumpVelocity;
        }
    }

    private IEnumerator Dash(InputManager.Action action, float duration)
    {
        yield return new WaitForSeconds(duration);
        _inputManager.Actions["Dash"].finish?.Invoke(action);
        StartCoroutine(_inputManager.Disable(_dashCooldownDuration, _inputManager.Actions["Dash"]));
        yield break;
    }
    
    public void EnableAttackStop()
    {
        _isAttacking = true;
        //_inputManager.StopMove();
        InputManager.Action[] actions =
        {
            _inputManager.Actions["Move"], 
            _inputManager.Actions["Dash"],
            _inputManager.Actions["Jump"]
        };
        StartCoroutine(_inputManager.Disable(() => _isAttacking == false, actions));
    }
    
    public void DisableAttackStop()
    {
        _isAttacking = false;
    }

    private IEnumerator DisableCollisionLayers(float duration, params int[] layers)
    {
        foreach(int layer in layers)
        {
            _collisionMask &= ~(1 << layer);
        }
        yield return new WaitForSeconds(duration);

        foreach (int layer in layers)
        {
            _collisionMask |= (1 << layer);
        }
        yield break;
    }

    private void RemoveCollisionLayers(params int[] layers)
    {
        foreach (int layer in layers)
        {
            _collisionMask &= ~(1 << layer);
        }
    }

    private void AddCollisionLayers(params int[] layers)
    {
        foreach (int layer in layers)
        {
            _collisionMask |= (1 << layer);
        }
    }
}
