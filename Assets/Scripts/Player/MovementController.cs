using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WesleyDavies;

[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(InputManager))]
public class MovementController : MonoBehaviour
{
    public enum ForceEasing { Linear, Quadratic, Cubic }

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
    //[SerializeField] private float _movementDisableDuration;
    [SerializeField] private float _dashCooldownDuration;
    [SerializeField] private ForceEasing _dashEasing;

    [Header("Collisions")]
    [Tooltip("What layer(s) should collisions be checked on?")]
    [SerializeField] private LayerMask _collisionMask;
    [Tooltip("How many rays should be shot out horizontally?")]
    private int _xAxisRayCount = 3;
    [Tooltip("How many rays should be shot out forward?")]
    private int _zAxisRayCount = 3;
    [Tooltip("How many rays should be shot out vertically?")]
    private int _yAxisRayCount = 3;
    [Tooltip("Should debug rays be drawn?")]
    [SerializeField] private bool _drawDebugRays;

    private Fighter _fighter;
    private BoxCollider _boxCollider;
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

    private RaycastOrigins _raycastOrigins;
    public CollisionInfo CollisionData
    {
        get => _collisionData;
        private set => _collisionData = value;
    }
    private CollisionInfo _collisionData;

    private readonly float _skinWidth = 0.1f;

    private float _accelerationTimeAirborne = .2f;
    private float _accelerationTimeGrounded = .1f;
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



        //Easing function = Easing.CreateEasingFunc(Easing.Funcs.QuadraticOut);
        //float fiveTotal = 0f;
        //for (float i = 0; i < 10; i++)
        //{
        //    float number = function.Ease(4, 0, i / 10);
        //    Debug.Log(number);
        //    fiveTotal += number;
        //}
        //Debug.Log(fiveTotal);

        //float tenTotal = 0f;
        //for (float i = 0; i < 10; i++)
        //{
        //    float number = function.Ease(5, 0, i / 10);
        //    Debug.Log(number);
        //    tenTotal += number;
        //}
        //Debug.Log(tenTotal);

        switch (_dashEasing)
        {
            case ForceEasing.Linear:
                _dashForce = (_dashDistance * 2f) / (_dashDuration * Time.fixedDeltaTime + _dashDuration);
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
        float accelerationTime = _collisionData.y.isNegativeHit ? _accelerationTimeGrounded : _accelerationTimeAirborne;

        _horizontalTargetVelocity += new Vector2(_forceVelocity.x, _forceVelocity.z);
        //_velocity += _forceVelocity;

        //_velocity.x = Mathf.SmoothDamp(_velocity.x, _horizontalTargetVelocity.x, ref _horizontalVelocitySmoothing.x, accelerationTime);
        //_velocity.z = Mathf.SmoothDamp(_velocity.z, _horizontalTargetVelocity.y, ref _horizontalVelocitySmoothing.y, accelerationTime);
        _unforcedVelocity.y -= _gravity * Time.fixedDeltaTime;
        _velocity = _unforcedVelocity + _forceVelocity;
        //_velocity += _forceVelocity;
        Move(_velocity * Time.fixedDeltaTime);
        if (_collisionData.y.isNegativeHit || _collisionData.y.isPositiveHit)
        {
            _unforcedVelocity.y = 0f;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
    }

    private void AssignComponents()
    {
        _fighter = GetComponent<Fighter>();
        _playerInput = GetComponent<PlayerInput>();
        _boxCollider = GetComponent<BoxCollider>();
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
        Bounds bounds = _boxCollider.bounds;

        _xAxisRayCount = Mathf.Clamp(_xAxisRayCount, 2, int.MaxValue);
        _zAxisRayCount = Mathf.Clamp(_zAxisRayCount, 2, int.MaxValue);
        _yAxisRayCount = Mathf.Clamp(_yAxisRayCount, 2, int.MaxValue);

        _xAxisRaySpacing.x = bounds.size.z / (_xAxisRayCount - 1);
        _xAxisRaySpacing.y = bounds.size.y / (_xAxisRayCount - 1);
        _zAxisRaySpacing.x = bounds.size.x / (_zAxisRayCount - 1);
        _zAxisRaySpacing.y = bounds.size.y / (_zAxisRayCount - 1);
        _yAxisRaySpacing.x = bounds.size.x / (_yAxisRayCount - 1);
        _yAxisRaySpacing.y = bounds.size.z / (_yAxisRayCount - 1);
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
        Bounds bounds = _boxCollider.bounds;
        Bounds internalBounds = bounds;
        internalBounds.Expand(_skinWidth * -2f);

        _raycastOrigins.x.negative = new Vector3(internalBounds.min.x, bounds.min.y, bounds.min.z);
        _raycastOrigins.x.positive = new Vector3(internalBounds.max.x, bounds.min.y, bounds.min.z);
        _raycastOrigins.z.negative = new Vector3(bounds.min.x, bounds.min.y, internalBounds.min.z);
        _raycastOrigins.z.positive = new Vector3(bounds.min.x, bounds.min.y, internalBounds.max.z);
        _raycastOrigins.y.negative = new Vector3(bounds.min.x, internalBounds.min.y, bounds.min.z);
        _raycastOrigins.y.positive = new Vector3(bounds.min.x, internalBounds.max.y, bounds.min.z);
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

        if (axis == Axis.x)
        {
            if (axisDirection == -1f && _fighter.FacingDirection == Fighter.Direction.Right)
            {
                _fighter.FlipCharacter(Fighter.Direction.Left);
            }
            if (axisDirection == 1f && _fighter.FacingDirection == Fighter.Direction.Left)
            {
                _fighter.FlipCharacter(Fighter.Direction.Right);
            }
        }

        float rayLength = Mathf.Abs(axisVelocity) + _skinWidth;

        for (int i = 0; i < rayCount; i++)
        {
            for (int j = 0; j < rayCount; j++)
            {
                Vector3 rayOrigin = axisDirection == -1f ? originAxis.negative : originAxis.positive;
                rayOrigin += iDirection * (raySpacing.y * i + iOffset) + jDirection * (raySpacing.x * j + jOffset);

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

                if (_drawDebugRays)
                {
                    Debug.DrawRay(rayOrigin, axisDirection * rayLength * rayDirection, Color.red);
                }
            }
        }
    }

    private void StartMoving(InputManager.Action action)
    {
        //_horizontalTargetVelocity = action.inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
        Vector2 inputVector = action.inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
        _unforcedVelocity.x = inputVector.x;
        _unforcedVelocity.z = inputVector.y;
    }

    private void StopMoving(InputManager.Action action)
    {
        //_horizontalTargetVelocity = Vector2.zero;
        _unforcedVelocity = Vector3.zero;
    }

    private void Dash(InputManager.Action action)
    {
        //TODO: end the dash if player hits an obstacle
        if (_inputManager.Actions["Move"].isBeingPerformed)
        {
            Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<Vector2>().normalized;
            StartCoroutine(ApplyForce(new Vector3(inputVector.x, 0f, inputVector.y), _dashForce, _dashDuration));
            StartCoroutine(_inputManager.Disable(_dashDuration, _inputManager.Actions["Move"]));
            if (_dashToZero)
            {
                _unforcedVelocity = Vector3.zero;
            }
        }
        else
        {
            StartCoroutine(ApplyForce(_fighter.FacingDirection == Fighter.Direction.Left ? Vector3.left : Vector3.right, _dashForce, _dashDuration));
            StartCoroutine(_inputManager.Disable(_dashDuration, _inputManager.Actions["Move"]));
        }
    }

    private void Jump(InputManager.Action action)
    {
        if (_collisionData.y.isNegativeHit)
        {
            //_velocity.y = _maxJumpVelocity;
            _unforcedVelocity.y = _maxJumpVelocity;
            StartCoroutine(_inputManager.Disable(() => _collisionData.y.isNegativeHit, _inputManager.Actions["Move"]));
        }
    }

    private void StopJumping(InputManager.Action action)
    {
        if (_unforcedVelocity.y > _minJumpVelocity)
        {
            //_velocity.y = _minJumpVelocity;
            _unforcedVelocity.y = _minJumpVelocity;
        }
    }

    public IEnumerator ApplyForce(Vector3 direction, float magnitude, float duration, ForceEasing easingFunction = ForceEasing.Linear)
    {
        //direction.Normalize();
        //Vector3 force = direction * magnitude;
        //_forceVelocity += force;
        //_horizontalTargetVelocity = new Vector2(force.x, force.z);
        //_velocity.x = force.x;
        //_velocity.y = force.y;
        //_velocity.z = force.z;
        //_horizontalTargetVelocity = direction * _moveSpeed;
        //_horizontalTargetVelocity.x = force.x;
        //_horizontalTargetVelocity.y = force.z;
        float timer = 0f;
        while (timer < duration)
        {
            float forceMagnitude = 0f;
            Easing function;

            switch (easingFunction)
            {
                case ForceEasing.Linear:
                    forceMagnitude = Mathf.Lerp(magnitude, 0f, timer / duration);
                    break;
                case ForceEasing.Quadratic:
                    function = Easing.CreateEasingFunc(Easing.Funcs.QuadraticOut);
                    forceMagnitude = function.Ease(magnitude, 0f, timer / duration);
                    break;
                case ForceEasing.Cubic:
                    function = Easing.CreateEasingFunc(Easing.Funcs.CubicOut);
                    forceMagnitude = function.Ease(magnitude, 0f, timer / duration);
                    break;
            }

            Vector3 force = direction * forceMagnitude;
            _forceVelocity += force;
            yield return new WaitForFixedUpdate();
            _forceVelocity -= force;
            timer += Time.fixedDeltaTime;
        }

        if (!_inputManager.Actions["Move"].isBeingPerformed)
        {
            //_horizontalTargetVelocity = Vector2.zero;
            _unforcedVelocity = Vector3.zero;
        }
        else
        {
            Vector2 inputVector = _inputManager.Actions["Move"].inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
            _unforcedVelocity.x = inputVector.x;
            _unforcedVelocity.z = inputVector.y;
        }
        StartCoroutine(_inputManager.Disable(_dashCooldownDuration, _inputManager.Actions["Dash"]));
        yield break;
    }
}
