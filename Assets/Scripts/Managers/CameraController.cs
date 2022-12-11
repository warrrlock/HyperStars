using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _rotationalPivot;

    private Camera _camera;

    private void Awake()
    {
        AssignComponents();
    }

    public void Track(Vector3 target)
    {

    }

    public void Zoom(float target)
    {

    }

    public void Rotate(Vector3 rotation)
    {
        _rotationalPivot.eulerAngles += rotation;
    }

    public void Pan()
    {
        //_camera.transform.eulerAngles
    }

    private void AssignComponents()
    {
        _camera = GetComponent<Camera>();
    }
}
