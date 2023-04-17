using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.VFX;
using UnityEngine.Rendering;

[RequireComponent(typeof(InputManager))]
public class CharacterVFXManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public VFXConfig vfxConfig;
    [Header("Character Based VFX")]
    [SerializeField] private VisualEffect visualEffect;
    private Fighter _fighter;
    private VFXSpawnManager _vfxSpawnManager;
    [SerializeField] private float actionSmokeGroundOffset;

    //
    private SpriteRenderer _spriteRenderer;
    private float _delayTimer;
    [Header("Afterimage controls")]
    [SerializeField] private bool _hasDelay;
    public float delayTime;

    //
    private InputManager _inputManager;
    
    // states
    [Header("State change based spawning")]
    [Tooltip("For spawning dash smoke.")]
    [SerializeField] private BaseState[] _dashStates;
    [Tooltip("For spawning afterimage.")]
    [SerializeField] private List<BaseState> _afterImageStates;
    [Tooltip("For spawning camera blur.")]
    [SerializeField] private BaseState[] _blurStates;
    [SerializeField] private BaseState[] _parryStates;
    
    
    void Awake()
    {
        VFXAssignComponents();
    }

    private void Start()
    {
        VFXSubscribeEvents();
    }

    void Update() {
        AfterImageUpdate();
    }

    private void OnDestroy()
    {
        VFXUnsubscribeEvents();
    }
    
    void VFXAssignComponents() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _inputManager = GetComponent<InputManager>();
        _fighter = GetComponent<Fighter>();
        _vfxSpawnManager = GameObject.Find("VFX Camera").GetComponent<VFXSpawnManager>();
    }
    void VFXSubscribeEvents() {
        foreach (BaseState dashState in _dashStates) _fighter.BaseStateMachine.States[dashState].execute += DashSmoke;
        _inputManager.Actions["Jump"].perform += JumpSmoke;
        // _inputManager.Actions["Parry"].perform += BlockGlow;
        // _fighter.Events.onBlockHit += BlockGlow;
        _fighter.Events.onStateChange += SpawnOnStateChange;
        _fighter.Events.onLandedHurt += GroundWave;
        _fighter.Events.wallBounce += WallWave;
    }
    void VFXUnsubscribeEvents() {
        foreach (BaseState dashState in _dashStates) _fighter.BaseStateMachine.States[dashState].execute -= DashSmoke;
        _inputManager.Actions["Jump"].perform -= JumpSmoke;
        // _inputManager.Actions["Parry"].perform -= BlockGlow;
        // _fighter.Events.onBlockHit -= BlockGlow;
        _fighter.Events.onStateChange -= SpawnOnStateChange;
        _fighter.Events.onLandedHurt -= GroundWave;
        _fighter.Events.wallBounce -= WallWave;
    }

    void DashSmoke() {
        if (_fighter.MovementController.IsGrounded)
        {
            _vfxSpawnManager.InitializeVFX(VFXGraphsNeutral.SMOKE_DASH, transform.localPosition + new Vector3(0f, 
                actionSmokeGroundOffset, 0f), GetComponent<Fighter>());
        }
        else
        {
            _vfxSpawnManager.InitializeVFX(VFXGraphsNeutral.SMOKE_AIRDASH, transform.localPosition + new Vector3(0f, 
                actionSmokeGroundOffset, 0f), GetComponent<Fighter>());
        }
    }
    
    void JumpSmoke(InputManager.Action action) {
        _vfxSpawnManager.InitializeVFX(VFXGraphsNeutral.SMOKE_JUMP, transform.localPosition + new Vector3(0f, 
            actionSmokeGroundOffset, 0f), GetComponent<Fighter>());
    }
    
    // void BlockGlow(Dictionary<string, object> d)
    // {
    //     try
    //     {
    //         Fighter attacked = d["attacked"] as Fighter;
    //         Fighter attacker = d["attacker"] as Fighter;
    //         if (!attacker || !attacked) return;
    //         TriggerParry(attacked);
    //     }
    //     catch (KeyNotFoundException)
    //     {
    //         Debug.Log("blocker not found");
    //     }
    // }
    //
    // void BlockGlow(InputManager.Action action)
    // {
    //     TriggerParry(_fighter);
    // }
    //
    // private Coroutine parryRoutine;
    // void TriggerParry(Fighter f)
    // {
    //     if (parryRoutine != null) StopCoroutine(parryRoutine);
    //     parryRoutine = StartCoroutine(ParryGlow(f));
    // }
    //
    // IEnumerator ParryGlow(Fighter f)
    // {
    //     SpriteRenderer sr = f.GetComponent<SpriteRenderer>();
    //     sr.material.SetFloat("_Parry_Trigger", 1f);
    //     yield return new WaitForSeconds(.35f);
    //     sr.material.SetFloat("_Parry_Trigger", 0f);
    // }

    void GroundWave()
    {
        _vfxSpawnManager.InitializeVFX(VFXGraphsNeutral.WAVE_GROUND, transform.localPosition + new Vector3(0, .3f, 0));
    }

    void WallWave()
    {
        _vfxSpawnManager.InitializeVFX(_fighter.FacingDirection == Fighter.Direction.Right ? VFXGraphsNeutral.WAVE_WALL_RIGHT : VFXGraphsNeutral.WAVE_WALL_LEFT,
            transform.localPosition + new Vector3(0, 0f, 0));
    }
    
    /// <summary>
    /// Coroutine for shaking the character during hit stop
    /// </summary>
    /// <param name="f">which fighter to shake</param>
    /// <param name="shakeSpeed">how fast is the shake</param>
    /// <param name="shakeScale">how intense is the shake (!!!LOWER IS MORE INTENSE!!!) </param>
    /// <param name="shakeDuration">how long does the character shake</param>
    /// <returns></returns>
    public IEnumerator Shake(Fighter f, float shakeSpeed, float shakeScale, float shakeDuration)
    {
        SpriteRenderer sr = f.GetComponent<SpriteRenderer>();
        sr.material.SetFloat("_Shake_Scale", shakeScale);

        if (!f.MovementController.IsGrounded)
            sr.material.SetFloat("_Vertical_Shake_Trigger", 1f);

        var shakeTimer = 0f;
        var shake = shakeSpeed;

        while (shakeTimer < shakeDuration)
        {
            sr.material.SetFloat("_Shake_Intensity", shake);
            shake = Mathf.Lerp(shake, 0, shakeTimer / shakeDuration);
            shakeTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(.1f);
        sr.material.SetFloat("_Shake_Intensity", 0f); // stopping the shake
        sr.material.SetFloat("_Shake_Scale", 3f); // default to default shake scale
        sr.material.SetFloat("_Vertical_Shake_Trigger", 0f);
    }

    void AfterImageUpdate() {
        if (_hasDelay)
        {
            if (_delayTimer > 0)
            {
                _delayTimer -= Time.deltaTime;
            }
            else
            {
                visualEffect.SetTexture("MainTex2D", _spriteRenderer.sprite.texture);
                _delayTimer = delayTime;
            }
        }
        else
        {
            visualEffect.SetTexture("MainTex2D", _spriteRenderer.sprite.texture);
        }

        visualEffect.SetBool("FaceLeft", _fighter.FacingDirection == Fighter.Direction.Left);
    }

    private void SpawnOnStateChange(BaseState s)
    {
        // spawn afterimage
        visualEffect.SendEvent("OnStop");
        foreach (BaseState wantedState in _afterImageStates)
        {
            if (s != wantedState) continue;
            visualEffect.SendEvent("OnDash");
            break;
        }

        // spawn blur
        foreach (BaseState wantedState in _blurStates)
        {
            if (s == wantedState)
            {
                StartCoroutine(Services.CameraManager.CameraBlur(_fighter, .35f));
                StartCoroutine(Services.CameraManager.CameraZoom(.1f, 38f, .2f, .12f));
                break;
            }
        }
        
        // spawn parry flash
        _spriteRenderer.material.SetFloat("_Parry_Trigger", 0f);
        foreach (BaseState wantedState in _parryStates)
        {
            if (s == wantedState)
            {
                _spriteRenderer.material.SetFloat("_Parry_Trigger", 1f);
                break;
            }
        }
    }

    public void TurnOnParryFlash()
    {
        _spriteRenderer.material.SetFloat("_Parry_Trigger", 1f);
    }

    public void TurnOffParryFlash()
    {
        _spriteRenderer.material.SetFloat("_Parry_Trigger", 0f);
    }
}
