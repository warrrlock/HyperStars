using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CameraController))]
public class CameraManager : MonoBehaviour
{
    //[SerializeField] private float _minCameraSize;
    //[SerializeField] private float _maxCameraSize;
    [SerializeField] private Transform _cameraDefaultTransform;
    [SerializeField] private float _minCameraZ;
    [SerializeField] private float _maxFightersDistance;
    [SerializeField] private float _cameraCatchUpSpeed; //how quickly the camera to catch up to its target

    private Camera _camera;
    private CameraController _controller;
    private float[] _lastFighterRotations = new float[2];
    private Transform[] _targets = new Transform[2];
    private float _targetsMidPoint;
    private Vector3 _destination;

    private void Awake()
    {
        Services.CameraManager = this;
        AssignComponents();
    }

    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            _targets[i] = Services.Fighters[i].transform;
        }
        transform.position = _cameraDefaultTransform.position;
        transform.rotation = _cameraDefaultTransform.rotation;
    }

    private void Update()
    {
        _targetsMidPoint = (_targets[0].position.x + _targets[1].position.x) / 2f;
        if (_targetsMidPoint != 0f)
        {
            Debug.Log("Midpoint: " + _targetsMidPoint);
        }
        _destination = new Vector3(_targetsMidPoint, _camera.transform.position.y, _camera.transform.position.z);
        float fightersDistance = Mathf.Abs(_targets[1].position.x - _targets[0].position.x);
        if (fightersDistance < _maxFightersDistance)
        {
            _destination.z = -fightersDistance * 1.5f;
            _destination.z = Mathf.Clamp(_destination.z, -Mathf.Infinity, _minCameraZ);
        }
    }

    private void LateUpdate()
    {
        //_destination = _camera.transform.InverseTransformPoint(_destination);
        _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _destination, _cameraCatchUpSpeed * Time.deltaTime);
    }

    public void ReframeFighters()
    {
        //rotate

        //translate
    }

    public void RotateCamera(Vector3 rotation)
    {
        _controller.Rotate(rotation);
    }

    private void AssignComponents()
    {
        _camera = GetComponent<Camera>();
        _controller = GetComponent<CameraController>();
    }
}
