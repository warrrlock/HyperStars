using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXGraphs
{
    LISA_HIT_1, LISA_HIT_5, DASH_SMOKE
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
        //newVFX.visualEffectAsset = visualEffectAssets[(int)graphIndex];
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
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }
}
