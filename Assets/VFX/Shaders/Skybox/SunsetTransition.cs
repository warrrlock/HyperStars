using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SunsetTransition : MonoBehaviour
{
    [SerializeField] private bool isDebugging;
    [Range(0, 1)] [SerializeField] private float skyboxBlend = 0;
    [Header("Values")]
    private Color _defaultFogColor;
    [SerializeField] private float defaultSkyboxRotationSpeed;
    [SerializeField] private float transitionSkyboxRotationSpeed = 32;
    [SerializeField, ColorUsage(false)] private Color sunsetFogColor;

    [Header("References")]
    [SerializeField] private Animator sunAnimator;
    [SerializeField] private Volume sunsetVolume;
    [SerializeField] private ReflectionProbe[] reflectionProbes;

    void Awake()
    {
        _defaultFogColor = RenderSettings.fogColor;
        VFXSpawnManager.skyboxRotationSpeed = defaultSkyboxRotationSpeed;
    }
    
    void Update()
    {
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;
        if (skyboxBlend > 0 && skyboxBlend < 1) VFXSpawnManager.skyboxRotationSpeed = transitionSkyboxRotationSpeed;

        // debug
        if (!isDebugging) return;
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlaySunset();
        }
    }

    public void PlaySunset()
    {
        // handle reflections
        foreach (var probe in reflectionProbes)
        {
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
        }
        GetComponent<Animator>().Play("SunsetLightTransition");
        sunAnimator.Play("SunsetTransition");
    }

    public void SunsetFinished()
    {
        // handle reflections
        foreach (var probe in reflectionProbes)
        {
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        }
        VFXSpawnManager.skyboxRotationSpeed = defaultSkyboxRotationSpeed;
    }

    private void OnDisable()
    {
        skyboxBlend = 0;
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;
    }
}
