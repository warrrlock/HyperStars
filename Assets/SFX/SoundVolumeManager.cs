using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVolumeManager : MonoBehaviour
{
    [Range(0, 1)] public float masterVolume = 1;
    [Range(0, 1)] public float musicVolume = 1;
    [Range(0, 1)] public float crowdVolume = 1;
    [Range(0, 1)] public float sfxVolume = 1;
    [Range(0, 1)] public float voVolume = 1;
    public static float currentCrowdVolume = 0;

    void Awake()
    {
        Services.SoundVolumeManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        AkSoundEngine.SetRTPCValue("MasterVolume", masterVolume);
        AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
        AkSoundEngine.SetRTPCValue("CrowdVolume", currentCrowdVolume); 
        AkSoundEngine.SetRTPCValue("SFXVolume", sfxVolume);
        AkSoundEngine.SetRTPCValue("VoiceVolume", voVolume); 
    }
}
