using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsManager : MonoBehaviour
{
    [SerializeField] private float _overlapResolutionSpeed;
    public LayerMask fightersMask;
    [SerializeField] private LayerMask _terrainMask;
    public OverlapDetector[] fighterDetectors = new OverlapDetector[2];
    //[SerializeField] private OverlapDetector[] _fighterDetectors;
    [SerializeField] private OverlapDetector[] _terrainDetecors;
    [SerializeField] private OverlapDetector _groundDetector;
    public bool DrawDebugRays { get => _drawDebugRays; }
    [SerializeField] private bool _drawDebugRays;
    private List<OverlapInfo> _overlapInfos = new();
    private List<OverlapDetector> _nonOverlappedDetectors = new();
    //private OverlapDetector[] _allOverlapDetectors;

    private void Awake()
    {
        Services.CollisionsManager = this;
    }

    void Start()
    {
        //for (int i = 0; i < fighterDetectors.Length; i++)
        //{
        //    fighterDetectors[i].Initialize(this);
        //}
        for (int i = 0; i < _terrainDetecors.Length; i++)
        {
            _terrainDetecors[i].Initialize(this);
        }
        //_allOverlapDetectors = FindObjectsOfType<OverlapDetector>();
    }

    private void FixedUpdate()
    {
        _nonOverlappedDetectors = new(fighterDetectors);
        for (int i = 0; i < fighterDetectors.Length; i++)
        {
            fighterDetectors[i].CheckOverlaps(fightersMask);
        }
        for (int i = 0; i < _terrainDetecors.Length; i++)
        {
            _terrainDetecors[i].CheckOverlaps(_terrainMask);
        }
        //RemoveDoubleOverlapInfos();
        OverlapInfo[] overlapInfos = new List<OverlapInfo>(_overlapInfos).ToArray();
        _overlapInfos.Clear();
        foreach (OverlapInfo overlapInfo in overlapInfos)
        {
            //if (overlapInfo.detector == _groundDetector && Array.Exists(fighterDetectors, x => x == overlapInfo.other))
            //{
            //    overlapInfo.other.MovementController.ResetToStartingY();
            //    overlapInfo.other.MovementController.overlapResolutionVelocity = Vector3.zero;
            //    continue;
            //}
            if (!overlapInfo.other.IsMoveable)
            {
                continue;
            }
            if (_nonOverlappedDetectors.Contains(overlapInfo.other))
            {
                _nonOverlappedDetectors.Remove(overlapInfo.other);
            }
            Vector3 overlapResolutionVelocity = Vector3.zero;
            Debug.Log(overlapInfo.otherPushDirection);
            switch (overlapInfo.otherPushDirection)
            {
                case OverlapInfo.Direction.Up:
                    overlapResolutionVelocity = Vector3.up;
                    break;
                case OverlapInfo.Direction.Down:
                    overlapResolutionVelocity = Vector3.down;
                    break;
                case OverlapInfo.Direction.Left:
                    overlapResolutionVelocity = Vector3.left;
                    break;
                case OverlapInfo.Direction.Right:
                    overlapResolutionVelocity = Vector3.right;
                    Debug.Log("penis");
                    break;
            }
            overlapResolutionVelocity *= _overlapResolutionSpeed;
            overlapInfo.other.MovementController.overlapResolutionVelocity = overlapResolutionVelocity;
        }
        for (int i = 0; i < _nonOverlappedDetectors.Count; i++)
        {
            if (_nonOverlappedDetectors[i].MovementController.overlapResolutionVelocity != Vector3.zero)
            {
                _nonOverlappedDetectors[i].MovementController.overlapResolutionVelocity = Vector3.zero;
            }
        }
    }

    public void AddOverlapInfo(OverlapInfo overlapInfo)
    {
        if (_overlapInfos.Exists(x => x.detector == overlapInfo.detector && x.other == overlapInfo.other))
        {
            return;
        }
        _overlapInfos.Add(overlapInfo);
    }

    private void RemoveDoubleOverlapInfos()
    {
        List<OverlapInfo> temp = new(_overlapInfos);
        int count = _overlapInfos.Count;
        for (int i = 0; i < count; i++)
        {
            //_overlapInfos[i]
        }
    }
}
