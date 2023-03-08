using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OverlapInfo
{
    public enum Direction { Up, Down, Left, Right }

    public readonly OverlapDetector detector;
    public readonly OverlapDetector other;
    public readonly Direction otherPushDirection;

    public OverlapInfo(OverlapDetector detector, OverlapDetector other, Direction otherPushDirection)
    {
        this.detector = detector;
        this.other = other;
        this.otherPushDirection = otherPushDirection;
    }
}

[RequireComponent(typeof(BoxCollider))]
public class OverlapDetector : MonoBehaviour
{
    public MovementController MovementController { get => _movementController; }
    [SerializeField] private MovementController _movementController;
    public Vector2 ColliderSize { get; private set; }

    private enum Axis { Horizontal, Vertical }
    [SerializeField] private Axis _pushAxis;
    private enum TypePush { ClosestSide, Positive, Negative }
    [SerializeField] private TypePush _pushType;
    [SerializeField] private float _skinWidth;
    public bool IsMoveable { get => _isMoveable; }
    [SerializeField] private bool _isMoveable;
    [SerializeField] private int _rayCount = 3;

    private CollisionsManager _collisionsManager;
    private BoxCollider _boxCollider;
    private RaycastOrigins _raycastOrigins;
    private float _pushAxisLength;
    private float _axisRaySpacing;

    private struct RaycastOrigins
    {
        public Vector3 negative;
        public Vector3 positive;
    }

    private void Start()
    {
        if (TryGetComponent<Fighter>(out Fighter fighter))
        {
            Services.CollisionsManager.fighterDetectors[fighter.PlayerId] = this;
            Initialize(Services.CollisionsManager);
        }
    }

    public void Initialize(CollisionsManager collisionsManager)
    {
        AssignComponents();
        _collisionsManager = collisionsManager;
    }

    public void CheckOverlaps(LayerMask collisionMask)
    {
        UpdateRaycastOrigins();
        SpaceRays();

        //float rayLength = _pushAxisLength + _skinWidth;
        float rayLength = _pushAxisLength;

        for (int i = 0; i < 2; i++)
        {
            Vector3 rayDirection;
            Vector3 spacingDirection;
            if (_pushAxis == Axis.Horizontal)
            {
                rayDirection = i == 0 ? Vector3.right : Vector3.left;
                spacingDirection = Vector3.up;
            }
            else
            {
                rayDirection = i == 0 ? Vector3.up : Vector3.down;
                spacingDirection = Vector3.right;
            }
            for (int j = 0; j < _rayCount; j++)
            {
                Vector3 rayOrigin = i == 0 ? _raycastOrigins.negative : _raycastOrigins.positive;
                rayOrigin += spacingDirection;
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, rayLength, collisionMask);
                foreach(RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        continue;
                    }
                    if (!hit.collider.TryGetComponent<OverlapDetector>(out OverlapDetector other))
                    {
                        continue;
                    }
                    rayLength = hit.distance - _skinWidth;
                    float otherCenterDistance = _pushAxis == Axis.Horizontal ? rayLength + other.ColliderSize.x / 2f : rayLength + other.ColliderSize.y / 2f;
                    OverlapInfo.Direction pushDirection;
                    switch (_pushType)
                    {
                        case TypePush.Positive:
                            pushDirection = _pushAxis == Axis.Horizontal ? OverlapInfo.Direction.Right : OverlapInfo.Direction.Up;
                            break;
                        case TypePush.Negative:
                            pushDirection = _pushAxis == Axis.Horizontal ? OverlapInfo.Direction.Left : OverlapInfo.Direction.Down;
                            break;
                        case TypePush.ClosestSide:
                            if (_pushAxis == Axis.Horizontal)
                            {
                                if (otherCenterDistance > ColliderSize.x / 2f)
                                {
                                    pushDirection = i == 0 ? OverlapInfo.Direction.Right : OverlapInfo.Direction.Left;
                                }
                                else if (otherCenterDistance < ColliderSize.x / 2f)
                                {
                                    pushDirection = i == 0 ? OverlapInfo.Direction.Left : OverlapInfo.Direction.Right;
                                }
                                else
                                {
                                    //TODO: add more logic to this, probably a new push direction that chooses one object to move towards the center of the stage
                                    pushDirection = OverlapInfo.Direction.Left;
                                }
                            }
                            else
                            {
                                if (otherCenterDistance > ColliderSize.y / 2f)
                                {
                                    pushDirection = i == 0 ? OverlapInfo.Direction.Up : OverlapInfo.Direction.Down;
                                }
                                else if (otherCenterDistance < ColliderSize.y / 2f)
                                {
                                    pushDirection = i == 0 ? OverlapInfo.Direction.Down : OverlapInfo.Direction.Up;
                                }
                                else
                                {
                                    pushDirection = OverlapInfo.Direction.Up;
                                }
                            }
                            break;
                        default:
                            pushDirection = OverlapInfo.Direction.Up;
                            break;
                    }
                    OverlapInfo overlapInfo = new(this, other, pushDirection);
                    _collisionsManager.AddOverlapInfo(overlapInfo);
                    if (_collisionsManager.DrawDebugRays)
                    {
                        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.blue);
                    }
                    return;
                }

                //if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayLength, collisionMask))
                //{
                //    rayLength = hit.distance;
                //    OverlapDetector other = hit.collider.GetComponent<OverlapDetector>();
                //    float otherCenterDistance = _pushAxis == Axis.Horizontal ? rayLength + other.ColliderSize.x / 2f : rayLength + other.ColliderSize.y / 2f;
                //    OverlapInfo.Direction pushDirection;
                //    switch (_pushType)
                //    {
                //        case TypePush.Positive:
                //            pushDirection = _pushAxis == Axis.Horizontal ? OverlapInfo.Direction.Right : OverlapInfo.Direction.Up;
                //            break;
                //        case TypePush.Negative:
                //            pushDirection = _pushAxis == Axis.Horizontal ? OverlapInfo.Direction.Left : OverlapInfo.Direction.Down;
                //            break;
                //        case TypePush.ClosestSide:
                //            if (_pushAxis == Axis.Horizontal)
                //            {
                //                if (otherCenterDistance > ColliderSize.x / 2f)
                //                {
                //                    pushDirection = OverlapInfo.Direction.Right;
                //                }
                //                else if (otherCenterDistance < ColliderSize.x / 2f)
                //                {
                //                    pushDirection = OverlapInfo.Direction.Left;
                //                }
                //                else
                //                {
                //                    //TODO: add more logic to this, probably a new push direction that chooses one object to move towards the center of the stage
                //                    pushDirection = OverlapInfo.Direction.Left;
                //                }
                //            }
                //            else
                //            {
                //                if (otherCenterDistance > ColliderSize.y / 2f)
                //                {
                //                    pushDirection = OverlapInfo.Direction.Up;
                //                }
                //                else if (otherCenterDistance < ColliderSize.y / 2f)
                //                {
                //                    pushDirection = OverlapInfo.Direction.Down;
                //                }
                //                else
                //                {
                //                    pushDirection = OverlapInfo.Direction.Up;
                //                }
                //            }
                //            break;
                //        default:
                //            pushDirection = OverlapInfo.Direction.Up;
                //            break;
                //    }
                //    OverlapInfo overlapInfo = new(this, other, pushDirection);
                //    if (_collisionsManager.DrawDebugRays)
                //    {
                //        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.blue);
                //    }
                //    return;
                //}
            }
        }
    }

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = _boxCollider.bounds;
        Bounds externalBounds = bounds;
        externalBounds.Expand(_skinWidth * 2f);

        switch (_pushAxis)
        {
            case Axis.Horizontal:
                _pushAxisLength = bounds.size.x;
                _raycastOrigins.negative = new Vector3(externalBounds.min.x, bounds.min.y, bounds.center.z);
                _raycastOrigins.positive = new Vector3(externalBounds.max.x, bounds.min.y, bounds.center.z);
                break;
            case Axis.Vertical:
                _pushAxisLength = bounds.size.y;
                _raycastOrigins.negative = new Vector3(bounds.min.x, externalBounds.min.y, bounds.center.z);
                _raycastOrigins.positive = new Vector3(bounds.min.x, externalBounds.max.y, bounds.center.z);
                break;
        }
    }

    private void SpaceRays()
    {
        Bounds bounds = _boxCollider.bounds;
        Bounds internalBounds = bounds;
        internalBounds.Expand(_skinWidth * -2f);

        _rayCount = Mathf.Clamp(_rayCount, 2, int.MaxValue);

        switch (_pushAxis)
        {
            case Axis.Horizontal:
                _axisRaySpacing = internalBounds.size.y / (_rayCount - 1);
                break;
            case Axis.Vertical:
                _axisRaySpacing = internalBounds.size.x / (_rayCount - 1);
                break;
        }
    }

    private void AssignComponents()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }
}
