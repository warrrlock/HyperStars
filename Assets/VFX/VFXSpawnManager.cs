using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public enum VFXGraphsNeutral
{
    HIT_BASE, 
    PARRY, PARRY_TWT,
    SMOKE_DASH, SMOKE_JUMP, 
    WAVE_GROUND, WAVE_WALL_RIGHT, WAVE_WALL_LEFT,
}

public enum VFXGraphsCharacter
{
    Hit_V1, Hit_V2, Hit_Special, TakeDamage_Random
}

public enum VFXTypes
{
    Hit_Base, Hit_Character, Hit_Special, Combat_Neutral, Hit_TakeDamage
}

public class VFXSpawnManager : MonoBehaviour
{
    [Header("VFX Prefabs")]
    [SerializeField] public GameObject[] visualEffectPrefabsNeutral;

    [Header("VFX Configs")]
    [SerializeField] public VFXConfig[] vfxConfigs;

    [Header("Skybox")]
    private float currentRotation;
    [SerializeField] private float skyboxRotationSpeed;

    void Start()
    {
        foreach (Fighter f in Services.Fighters)
        {
            f.Events.onAttackHit += PlayHitVFX;
            f.Events.onBlockHit += PlayBlockVFX;
        }
    }

    void Update()
    {
        SkyboxRotation();
    }
    

    void SkyboxRotation()
    {
        currentRotation += skyboxRotationSpeed * Time.deltaTime;
        if (currentRotation >= 360)
        {
            currentRotation = 0;
        }
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }

    private void OnDestroy()
    {
        foreach (Fighter f in Services.Fighters)
        {
            f.Events.onAttackHit -= PlayHitVFX;
            f.Events.onBlockHit -= PlayBlockVFX;
        }
    }

    public void InitializeVFX(VFXGraphsNeutral graphIndex, Vector3 spawnPos)
    {
        Instantiate(visualEffectPrefabsNeutral[(int)graphIndex], spawnPos, Quaternion.identity);
    }
    
    public void InitializeVFX(VFXGraphsNeutral graphIndex, Vector3 spawnPos, Fighter triggerFighter)
    {
        VisualEffect newVFX = Instantiate(visualEffectPrefabsNeutral[(int)graphIndex], spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
        newVFX.GetComponent<VFXCleanUp>().f = triggerFighter;
    }

    public void InitializeVFX(VFXTypes type, Vector3 spawnPos, Fighter triggerFighter)
    {
        VFXGraphsCharacter vfxGraphChar = VFXGraphsCharacter.Hit_V1;
        switch (type)
        {
            case VFXTypes.Hit_Character:
                float rand = Random.Range(0f, 1f);
                vfxGraphChar = rand < .5f ? VFXGraphsCharacter.Hit_V1 : VFXGraphsCharacter.Hit_V2;
                break;
            case VFXTypes.Hit_Special:
                vfxGraphChar = VFXGraphsCharacter.Hit_Special;
                break;
            case VFXTypes.Hit_TakeDamage:
                vfxGraphChar = VFXGraphsCharacter.TakeDamage_Random;
                break;
        }
        VFXConfig triggerConfig = triggerFighter.GetComponent<CharacterVFXManager>().vfxConfig;
        VisualEffect newVFX = Instantiate(triggerConfig.VFXSet[(int)vfxGraphChar], spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
        newVFX.GetComponent<VFXCleanUp>().f = triggerFighter;
    }

    void PlayHitVFX(Dictionary<string, object> message)
    {
        try
        {
            Vector3 hitPos = (Vector3)message["hit point"];
            Fighter sender = (Fighter)message["attacker"];
            Fighter receiver = (Fighter)message["attacked"];
            AttackInfo attackInfo = (AttackInfo)message["attack info"];

            CameraManager cam = Services.CameraManager;
            VFXConfig senderConfig = sender.GetComponent<CharacterVFXManager>().vfxConfig;
            
            InitializeVFX(VFXGraphsNeutral.HIT_BASE, hitPos, sender);
            InitializeVFX(VFXTypes.Hit_TakeDamage, hitPos, receiver);
            StartCoroutine(receiver.GetComponent<CharacterVFXManager>().Shake(receiver, 98f, 1f, .8f));
            // camera based on hits
            switch (attackInfo.attackType)
            {
                case AttackInfo.AttackType.Light:
                    AssignHitVFXStyle(senderConfig.LightHit, hitPos, sender, receiver);
                    break;
                case AttackInfo.AttackType.Medium:
                    InitializeVFX(VFXTypes.Hit_Character, hitPos, sender);
                    AssignHitVFXStyle(senderConfig.MediumHit, hitPos, sender, receiver);
                    break;
                case AttackInfo.AttackType.Heavy:
                    InitializeVFX(VFXTypes.Hit_Character, hitPos, sender);
                    AssignHitVFXStyle(senderConfig.HeavyHit, hitPos, sender, receiver);
                    break;
                case AttackInfo.AttackType.Special:
                    InitializeVFX(VFXTypes.Hit_Character, hitPos, sender);
                    AssignHitVFXStyle(senderConfig.SpecialHit, hitPos, sender, receiver);
                    break;
                default:
                    StartCoroutine(cam.CameraShake(.1f, .04f));
                    break;
            }
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }

    void AssignHitVFXStyle(VFXHitData configData, Vector3 hitPos, Fighter sender, Fighter receiver)
    {
        CameraManager cam = Services.CameraManager;
        
        if (configData.cameraShake != CameraShakeType.NoShake)
        {
            StartCoroutine(cam.CameraShake(configData.cameraShakeDuration, configData.cameraShakeMagnitude));
        }
        if (configData.distortionZoom)
        {
            DistortionZoomSettings s = configData.distortionZoom.Value;
            StartCoroutine(cam.CameraZoom(s.zoomSpeed, s.zoomFov, s.zoomHoldTime, s.zoomDistortion));
        }
        if (configData.shockwave)
        {
            ShockwaveSettings s = configData.shockwave.Value;
            StartCoroutine(cam.Camerashockwave(hitPos, s.shockwaveDuration, s.shockwavePercentage));
        }
        if (configData.hasSilhouette)
        {
            Material[] mats =
            {
                sender.GetComponent<SpriteRenderer>().material, 
                receiver.GetComponent<SpriteRenderer>().material
            };
            cam.SilhouetteToggle(true, mats);
        }
    }
    
    void PlayBlockVFX(Dictionary<string, object> message)
    {
        try
        {
            Vector3 hitPos = (Vector3) message["hit point"];
            Fighter sender = (Fighter) message["attacker"];
            Fighter receiver = (Fighter) message["attacked"];
            InitializeVFX(VFXGraphsNeutral.PARRY_TWT, hitPos, sender);
            StartCoroutine(receiver.GetComponent<CharacterVFXManager>().Shake(sender, 98f, 2f, .5f));
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }

    private void OnDisable()
    {
        // skybox rotation reset
        RenderSettings.skybox.SetFloat("_Rotation", 0);
    }
}
