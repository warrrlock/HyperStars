using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
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
    [Header("Afterimage controls")]
    [SerializeField] private bool _hasDelay;
    public float delayTime;

    //
    private InputManager _inputManager;
    
    // states
    [SerializeField] private List<BaseState> _afterImageStates;

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
        _inputManager.Actions["Dash"].perform += DashSmoke;
        _inputManager.Actions["Jump"].perform += JumpSmoke;
        _fighter.Events.onBlockHit += BlockGlow;
        _fighter.Events.onStateChange += SpawnAfterImage;
    }
    
    void VFXUnsubscribeEvents() {
        _inputManager.Actions["Dash"].perform -= DashSmoke;
        _inputManager.Actions["Jump"].perform -= JumpSmoke;
        _fighter.Events.onBlockHit -= BlockGlow;
        _fighter.Events.onStateChange -= SpawnAfterImage;
    }

    void DashSmoke(InputManager.Action action) {
        if (_fighter.MovementController.IsGrounded)
        {
            _vfxSpawnManager.InitializeVFX(VFXGraphs.DASH_SMOKE, transform.localPosition + new Vector3(0f, 
                        dashSmokeGroundOffset, 0f), GetComponent<Fighter>());
        }
    }
    
    void JumpSmoke(InputManager.Action action) {
        _vfxSpawnManager.InitializeVFX(VFXGraphs.JUMP_SMOKE, transform.localPosition + new Vector3(0f, 
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

    void SpriteUpdate() {
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

    private void SpawnAfterImage(BaseState s)
    {
        // visualEffect.visualEffectAsset = vfxGraphs[(int)vfxAssets.AfterImage];
        foreach (BaseState wantedState in _afterImageStates)
        {
            if (s == wantedState)
            {
                visualEffect.SendEvent("OnDash");
                StartCoroutine(Services.CameraManager.CameraBlur(_fighter, .35f));
                StartCoroutine(Services.CameraManager.CameraZoom(.1f, 38f, .2f, .12f));
                return;
            }
        }
        visualEffect.SendEvent("OnStop");
    }
}
