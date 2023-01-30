using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXGraphs
{
    LISA_HIT_1, LISA_HIT_5, LISA_HIT_PARRY, DASH_SMOKE, JUMP_SMOKE
}

public class VFXSpawnManager : MonoBehaviour
{
    [SerializeField] public VisualEffectAsset[] visualEffectAssets;
    [SerializeField] public GameObject spawnedVfxObject;
    
    // try

    void Start()
    {
        foreach (Fighter f in Services.Fighters)
        {
            f.Events.onAttackHit += PlayHitVFX;
            f.Events.onBlockHit += PlayBlockVFX;
        }
    }

    private void OnDestroy()
    {
        foreach (Fighter f in Services.Fighters)
        {
            f.Events.onAttackHit -= PlayHitVFX;
            f.Events.onBlockHit -= PlayBlockVFX;
        }
    }

    public void InitializaeVFX(VFXGraphs graphIndex, Vector3 spawnPos)
    {
        VisualEffect newVFX = Instantiate(spawnedVfxObject, spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
        newVFX.visualEffectAsset = visualEffectAssets[(int)graphIndex];
    }
    
    public void InitializaeVFX(VFXGraphs graphIndex, Vector3 spawnPos, Fighter sender)
    {
        VisualEffect newVFX = Instantiate(spawnedVfxObject, spawnPos, Quaternion.identity).GetComponent<VisualEffect>();
        newVFX.visualEffectAsset = visualEffectAssets[(int)graphIndex];
        newVFX.GetComponent<VFXCleanUp>().sender = sender;
    }

    void PlayHitVFX(Dictionary<string, object> message)
    {
        try
        {
            Vector3 hitPos = (Vector3) message["hit point"];
            Fighter sender = (Fighter) message["attacker"];
            Fighter receiver = (Fighter) message["attacked"];
            InitializaeVFX(VFXGraphs.LISA_HIT_1, hitPos, sender);
            StartCoroutine(receiver.GetComponent<CharacterVFXManager>().Shake(receiver, 98f, 1f, .8f));
            // temp camera shake
            StartCoroutine(Camera.main.GetComponent<CameraManager>().CameraShake(.2f, .09f));
            // StartCoroutine(Camera.main.GetComponent<CameraManager>().CameraZoom(hitPos, .08f, 20f, .4f));
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }
    
    void PlayBlockVFX(Dictionary<string, object> message)
    {
        Debug.Log("PARRY VFX");
        try
        {
            Vector3 hitPos = (Vector3) message["hit point"];
            Fighter sender = (Fighter) message["attacker"];
            Fighter receiver = (Fighter) message["attacked"];
            InitializaeVFX(VFXGraphs.LISA_HIT_PARRY, hitPos, sender);
            StartCoroutine(receiver.GetComponent<CharacterVFXManager>().Shake(sender, 98f, 2f, .5f));
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }
}
