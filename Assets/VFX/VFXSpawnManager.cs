using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXGraphs
{
    LISA_HIT_1, DASH_SMOKE
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
        newVFX.visualEffectAsset = visualEffectAssets[(int)graphIndex];
        newVFX.GetComponent<VFXCleanUp>().sender = sender;
    }

    void PlayHitVFX(Fighter sender, Fighter receiver, Vector3 hitPos)
    {
        InitializaeVFX(VFXGraphs.LISA_HIT_1, hitPos, sender);
    }
}
