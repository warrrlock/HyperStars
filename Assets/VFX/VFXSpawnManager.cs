using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXConfigEnums
{
    LISA_HIT_1,
}

public class VFXSpawnManager : MonoBehaviour
{
    [Header("Config Files")]
    [SerializeField] public VisualEffectAsset[] _visualEffectAssets;

    [SerializeField] public GameObject spawnedVfxObject;
    
    // try
    [SerializeField] private Fighter _fighter;

    private void InitializaeVFX(VFXConfigEnums _configIndex, Vector3 hitPos, Fighter receiver)
    {
        VisualEffect newVFX = Instantiate(spawnedVfxObject, hitPos, transform.rotation).GetComponent<VisualEffect>();
        newVFX.visualEffectAsset = _visualEffectAssets[(int)_configIndex];
        newVFX.GetComponent<VFXCleanUp>().receiver = receiver;
    }

    void Start()
    {
        foreach (Fighter fighter in Services.Fighters)
        {
            fighter.Events.onAttackHit += PlayVFX;
        }
        // _fighter.Events.onAttackHit += PlayVFX;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayVFX(Fighter sender, Fighter receiver, Vector3 hitPos)
    {
        InitializaeVFX(VFXConfigEnums.LISA_HIT_1, hitPos, receiver);
    }
}
