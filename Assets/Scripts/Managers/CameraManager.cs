using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cyan;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Util;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CameraController))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera _cinemachineCamera;
    //[SerializeField] private float _minCameraSize;
    //[SerializeField] private float _maxCameraSize;
    [SerializeField] private Transform _cameraDefaultTransform;
    [SerializeField] private float _maxCameraZ;
    [SerializeField] private float _maxFightersDistanceX;
    [SerializeField] private float _minFightersDistanceY;
    [SerializeField] private float _maxFightersDistanceY;
    [SerializeField] private float _cameraCatchUpSpeedX; //how quickly the camera to catch up to its target horizontally
    [SerializeField] private float _cameraCatchUpSpeedUp; //how quickly the camera to catch up to its target while going up
    [SerializeField] private float _cameraCatchUpSpeedDown; //how quickly the camera to catch up to its target while going down
    [SerializeField] private float _cameraCatchUpSpeedZ; //how quickly the camera to catch up to its target forward

    [Header("Cutscene")]
    private Vector3 _statueCameraPosition = new Vector3(0f, 20f, 300f);
    [SerializeField] private float _initialZoomOutDuration;
    [Tooltip("How long between the game camera reaching its destination and the UI elements appearing.")]
    [SerializeField] private float _preUiDuration;

    private Camera _camera;
    private CameraController _controller;
    private Transform[] _targets = new Transform[2];
    private float _targetsMidPointX;
    private float _targetsMidPointY;
    private Vector3 _destination;

    private float _defaultX;
    private float _defaultY;
    private float _defaultTargetY;
    private float _defaultFov;
    private Vector3 _defaultRotation;
    private float _lastDistanceY = -Mathf.Infinity;
    
    [Header("Camera Effects")]
    [SerializeField] private Material ieMaterial;
    private float defaultDistortion;
    [SerializeField] private GameObject blurFilterPrefab;
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Camera silhouetteCamera;
    [SerializeField] private Material shockwaveMaterial;

    private bool _hasGameStarted = false;

    public delegate void CameraSwitch();
    public CameraSwitch onCameraSwitch;
    public delegate void CameraFinalized();
    public CameraFinalized onCameraFinalized;

    [SerializeField] private GameObject _favorCanvas;
    [SerializeField] private GameObject _roundsCanvas;
    [SerializeField] private GameObject _specialMeterCanvas;

    private void Awake()
    {
        Services.CameraManager = this;
        AssignComponents();
        defaultDistortion = ieMaterial.GetFloat("_distortion");

        SubscribeEvents();
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
        _defaultFov = _camera.fieldOfView;
        _defaultRotation = transform.eulerAngles;
        // default ratio
        shockwaveMaterial.SetFloat("_SizeRatio", Camera.main.aspect);

        //SubscribeEvents();

        if (SceneInfo.IsTraining)
        {
            onCameraSwitch?.Invoke();
            return;
        }

        if (RoundInformation.round == 1)
        {
            DeactivateUi();
            _camera.enabled = false;
        }
        else
        {
            onCameraSwitch?.Invoke();
        }
    }

    private void DeactivateUi()
    {
        _favorCanvas.SetActive(false);
        _roundsCanvas.SetActive(false);
        _specialMeterCanvas.SetActive(false);
    }

    private void ActivateUi()
    {
        _favorCanvas.SetActive(true);
        _roundsCanvas.SetActive(true);
        _specialMeterCanvas.SetActive(true);
    }

    private void FixedUpdate()
    {
        if(_hasGameStarted)
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
            //if (fightersDistanceX < _maxFightersDistanceX)
            //{
            //    _destination.z = -fightersDistanceX * 1.5f;
            //}
            //_destination.z = -fightersDistanceX * 1.5f;
            float xZdest = -fightersDistanceX * 1.5f;
            float fightersDistanceY = Mathf.Abs(_targets[1].position.y - _targets[0].position.y);
            if (_lastDistanceY <= -Mathf.Infinity)
            {
                _lastDistanceY = fightersDistanceY;
            }
            float multiplier = Mathf.Lerp(3f, 0f, fightersDistanceX / 20f);
            //float multiplier = 10f / fightersDistanceX;
            //if (fightersDistanceY > _minFightersDistanceY && fightersDistanceY < _maxFightersDistanceY)
            //{
            //    _destination.z += fightersDistanceY * multiplier;
            //}
            //else
            //{
            //    _destination.z -= fightersDistanceY * multiplier;
            //}
            //float zDelta = fightersDistanceY - _lastDistanceY;
            float yZdest = 0f;
            if (fightersDistanceY > _lastDistanceY)
            {
                //Debug.Log("zdelta: " + zDelta);
                //_destination.z -= zDelta * 100f;
                yZdest = -fightersDistanceY * multiplier;
            }
            else if (fightersDistanceY < _lastDistanceY)
            {
                //_destination.z -= zDelta * 100f;
                yZdest = fightersDistanceY * multiplier;
            }
            _lastDistanceY = fightersDistanceY;
            _destination.z = xZdest + yZdest;
            _destination.z = Mathf.Clamp(_destination.z, -Mathf.Infinity, _maxCameraZ);
        }
    }

    private void LateUpdate()
    {
        if (_hasGameStarted)
        {
            //_destination = _camera.transform.InverseTransformPoint(_destination);
            //_camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _destination, _cameraCatchUpSpeedX * Time.deltaTime);
            Vector3 newCameraPosition = new();
            newCameraPosition.x = Mathf.Lerp(_camera.transform.localPosition.x, _destination.x, _cameraCatchUpSpeedX * Time.deltaTime);
            float cameraCatchUpY = _destination.y > _camera.transform.localPosition.y ? _cameraCatchUpSpeedUp : _cameraCatchUpSpeedDown;
            newCameraPosition.y = Mathf.Lerp(_camera.transform.localPosition.y, _destination.y, cameraCatchUpY * Time.deltaTime);
            newCameraPosition.z = Mathf.Lerp(_camera.transform.localPosition.z, _destination.z, _cameraCatchUpSpeedZ * Time.deltaTime);
            _camera.transform.localPosition = newCameraPosition;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void FindStartCameraPosition()
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
        //if (fightersDistanceX < _maxFightersDistanceX)
        //{
        //    _destination.z = -fightersDistanceX * 1.5f;
        //}
        //_destination.z = -fightersDistanceX * 1.5f;
        float xZdest = -fightersDistanceX * 1.5f;
        float fightersDistanceY = Mathf.Abs(_targets[1].position.y - _targets[0].position.y);
        if (_lastDistanceY <= -Mathf.Infinity)
        {
            _lastDistanceY = fightersDistanceY;
        }
        float multiplier = Mathf.Lerp(3f, 0f, fightersDistanceX / 20f);
        //float multiplier = 10f / fightersDistanceX;
        //if (fightersDistanceY > _minFightersDistanceY && fightersDistanceY < _maxFightersDistanceY)
        //{
        //    _destination.z += fightersDistanceY * multiplier;
        //}
        //else
        //{
        //    _destination.z -= fightersDistanceY * multiplier;
        //}
        //float zDelta = fightersDistanceY - _lastDistanceY;
        float yZdest = 0f;
        if (fightersDistanceY > _lastDistanceY)
        {
            //Debug.Log("zdelta: " + zDelta);
            //_destination.z -= zDelta * 100f;
            yZdest = -fightersDistanceY * multiplier;
        }
        else if (fightersDistanceY < _lastDistanceY)
        {
            //_destination.z -= zDelta * 100f;
            yZdest = fightersDistanceY * multiplier;
        }
        _lastDistanceY = fightersDistanceY;
        _destination.z = xZdest + yZdest;
        _destination.z = Mathf.Clamp(_destination.z, -Mathf.Infinity, _maxCameraZ);
    }

    private void AssignComponents()
    {
        _camera = GetComponent<Camera>();
        _controller = GetComponent<CameraController>();
    }

    private void SubscribeEvents()
    {
        if (RoundInformation.round == 1)
        {
            onCameraFinalized += ActivateUi;
        }
        onCameraSwitch += SwitchCamera;
        onCameraFinalized += FinalizeCamera;
        //onCameraFinalized += ActivateUi;
    }

    private void UnsubscribeEvents()
    {
        if (RoundInformation.round == 1)
        {
            onCameraFinalized -= ActivateUi;
        }
        onCameraSwitch -= SwitchCamera;
        onCameraFinalized -= FinalizeCamera;
        //onCameraFinalized -= ActivateUi;
    }

    private void SwitchCamera()
    {
        if (SceneInfo.IsTraining)
        {
            onCameraFinalized?.Invoke();
            return;
        }

        if (_cinemachineCamera) _cinemachineCamera.enabled = false;
        _camera.enabled = true;
        if (RoundInformation.round != 1)
        {
            onCameraFinalized?.Invoke();
            return;
        }

        StartCoroutine(InitialZoomOut());
    }

    private void FinalizeCamera()
    {
        _hasGameStarted = true;
    }

    private IEnumerator InitialZoomOut()
    {
        //yield return new WaitForFixedUpdate();
        //if (RoundInformation.round != 1)
        //{
        //    onCameraFinalized?.Invoke();
        //    yield break;
        //}
        FindStartCameraPosition();
        _statueCameraPosition.x = _destination.x;
        _camera.transform.localPosition = _statueCameraPosition;
        float timer = 0f;
        while (timer < _initialZoomOutDuration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            _camera.transform.localPosition = Vector3.Lerp(_statueCameraPosition, _destination, timer / _initialZoomOutDuration);
        }
        _camera.transform.localPosition = _destination;
        yield return new WaitForSeconds(_preUiDuration);

        onCameraFinalized?.Invoke();
        yield break;
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

    /// <summary>
    /// Triggers camera zoom on a sepcific target
    /// </summary>
    /// <param name="zoomSpeed">How fast it zooms onto target</param>
    /// <param name="zoomFov">What's the fov when zoomed in, DEFAULT: 41</param>
    /// <param name="zoomHold">How long do we hold the zoom for</param>
    /// <param name="distortionStrength">Amount of distortion on screen, DEFAULT: .21</param>
    /// <returns></returns>
    public IEnumerator CameraZoom(float zoomSpeed, float zoomFov, float zoomHold, float distortionStrength)
    {
        var isDistort = distortionStrength != 0;
        var zoomElapsed = 0f;
        while (zoomElapsed < zoomSpeed)
        {
            ieMaterial.SetFloat("_distortion", Mathf.Lerp(ieMaterial.GetFloat("_distortion"), 
                isDistort ? distortionStrength : defaultDistortion, zoomElapsed / zoomSpeed));
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, zoomFov, zoomElapsed / zoomSpeed);
            zoomElapsed += Time.fixedDeltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(zoomHold);

        var zoomRecoveryElapsed = 0f;
        while (zoomRecoveryElapsed < .3f)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView , _defaultFov, zoomRecoveryElapsed / .3f);
            ieMaterial.SetFloat("_distortion", Mathf.Lerp(ieMaterial.GetFloat("_distortion"), defaultDistortion, zoomRecoveryElapsed / .3f));
            zoomRecoveryElapsed += Time.deltaTime;
            yield return null;
        }
        
        ieMaterial.SetFloat("_distortion", defaultDistortion);
        _camera.fieldOfView = _defaultFov;
        transform.rotation = Quaternion.Euler(_defaultRotation);
    }

    private void OnDisable()
    {
        ieMaterial.SetFloat("_distortion", defaultDistortion);
        ResetShockwave();
    }

    void ResetShockwave()
    {
        shockwaveMaterial.SetFloat("_ManualTime", .1f);
        shockwaveMaterial.SetFloat("_Weight", 0);
        shockwaveMaterial.SetVector("_FocalPoint", new Vector4(.5f, .5f, 0f, 0f));
        shockwaveMaterial.SetFloat("_SizeRatio", 1.777f);
    }

    /// <summary>
    /// triggers a blur effect centered at a particular fighter position
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IEnumerator CameraBlur(Fighter sender, float duration)
    {
        var dir = sender.gameObject.transform.position - transform.position;
        var r = new Ray(transform.position, dir);
        var spawnedBlurFilter = Instantiate(blurFilterPrefab, r.GetPoint(2), Quaternion.identity, transform);

        // lerp to blur
        var blurElapsed = 0f;
        while (blurElapsed < .05f)
        {
            spawnedBlurFilter.GetComponent<MeshRenderer>().material.SetFloat("_AlphaStrength", Mathf.Lerp(spawnedBlurFilter.GetComponent<MeshRenderer>().material.GetFloat("_AlphaStrength"), 1f, blurElapsed / .05f));
            blurElapsed += Time.fixedDeltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(duration);
        
        // lerp to unblur
        var unblurElapsed = 0f;
        var unblurSpeed = .25f;
        while (unblurElapsed < unblurSpeed)
        {
            spawnedBlurFilter.GetComponent<MeshRenderer>().material.SetFloat("_AlphaStrength", Mathf.Lerp(spawnedBlurFilter.GetComponent<MeshRenderer>().material.GetFloat("_AlphaStrength"), 0f, unblurElapsed / unblurSpeed));
            unblurElapsed += Time.fixedDeltaTime;
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        Destroy(spawnedBlurFilter);
    }

    /// <summary>
    /// Camera shockwave effect
    /// </summary>
    /// <param name="hitPos"></param>
    /// <returns></returns>
    public IEnumerator CameraShockwave(Vector3 hitPos, float shockwaveDuration, float shockwaveExpansionAmount, bool unscaled)
    {
        shockwaveMaterial.SetVector("_FocalPoint", GetScreenPoint(hitPos));
        // start shockwave
        var shockwaveTime = .1f;
        shockwaveMaterial.SetFloat("_ManualTime", shockwaveTime);
        shockwaveMaterial.SetFloat("_Weight", 1);

        bool dissolveStarted = false;
        
        // lerp to shockwave
        var shockwaveElapsed = 0f;
        while (shockwaveElapsed < shockwaveDuration)
        {
            shockwaveTime += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) * (1 / shockwaveDuration);
            if (shockwaveTime < .99f * shockwaveExpansionAmount)
                shockwaveMaterial.SetFloat("_ManualTime", shockwaveTime);
            else if (!dissolveStarted)
            {
                StartCoroutine(ShockwaveDissolve(unscaled));
                dissolveStarted = true;
            }
            shockwaveElapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShockwaveDissolve(bool unscaled)
    {
        // lerp to shockwave
        var dissolveELapsed = 0f;
        while (dissolveELapsed < .1f)
        {
            shockwaveMaterial.SetFloat("_Weight", Mathf.Lerp(shockwaveMaterial.GetFloat("_Weight"), 0, dissolveELapsed / .1f));
            dissolveELapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        yield return new WaitForFixedUpdate();
        shockwaveMaterial.SetFloat("_Weight", 0);
    }

    private Material[] bothMats;
    public void SilhouetteToggle(bool isOn, Material[] materials, Color silhouetteColor, float silhouetteDuration)
    {
        LayerCullingShow(silhouetteCamera, "Player");
        bothMats = materials;
        silhouetteMaterial.SetColor("_SilhouetteColor", silhouetteColor);
        foreach (var mat in materials)
        {
            mat.SetFloat("_SilhouetteStrength", isOn ? 1 : 0);
        }
        silhouetteMaterial.SetFloat("_SilhouetteAlpha", isOn ? 1 : 0);
        if (isOn) StartCoroutine(SilhouetteTurnOff(silhouetteDuration));
    }
    
    private void LayerCullingShow(Camera cam, string layer) {
        cam.cullingMask |= 1 << LayerMask.NameToLayer(layer);
    }
    private void LayerCullingHide(Camera cam, string layer) {
        cam.cullingMask &= ~1 << LayerMask.NameToLayer(layer);
    }

    private IEnumerator SilhouetteTurnOff(float duration)
    {
        yield return new WaitForSeconds(duration);
        SilhouetteToggle(false, bothMats, Color.white, 0f);
        LayerCullingHide(silhouetteCamera, "Player");
    }

    public static Vector2 GetScreenPoint(Vector3 targetPos)
    {
        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(targetPos);
        return new Vector2(screenPos.x / cam.pixelWidth, screenPos.y / cam.pixelHeight);
    }
}
