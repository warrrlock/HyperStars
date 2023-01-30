using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.Rendering;

public enum vfxAssets {AfterImage, };

[RequireComponent(typeof(InputManager))]
public class CharacterVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private VisualEffectAsset[] vfxGraphs;
    private Fighter _fighter;
    private VFXSpawnManager _vfxSpawnManager;
    [SerializeField] private float dashSmokeGroundOffset;
    [SerializeField] private float jumpSmokeGroundOffset;

    //
    private SpriteRenderer _spriteRenderer;
    private float _delayTimer;
    public float delayTime;

    //
    private InputManager _inputManager;
    
    // states
    [SerializeField] private List<BaseState> _afterImageStates;
    
    void VFXAssignComponents() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _inputManager = GetComponent<InputManager>();
        _fighter = GetComponent<Fighter>();
        _vfxSpawnManager = GameObject.Find("VFX Camera").GetComponent<VFXSpawnManager>();
    }
    void VFXSubscribeEvents() {
        _inputManager.Actions["Dash"].perform += DashSmoke;
        _inputManager.Actions["Jump"].perform += JumpSmoke;
        _fighter.Events.onBlockHit += BlockGlow;
        _fighter.Events.onStateChange += SpawnAfterImage;
    }

    void DashSmoke(InputManager.Action action) {
        if (_fighter.MovementController.IsGrounded)
        {
            _vfxSpawnManager.InitializaeVFX(VFXGraphs.DASH_SMOKE, transform.localPosition + new Vector3(0f, 
                        dashSmokeGroundOffset, 0f), GetComponent<Fighter>());
        }
    }
    
    void JumpSmoke(InputManager.Action action) {
        _vfxSpawnManager.InitializaeVFX(VFXGraphs.JUMP_SMOKE, transform.localPosition + new Vector3(0f, 
            jumpSmokeGroundOffset, 0f), GetComponent<Fighter>());
    }

    void BlockGlow(Dictionary<string, object> d)
    {
        try
        {
            Fighter attacked = d["attacked"] as Fighter;
            Fighter attacker = d["attacker"] as Fighter;
            if (!attacker || !attacked) return;
            StartCoroutine(ParryGlow(attacked));
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("blocker not found");
        }
    }

    IEnumerator ParryGlow(Fighter f)
    {
        f.GetComponent<SpriteRenderer>().material.SetFloat("_Parry_Trigger", 1f);
        yield return new WaitForSeconds(.35f);
        f.GetComponent<SpriteRenderer>().material.SetFloat("_Parry_Trigger", 0f);
    }

    public IEnumerator Shake(Fighter f, float shakeIntensity, float shakeDuration)
    {
        f.GetComponent<SpriteRenderer>().material.SetFloat("_Shake_Intensity", shakeIntensity);
        yield return new WaitForSeconds(shakeDuration);
        f.GetComponent<SpriteRenderer>().material.SetFloat("_Shake_Intensity", 0f); // stopping the shake
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

    private void SpawnAfterImage(BaseState s)
    {
        visualEffect.visualEffectAsset = vfxGraphs[(int)vfxAssets.AfterImage];
        foreach (BaseState wantedState in _afterImageStates)
        {
            if (s == wantedState)
            {
                visualEffect.SendEvent("OnDash");
                return;
            }
        }
        visualEffect.SendEvent("OnStop");
    }
}
