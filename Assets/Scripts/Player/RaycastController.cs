using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WesleyDavies;
using static RaycastController;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(InputManager))]
public class RaycastController : MonoBehaviour
{
    [Tooltip("How fast should the character move sideways (in m/s)?")]
    [SerializeField] private float _moveSpeed;
    [Tooltip("How high should the character jump on longest button press?")]
    [SerializeField] private float _maxJumpHeight;
    [Tooltip("How high should the character jump on shortest button press?")]
    [SerializeField] private float _minJumpHeight;
    [Tooltip("How long should the character jump?")]
    [SerializeField] private float _timeToJumpApex;
    [Tooltip("What layer(s) should collisions be checked on?")]
    [SerializeField] private LayerMask _collisionMask;
    [Tooltip("Should debug rays be drawn?")]
    [SerializeField] private bool _drawDebugRays;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashDuration;

    private Character _character;
    private BoxCollider _boxCollider;
    private PlayerInput _playerInput;
    private InputManager _inputManager;

    private Vector3 _velocity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private float _gravity;
    [Tooltip("How many rays should be shot out horizontally?")]
    private int _xAxisRayCount = 3;
    [Tooltip("How many rays should be shot out forward?")]
    private int _zAxisRayCount = 3;
    [Tooltip("How many rays should be shot out vertically?")]
    private int _yAxisRayCount = 3;
    private float _xAxisRaySpacing;
    private float _yAxisRaySpacing;
    private float _zAxisRaySpacing;

    private RaycastOrigins _raycastOrigins;
    public CollisionInfo CollisionData
    {
        get => _collisionData;
        private set => _collisionData = value;
    }
    private CollisionInfo _collisionData;

    private readonly float _skinWidth = 0.1f;

    [SerializeField] private float _accelerationTimeAirborne = .2f;
    [SerializeField] private float _accelerationTimeGrounded = .1f;
    [SerializeField] private float _decelerationTimeDashing = .1f;
    private Vector2 _horizontalVelocitySmoothing;
    private Vector2 _horizontalTargetVelocity;

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
    }

    private void FixedUpdate()
    {
        float accelerationTime = _collisionData.y.isNegativeHit ? _accelerationTimeGrounded : _accelerationTimeAirborne;
        if (_inputManager.Actions["Dash"].isBeingPerformed)
        {
            accelerationTime = _decelerationTimeDashing;
        }
        _velocity.x = Mathf.SmoothDamp(_velocity.x, _horizontalTargetVelocity.x, ref _horizontalVelocitySmoothing.x, accelerationTime);
        _velocity.z = Mathf.SmoothDamp(_velocity.z, _horizontalTargetVelocity.y, ref _horizontalVelocitySmoothing.y, accelerationTime);
        _velocity.y -= _gravity * Time.fixedDeltaTime;
        Move(_velocity * Time.fixedDeltaTime);
        if (_collisionData.y.isNegativeHit || _collisionData.y.isPositiveHit)
        {
            _velocity.y = 0f;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
    }

    private void AssignComponents()
    {
        _character = GetComponent<Character>();
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

        _xAxisRaySpacing = bounds.size.y / (_xAxisRayCount - 1);
        _zAxisRaySpacing = bounds.size.z / (_zAxisRayCount - 1);
        _yAxisRaySpacing = bounds.size.x / (_yAxisRayCount - 1);
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
        float raySpacing;
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
            if (axisDirection == -1f && _character.FacingDirection == Character.Direction.Right)
            {
                _character.FlipCharacter(Character.Direction.Left);
            }
            if (axisDirection == 1f && _character.FacingDirection == Character.Direction.Left)
            {
                _character.FlipCharacter(Character.Direction.Right);
            }
        }

        float rayLength = Mathf.Abs(axisVelocity) + _skinWidth;

        for (int i = 0; i < rayCount; i++)
        {
            for (int j = 0; j < rayCount; j++)
            {
                Vector3 rayOrigin = axisDirection == -1f ? originAxis.negative : originAxis.positive;
                rayOrigin += iDirection * (raySpacing * i + iOffset) + jDirection * (raySpacing * j + jOffset);

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
        _horizontalTargetVelocity = action.inputAction.ReadValue<Vector2>().normalized * _moveSpeed;
    }

    private void StopMoving(InputManager.Action action)
    {
        _horizontalTargetVelocity = Vector2.zero;
    }

    private void Dash(InputManager.Action action)
    {
        if (_inputManager.Actions["Move"].isBeingPerformed)
        {
            StartCoroutine(ApplyForce(_inputManager.Actions["Move"].inputAction.ReadValue<Vector2>().normalized, _dashForce, _dashDuration, _inputManager.Actions["Move"]));
        }
        else
        {
            StartCoroutine(ApplyForce(_character.FacingDirection == Character.Direction.Left ? Vector3.left : Vector3.right, _dashForce, _dashDuration, _inputManager.Actions["Move"]));
        }
    }

    private void Jump(InputManager.Action action)
    {
        if (_collisionData.y.isNegativeHit)
        {
            _velocity.y = _maxJumpVelocity;
        }
    }

    private void StopJumping(InputManager.Action action)
    {
        if (_velocity.y > _minJumpVelocity)
        {
            _velocity.y = _minJumpVelocity;
        }
    }

    private IEnumerator ApplyForce(Vector3 direction, float magnitude, float timeUntilActionsEnabled, params InputManager.Action[] actionsToDisable)
    {
        foreach(InputManager.Action action in actionsToDisable)
        {
            action.disabledCount++;
        }
        //direction.Normalize();
        Vector3 force = direction * magnitude;
        _horizontalTargetVelocity = direction * _moveSpeed;
        _velocity.x = force.x;
        _velocity.y = force.y;
        _velocity.z = force.z;
        //_horizontalTargetVelocity.x = force.x;
        //_horizontalTargetVelocity.y = force.z;
        yield return new WaitForSeconds(timeUntilActionsEnabled);

        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount--;
        }
        if (!_inputManager.Actions["Move"].isBeingPerformed)
        {
            _horizontalTargetVelocity = Vector2.zero;
        }
        yield break;
    }
}
