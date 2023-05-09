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
    [SerializeField, ColorUsage(false)] private Color sunsetFogColor;

    [Header("References")]
    [SerializeField] private Animator sunAnimator;
    [SerializeField] private Volume sunsetVolume;

    void Awake()
    {
        _defaultFogColor = RenderSettings.fogColor;
    }
    
    void Update()
    {
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;

        // debug
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            GetComponent<Animator>().Play("SunsetLightTransition");
        }
    }

    public void PlaySunset()
    {
        sunAnimator.Play("SunsetTransition");
    }

    private void OnDisable()
    {
        skyboxBlend = 0;
        RenderSettings.skybox.SetFloat("_TextureBlend", skyboxBlend);
        RenderSettings.fogColor = Color.Lerp(_defaultFogColor, sunsetFogColor, skyboxBlend);
        sunsetVolume.weight = skyboxBlend;
    }
}
