using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXGraphs
{
    LISA_HIT, BLUK_HIT, LISA_HIT_PARRY, DASH_SMOKE, JUMP_SMOKE, GROUND_WAVE, WALL_WAVE_RIGHT, WALL_WAVE_LEFT, RAND_NUTS,
    TWT_HIT, TWT_DEF
}

public enum VFXTypes
{
    Hit_Base, Hit_Character, Hit_Special, Combat_Neutral
}

public class VFXSpawnManager : MonoBehaviour
{
    [Header("New Prefabs")]
    [SerializeField] public GameObject[] visualEffectPrefabs;

    private float currentRotation;
    [SerializeField] private float skyboxRotationSpeed;
    
    // test
    [Header("Test")]
    [SerializeField] private bool turnOnSilhouetteOnLightAttacks;
    [SerializeField] private bool turnOnSilhouetteOnMediumAttacks;

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

    public void InitializeVFX(VFXGraphs graphIndex, Vector3 spawnPos)
    {
        Instantiate(visualEffectPrefabs[(int)graphIndex], spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
    }
    
    public void InitializeVFX(VFXGraphs graphIndex, Vector3 spawnPos, Fighter triggerFighter)
    {
        VisualEffect newVFX = Instantiate(visualEffectPrefabs[(int)graphIndex], spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
        newVFX.GetComponent<VFXCleanUp>().f = triggerFighter;
    }

    public void InitializeVFX(VFXTypes type, Vector3 spawnPos, Fighter triggerFighter)
    {
        VFXGraphs vfxGraph = VFXGraphs.TWT_HIT;
        string charName = Services.Characters[triggerFighter.PlayerId].name;
        switch (type)
        {
            case VFXTypes.Hit_Character:
                vfxGraph = CharacterVFXPickerLight(charName);
                break;
            case VFXTypes.Hit_Special:
                vfxGraph = CharacterVFXPickerLight(charName);
                break;
        }
        InitializeVFX(vfxGraph, spawnPos, triggerFighter);
    }

    public VFXGraphs CharacterVFXPickerLight(string charName)
    {
        switch (charName)
        {
            case "Lisa":
                return VFXGraphs.LISA_HIT;
            case "BLUK":
                return VFXGraphs.BLUK_HIT;
        }

        return VFXGraphs.TWT_HIT;
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
            
            InitializeVFX(VFXGraphs.TWT_HIT, hitPos, sender);
            InitializeVFX(VFXGraphs.RAND_NUTS, hitPos, receiver);
            StartCoroutine(receiver.GetComponent<CharacterVFXManager>().Shake(receiver, 98f, 1f, .8f));
            // camera based on hits
            switch (attackInfo.attackType)
            {
                case AttackInfo.AttackType.Light:
                    StartCoroutine(cam.CameraShake(.2f, .05f));
                    if (turnOnSilhouetteOnLightAttacks)
                    {
                        Material[] mats =
                        {
                            sender.GetComponent<SpriteRenderer>().material, 
                            receiver.GetComponent<SpriteRenderer>().material
                        };
                        cam.SilhouetteToggle(true, mats);
                    }
                    break;
                case AttackInfo.AttackType.Medium:
                    InitializeVFX(VFXTypes.Hit_Character, hitPos, sender);
                    if (turnOnSilhouetteOnMediumAttacks)
                    {
                        Material[] mats =
                        {
                            sender.GetComponent<SpriteRenderer>().material, 
                            receiver.GetComponent<SpriteRenderer>().material
                        };
                        cam.SilhouetteToggle(true, mats);
                    }
                    StartCoroutine(cam.CameraShake(.2f, .08f));
                    break;
                case AttackInfo.AttackType.Heavy:
                    StartCoroutine(cam.CameraShake(.3f, .19f));
                    break;
                case AttackInfo.AttackType.Special:
                    StartCoroutine(cam.CameraZoom(.2f, 36f, .16f, -.45f));
                    StartCoroutine(cam.CameraShake(.3f, .19f));
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
    
    void PlayBlockVFX(Dictionary<string, object> message)
    {
        try
        {
            Vector3 hitPos = (Vector3) message["hit point"];
            Fighter sender = (Fighter) message["attacker"];
            Fighter receiver = (Fighter) message["attacked"];
            InitializeVFX(VFXGraphs.TWT_DEF, hitPos, sender);
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
