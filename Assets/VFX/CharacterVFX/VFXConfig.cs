using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/VFX Config")]
public class VFXConfig : ScriptableObject
{
    [field: SerializeField] public GameObject[] VFXSet { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public VFXHitData LightHit { private set; get; }
    [field: SerializeField] public VFXHitData MediumHit { private set; get; }
    [field: SerializeField] public VFXHitData HeavyHit { private set; get; }
    [field: SerializeField] public VFXHitData SpecialHit { private set; get; }
}

[Serializable] public class VFXHitData
{
    [Header("Camera Shake")]
    [field: SerializeField] public bool hasCameraShake;
    [field: SerializeField] [Range(0, 0.3f)] public float cameraShakeDuration;
    [field: SerializeField] [Range(0, 0.2f)] public float cameraShakeMagnitude;
    [Header("Distortion Zoom")]
    [field: SerializeField] public bool hasDistortionZoom;
    [field: SerializeField] public float zoomFov;
    [field: SerializeField] public float zoomDistortion;
    [Header("Ripple")]
    [field: SerializeField] public bool hasRipple;
    [Header("Silhouette")]
    [field: SerializeField] public bool hasSilhouette;
}
