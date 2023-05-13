using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SunsetTransition : MonoBehaviour
{
    [Range(0, 1)] [SerializeField] private float skyboxBlend = 0;
    [Header("Values")]
    private Color _defaultFogColor;
    private float _defaultSkyboxRotationSpeed;
    [SerializeField] private float transitionSkyboxRotationSpeed = 32;
    [SerializeField, ColorUsage(false)] private Color sunsetFogColor;

    [Header("References")]
    [SerializeField] private Animator sunAnimator;
    [SerializeField] private Volume sunsetVolume;

    void Awake()
    {
        _defaultFogColor = RenderSettings.fogColor;
        _defaultSkyboxRotationSpeed = VFXSpawnManager.skyboxRotationSpeed;
    }
    
    void Update()
    {
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;
        if (skyboxBlend > 0 && skyboxBlend < 1) VFXSpawnManager.skyboxRotationSpeed = transitionSkyboxRotationSpeed;

        // debug
        // if (Keyboard.current.pKey.wasPressedThisFrame)
        // {
        //     PlaySunset();
        // }
    }

    public void PlaySunset()
    {
        GetComponent<Animator>().Play("SunsetLightTransition");
        sunAnimator.Play("SunsetTransition");
    }

    public void ResetSkyboxRotationSpeed()
    {
        VFXSpawnManager.skyboxRotationSpeed = _defaultSkyboxRotationSpeed;
    }

    private void OnDisable()
    {
        skyboxBlend = 0;
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;
    }
}
