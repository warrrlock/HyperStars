using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum vfxAssets {AfterImage, };

public class CharacterVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect _visualEffect;

    [SerializeField] private VisualEffectAsset[] _vfxGraphs;

    //
    private SpriteRenderer _spriteRenderer;
    private float delayTimer;
    public float delayTime;

    //
    private InputManager _inputManager;

    // TEST
    string currentVFXState = "AfterImage";
    
    void VFXAssignComponents() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _inputManager = GetComponent<InputManager>();
    }
    void VFXSubscribeEvents() {
        _inputManager.Actions["Dash"].perform += AfterImage;
    }

    void VFXSwitches() {
        switch(currentVFXState) {
            case "AfterImage":
                _visualEffect.visualEffectAsset = _vfxGraphs[((int)vfxAssets.AfterImage)];
                break;
            default:
                _visualEffect.visualEffectAsset = _vfxGraphs[((int)vfxAssets.AfterImage)];
                break;
        }
    }
    
    void StopVFX(InputManager.Action action) {
        _visualEffect.SendEvent("OnStop");
    }

    void AfterImage(InputManager.Action action) {
        _visualEffect.SendEvent("IsDashing");
    }
    
    void Start()
    {
        VFXAssignComponents();
        VFXSubscribeEvents();

        _visualEffect.visualEffectAsset = _vfxGraphs[((int)vfxAssets.AfterImage)];
    }

    void Update() {
        // VFXSwitches();
    }
    
    void LateUpdate()
    {
        SpriteUpdate();
    }

    void SpriteUpdate() {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            _visualEffect.SetTexture("MainTex2D", _spriteRenderer.sprite.texture);
            delayTimer = delayTime;
        }
    }
}
