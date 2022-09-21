using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WesleyDavies;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(PlayerInput))]
public class RaycastController : MonoBehaviour
{
    [Tooltip("How fast should the character move sideways (in m/s)?")]
    [SerializeField] private float _moveSpeed;
    //[Tooltip("How high should the character jump?")]
    //[SerializeField] private float _maxJumpHeight;
    //[SerializeField] private float _minJumpHeight;
    //[Tooltip("How long should the character jump?")]
    //[SerializeField] private float _timeToJumpApex;
    [Tooltip("How fast is gravity (in m/s)?")]
    [SerializeField] private float _gravity;
    [Tooltip("How many rays should be shot out horizontally?")]
    [SerializeField] private int _xAxisRayCount;
    [Tooltip("How many rays should be shot out forward?")]
    [SerializeField] private int _zAxisRayCount;
    [Tooltip("How many rays should be shot out vertically?")]
    [SerializeField] private int _yAxisRayCount;
    [Tooltip("What layer(s) should collisions be checked on?")]
    [SerializeField] private LayerMask _collisionMask;

    private BoxCollider _boxCollider;
    private PlayerInput _playerInput;

    private Vector3 _velocity;
    //private float _maxJumpVelocity;
    //private float _minJumpVelocity;

    private float _xAxisRaySpacing;
    private float _yAxisRaySpacing;
    private float _zAxisRaySpacing;

    private RaycastOrigins _raycastOrigins;
    private CollisionInfo _collisionInfo;
    //private bool _isJumping = false;

    private readonly float _skinWidth = 0.1f;

    //[SerializeField] private float _accelerationTimeAirborne = .2f;
    //[SerializeField] private float _accelerationTimeGrounded = .1f;
    //private float _velocityXSmoothing;
    //private float _targetVelocityX;

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
            public bool isNegativeHit;
            public bool isPositiveHit;
        }

        public void Reset()
        {
            x.isNegativeHit = x.isPositiveHit = z.isNegativeHit = z.isPositiveHit = y.isNegativeHit = y.isPositiveHit = false;
        }
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        SpaceRays();
        _playerInput.onActionTriggered += ResolveActions;
    }

    private void FixedUpdate()
    {
        if (_collisionInfo.y.isNegativeHit || _collisionInfo.y.isPositiveHit)
        {
            _velocity.y = 0f;
        }
        _velocity.y -= _gravity * Time.fixedDeltaTime;
        Move(_velocity * Time.fixedDeltaTime);
    }

    private void OnDestroy()
    {
        _playerInput.onActionTriggered -= ResolveActions;
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
        _collisionInfo.Reset();

        if (velocity.x != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionInfo.x, Axis.x);
        }
        if (velocity.z != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionInfo.z, Axis.z);
        }
        if (velocity.y != 0)
        {
            CalculateCollisions(ref velocity, ref _collisionInfo.y, Axis.y);
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
                collisionAxis = _collisionInfo.x;
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
                collisionAxis = _collisionInfo.z;
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
                collisionAxis = _collisionInfo.y;
                break;
            default:
                throw new System.Exception("Invalid axis provided.");
        }

        Debug.Log(rayCount);

        float axisDirection = Mathf.Sign(axisVelocity);
        float rayLength = Mathf.Abs(axisVelocity) + _skinWidth;

        for (int i = 0; i < rayCount; i++)
        {
            for (int j = 0; j < rayCount; j++)
            {
                Vector3 rayOrigin = axisDirection == -1 ? originAxis.negative : originAxis.positive;
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

                    collisionAxis.isNegativeHit = axisDirection == -1;
                    collisionAxis.isPositiveHit = axisDirection == 1;
                }

                Debug.DrawRay(rayOrigin, axisDirection * rayLength * rayDirection, Color.red);
            }
        }
    }

    private void ResolveActions(InputAction.CallbackContext context)
    {
        if (context.action == _playerInput.actions["Move"])
        {
            if (context.action.WasPerformedThisFrame())
            {
                //_targetVelocityX = Convert.ToInt16(context.action.ReadValue<Vector2>().x) * _moveSpeed;
                Vector2 inputVector = context.action.ReadValue<Vector2>().normalized;
                _velocity.x = inputVector.x * _moveSpeed;
                _velocity.z = inputVector.y * _moveSpeed;
            }
            else
            {
                //_targetVelocityX = 0f;
                _velocity.x = 0f;
                _velocity.z = 0f;
            }
        }
    }
}
