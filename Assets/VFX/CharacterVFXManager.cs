using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public enum vfxAssets {AfterImage, };

[RequireComponent(typeof(InputManager))]
public class CharacterVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect _visualEffect;
    [SerializeField] private VisualEffectAsset[] _vfxGraphs;
    private Fighter _fighter;
    private VFXSpawnManager _vfxSpawnManager;

    //
    private SpriteRenderer _spriteRenderer;
    private float delayTimer;
    public float delayTime;

    //
    private InputManager _inputManager;
    
    void VFXAssignComponents() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _inputManager = GetComponent<InputManager>();
        _fighter = GetComponent<Fighter>();
        _vfxSpawnManager = GameObject.Find("VFX Camera").GetComponent<VFXSpawnManager>();
    }
    void VFXSubscribeEvents() {
        _inputManager.Actions["Dash"].perform += AfterImage;
        _inputManager.Actions["Dash"].finish += StopVFX;
    }

    void StopVFX(InputManager.Action action) {
        _visualEffect.SendEvent("OnStop");
    }

    void AfterImage(InputManager.Action action) {
        _visualEffect.SendEvent("OnDash");
        _vfxSpawnManager.InitializaeVFX(VFXGraphs.DASH_SMOKE, transform.position + new Vector3(0f, 1.08f, 0f), GetComponent<Fighter>());
    }
    
    void Awake()
    {
        VFXAssignComponents();
        _visualEffect.visualEffectAsset = _vfxGraphs[((int)vfxAssets.AfterImage)];
    }

    private void Start()
    {
        VFXSubscribeEvents();
    }

    void Update() {
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
        
        _visualEffect.SetBool("FaceLeft", _fighter.FacingDirection == Fighter.Direction.Left);
    }
}
