using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXCleanUp : MonoBehaviour
{
    private VisualEffect _vfx;
    private float _lifeTime;
    [HideInInspector] public Fighter f;
    [HideInInspector] public bool isUnscaledTime;
    [SerializeField] private VFXTypes VFXType;
    [SerializeField] private bool isMoveBased;
    [SerializeField] private bool canUseUnscaledTime;

    void Start()
    {
        _vfx = GetComponent<VisualEffect>();
        _lifeTime = 0;
        // all vfx destroy after one second
        Destroy(gameObject, 1);
        if (f)
        {
            _vfx.SetBool("FaceLeft", isMoveBased ? f.MovingDirection == Fighter.Direction.Left : f.FacingDirection == Fighter.Direction.Left);
            switch (VFXType)
            {
                case VFXTypes.Hit_Character or VFXTypes.Hit_Special:
                    ChangeColor();
                    break;
            }
        }
    }

    private void Update()
    {
        // if (f)
        // {
        //     _vfx.SetBool("FaceLeft", isMoveBased ? f.MovingDirection == Fighter.Direction.Left : f.FacingDirection == Fighter.Direction.Left);
        // }
        _lifeTime += isUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (canUseUnscaledTime)
        {
            _vfx.SetFloat("UnscaledTime", _lifeTime);
        }
    }

    void ChangeColor()
    {
        ColorPicker picker = f.GetComponent<ColorPicker>();
        Gradient g = picker.characterColors.Palette[picker.currentColorIndex].EffectColor;
        GradientUsageAttribute gua = new GradientUsageAttribute(true);
        _vfx.SetGradient("ParticleGradient", picker.characterColors.Palette[picker.currentColorIndex].EffectColor);
    }
}
