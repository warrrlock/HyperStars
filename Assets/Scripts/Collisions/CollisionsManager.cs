using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionsManager : MonoBehaviour
{
    [SerializeField] private float _overlapResolutionSpeed;
    public LayerMask fightersMask;
    public LayerMask terrainMask;
    [NonSerialized] public OverlapDetector[] fighterDetectors = new OverlapDetector[2];
    //[SerializeField] private OverlapDetector[] _fighterDetectors;
    //[SerializeField] private OverlapDetector[] _terrainDetecors;
    //[SerializeField] private OverlapDetector _groundDetector;
    public bool DrawDebugRays { get => _drawDebugRays; }
    [SerializeField] private bool _drawDebugRays;
    private List<OverlapInfo> _overlapInfos = new();
    private List<OverlapDetector> _nonOverlappedDetectors;
    private OverlapDetector[] _allOverlapDetectors;

    private void Awake()
    {
        Services.CollisionsManager = this;
        //fighterDetectors = new OverlapDetector[2];
        //DontDestroyOnLoad(this);
    }

    void Start()
    {
        //for (int i = 0; i < fighterDetectors.Length; i++)
        //{
        //    fighterDetectors[i].Initialize(this);
        //}
        //for (int i = 0; i < _terrainDetecors.Length; i++)
        //{
        //    _terrainDetecors[i].Initialize(this);
        //}
        //_groundDetector.Initialize(this);
        _allOverlapDetectors = FindObjectsOfType<OverlapDetector>();
        //for (int i = 0; i < _allOverlapDetectors.Length; i++)
        //{
        //    _allOverlapDetectors[i].Initialize(this);
        //}
    }

    private void FixedUpdate()
    {
        _nonOverlappedDetectors = new(fighterDetectors);
        for (int i = 0; i < _allOverlapDetectors.Length; i++)
        {
            _allOverlapDetectors[i].CheckOverlaps();
        }
        //for (int i = 0; i < fighterDetectors.Length; i++)
        //{
        //    fighterDetectors[i].CheckOverlaps(fightersMask);
        //}
        //for (int i = 0; i < _terrainDetecors.Length; i++)
        //{
        //    _terrainDetecors[i].CheckOverlaps(terrainMask);
        //}
        //_groundDetector.CheckOverlaps(terrainMask);
        //RemoveDoubleOverlapInfos();
        OverlapInfo[] overlapInfos = new List<OverlapInfo>(_overlapInfos).ToArray(); //to prevent the list from being changed while we iterate through it
        _overlapInfos.Clear();
        foreach (OverlapInfo overlapInfo in overlapInfos)
        {
            if (overlapInfo.detector.TerrainType == OverlapDetector.Terrain.Ground && overlapInfo.other.TerrainType != OverlapDetector.Terrain.Wall)
            {
                overlapInfo.other.MovementController.ResetToStartingY();
                overlapInfo.other.MovementController.overlapResolutionVelocity = Vector3.zero;
                continue;
            }
            if (!overlapInfo.other.IsMoveable)
            {
                continue;
            }
            if (overlapInfo.detector.TerrainType == OverlapDetector.Terrain.Wall)
            {
                if (overlapInfo.other.TerrainType == OverlapDetector.Terrain.Wall)
                {
                    continue;
                }
                if (overlapInfo.other.TerrainType == OverlapDetector.Terrain.Ground)
                {
                    continue;
                }
                float newX = overlapInfo.otherPushDirection == OverlapInfo.Direction.Right ?
                    overlapInfo.detector.EdgeX + overlapInfo.other.HalfWidth + overlapInfo.detector.SkinWidth * 3f :
                    overlapInfo.detector.EdgeX - overlapInfo.other.HalfWidth - overlapInfo.detector.SkinWidth * 3f; //TODO: magic number
                overlapInfo.other.MovementController.MoveToX(newX);
                continue;
            }
            if (_nonOverlappedDetectors.Contains(overlapInfo.other))
            {
                _nonOverlappedDetectors.Remove(overlapInfo.other);
            }
            Vector3 overlapResolutionVelocity = Vector3.zero;
            //Debug.Log(overlapInfo.otherPushDirection);
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
                    break;
            }
            overlapResolutionVelocity *= _overlapResolutionSpeed;
            overlapInfo.other.MovementController.overlapResolutionVelocity = overlapResolutionVelocity;
        }
        int count = _nonOverlappedDetectors.Count;
        for (int i = 0; i < count; i++)
        {
            if (_nonOverlappedDetectors[i] == null)
            {
                continue;
            }
            //Debug.Log(_nonOverlappedDetectors.Count);
            //Debug.Log(Time.time);
            //Debug.Log(i);
            //Debug.Log(_nonOverlappedDetectors[i]);
            if (_nonOverlappedDetectors[i].MovementController.overlapResolutionVelocity != Vector3.zero)
            {
                _nonOverlappedDetectors[i].MovementController.overlapResolutionVelocity = Vector3.zero;
            }
        }
        //_nonOverlappedDetectors.Clear();
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
