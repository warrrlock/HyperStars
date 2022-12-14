using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public enum vfxAssets {AfterImage, };

[RequireComponent(typeof(InputManager))]
public class CharacterVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private VisualEffectAsset[] vfxGraphs;
    private Fighter _fighter;
    private VFXSpawnManager _vfxSpawnManager;
    [SerializeField] private float groundOffset;

    //
    private SpriteRenderer _spriteRenderer;
    private float _delayTimer;
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
    }

    void AfterImage(InputManager.Action action) {
        _vfxSpawnManager.InitializaeVFX(VFXGraphs.DASH_SMOKE, transform.localPosition + new Vector3(0f, 
            groundOffset, 0f), GetComponent<Fighter>());
    }

    void Awake()
    {
        VFXAssignComponents();
    }

    private void Start()
    {
        VFXSubscribeEvents();
    }

    void Update() {
        SpriteUpdate();
    }

    void SpriteUpdate() {
        if (_delayTimer > 0)
        {
            _delayTimer -= Time.deltaTime;
        }
        else
        {
            visualEffect.SetTexture("MainTex2D", _spriteRenderer.sprite.texture);
            _delayTimer = delayTime;
        }
        
        visualEffect.SetBool("FaceLeft", _fighter.FacingDirection == Fighter.Direction.Left);
    }

    // animation events
    public void VFX_CharacterTurnOn(vfxAssets vfx)
    {
        visualEffect.visualEffectAsset = vfxGraphs[(int)vfx];
        visualEffect.SendEvent("OnDash");
    }

    public void VFX_CharacterTurnOff(vfxAssets vfx)
    {
        visualEffect.visualEffectAsset = vfxGraphs[(int)vfx];
        visualEffect.SendEvent("OnStop");
    }
}
