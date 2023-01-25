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
    [SerializeField] private float _maxFightersDistanceX;
    [SerializeField] private float _minFightersDistanceY;
    [SerializeField] private float _maxFightersDistanceY;
    [SerializeField] private float _cameraCatchUpSpeedX; //how quickly the camera to catch up to its target horizontally
    [SerializeField] private float _cameraCatchUpSpeedY; //how quickly the camera to catch up to its target vertically
    [SerializeField] private float _cameraCatchUpSpeedZ; //how quickly the camera to catch up to its target forward

    private Camera _camera;
    private CameraController _controller;
    private Transform[] _targets = new Transform[2];
    private float _targetsMidPointX;
    private float _targetsMidPointY;
    private Vector3 _destination;

    private float _defaultX;
    private float _defaultY;
    private float _defaultTargetY;

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
        //transform.rotation = _cameraDefaultTransform.rotation;
        _defaultX = _cameraDefaultTransform.position.x;
        _defaultY = _cameraDefaultTransform.position.y;
        _defaultTargetY = _targets[0].position.y;
    }

    private void Update()
    {
        _targetsMidPointX = (_targets[0].position.x + _targets[1].position.x) / 2f;
        _targetsMidPointY = ((_targets[0].position.y + _targets[1].position.y) / 2f) - _defaultTargetY;
        //_destination = new Vector3(_targetsMidPointX, _camera.transform.position.y, _camera.transform.position.z);
        if (_targetsMidPointY < _minFightersDistanceY)
        {
            _targetsMidPointY = 0f;
        }
        _targetsMidPointX += _defaultX;
        _targetsMidPointY += _defaultY;
        _destination = new Vector3(_targetsMidPointX, _targetsMidPointY, _camera.transform.position.z);
        float fightersDistanceX = Mathf.Abs(_targets[1].position.x - _targets[0].position.x);
        if (fightersDistanceX < _maxFightersDistanceX)
        {
            _destination.z = -fightersDistanceX * 1.5f;
            _destination.z = Mathf.Clamp(_destination.z, -Mathf.Infinity, _minCameraZ);
        }
        float fightersDistanceY = Mathf.Abs(_targets[1].position.y - _targets[0].position.y);
        if (fightersDistanceY > _minFightersDistanceY && fightersDistanceY < _maxFightersDistanceY)
        {
            _destination.z += -fightersDistanceY * 1.5f;
            _destination.z = Mathf.Clamp(_destination.z, -Mathf.Infinity, _minCameraZ);
        }
    }

    private void LateUpdate()
    {
        //_destination = _camera.transform.InverseTransformPoint(_destination);
        //_camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _destination, _cameraCatchUpSpeedX * Time.deltaTime);
        Vector3 newCameraPosition = new();
        newCameraPosition.x = Mathf.Lerp(_camera.transform.localPosition.x, _destination.x, _cameraCatchUpSpeedX * Time.deltaTime);
        newCameraPosition.y = Mathf.Lerp(_camera.transform.localPosition.y, _destination.y, _cameraCatchUpSpeedY * Time.deltaTime);
        newCameraPosition.z = Mathf.Lerp(_camera.transform.localPosition.z, _destination.z, _cameraCatchUpSpeedZ * Time.deltaTime);
        _camera.transform.localPosition = newCameraPosition;
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

    /// <summary>
    /// Triggers camera shake
    /// </summary>
    /// <param name="duration">how long it lasts</param>
    /// <param name="magnitude">shake intensity, best to keep between 0 - 1</param>
    /// <returns></returns>
    public IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originPos = transform.localPosition;

        var shakeElapsed = 0f;

        while (shakeElapsed < duration)
        {
            var x = originPos.x + Random.Range(-1f, 1f) * magnitude;
            var y = originPos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originPos.z);
            shakeElapsed += Time.deltaTime;

            yield return null;
        }
    }

    public IEnumerator CameraZoom(Vector3 zoomTarget, float zoomHold)
    {
        yield return null;
    }
}
