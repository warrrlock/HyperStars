using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/VFX Config")]
public class VFXConfig : ScriptableObject
{
    [field: SerializeField] public GameObject[] VFXSet;

    [field: Space(10)]
    [field: SerializeField] public VFXHitData LightHit;
    [field: SerializeField] public VFXHitData MediumHit;
    [field: SerializeField] public VFXHitData HeavyHit;
    [field: SerializeField] public VFXHitData SpecialHit;
    [field: SerializeField] public VFXHitData ParryHit;
}

[Serializable] public class VFXHitData
{
    [field: SerializeField] public CameraShakeType cameraShake;
    [field: SerializeField] public Optional<CameraShakeSettings> cameraShakeSettingsOverride;
    public float cameraShakeDuration
    {
        get
        {
            if (cameraShakeSettingsOverride)
            {
                return cameraShakeSettingsOverride.Value.cameraShakeDuration;
            }
            return cameraShake switch
            {
                CameraShakeType.LightShake => .2f,
                CameraShakeType.MediumShake or CameraShakeType.HeavyShake => .3f,
            };
        }
    }
    public float cameraShakeMagnitude
    {
        get
        {
            if (cameraShakeSettingsOverride)
            {
                return cameraShakeSettingsOverride.Value.cameraShakeMagnitude;
            }
            return cameraShake switch
            {
                CameraShakeType.LightShake => .03f,
                CameraShakeType.MediumShake => .06f,
                CameraShakeType.HeavyShake => .19f,
            };
        }
    }
    [field: SerializeField] public Optional<CharacterShakeSettings> characterShakeOverride;
    [field: SerializeField] public Optional<DistortionZoomSettings> distortionZoom;
    [field: SerializeField] public Optional<ShockwaveSettings> shockwave;
    [field: SerializeField] public Optional<SilhouetteSettings> silhouette;
}

public enum CameraShakeType
{
    NoShake, LightShake, MediumShake, HeavyShake
}

[Serializable] public struct CameraShakeSettings
{
    [Range(0f, .3f)] public float cameraShakeDuration;
    [Range(0f, .2f)] public float cameraShakeMagnitude;
}

[Serializable] public struct CharacterShakeSettings
{
    [Range(0f, 1f)] public float duration;
    [Range(0f, 2f)] [Tooltip("SMALLER = MORE SHAKE")] public float magnitude;
}

[Serializable] public struct DistortionZoomSettings
{
    [Range(0f, .3f)] [Tooltip("Recommended: 0.2")] public float zoomSpeed;
    [Range(32, 41)] [Tooltip("Default: 41")] public float zoomFov;
    [Range(0f, .2f)] [Tooltip("Recommended: 0.1")] public float zoomHoldTime;
    [Range(-.21f, .21f)] [Tooltip("Default: 0.21")] public float zoomDistortion;
}

[Serializable] public struct ShockwaveSettings
{
    [Range(0f, 1f)] [Tooltip("Recommended: 0.3")] public float shockwaveDuration;
    [Range(0f, 1f)] [Tooltip("Recommended: 0.6")] public float shockwavePercentage;
    public bool useUnscaledTime;
}

[Serializable]
public struct SilhouetteSettings
{
    public Color silhouetteColor;
    [Range(0f, .3f)] [Tooltip("Recommended: 0.1")] public float silhouetteDuration;
}