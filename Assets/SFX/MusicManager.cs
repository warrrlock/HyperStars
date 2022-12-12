using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MusicManager : MonoBehaviour
{
    public AK.Wwise.Event MusicTrack;
    public AK.Wwise.Event CrowdLoop;
    public AK.Wwise.Event CrowdHype;
    
    [HideInInspector] public static GameObject ourMusicManager;
    [Range(1, 3)]
    public int Intensity;
    [Range(0, 1)]
    public float musicVolume;
    [Range(0, 1)]
    public float crowdVolume;
    
    //id of the wwise event - using this to get the playback position
    private uint playingIDGlobal;
    
    private bool inVerse;
    private bool inChorus;
    private static bool lockedOut;
    public bool Impressed;
    private bool impressionCalled;

    public bool increaseIntensityDuringChorus;
    private bool intensityIncremented;
    private static bool increaseIntensityAfterChorus;
    public bool DebugMode;
    
    // public AK.Wwise.Event musicStart;
    
    // music logics
    [HideInInspector] public int hitCountP1;
    [HideInInspector] public int hitCountP2;
    private int hitCountTotal;
    [SerializeField] private int intensityTierTwoCountRequirement;
    [SerializeField] private int intensityTierThreeCountRequirement;
    private float _timer;
    [SerializeField] private float hitEffectiveTime;
    [HideInInspector] public int thisHit; // 0 = p1; 1 = p2;
    private int lastHit;
    private int lastSamePlayerCombo;
    private int currentSamePlayerCombo;
    
    void Start()
    {
        //Set our Music Manager
        ourMusicManager = gameObject;
        increaseIntensityAfterChorus = increaseIntensityDuringChorus;
        //Start music 
        AkSoundEngine.SetRTPCValue("Intensity", Intensity); //Set Intensity
        AkSoundEngine.SetSwitch("MusicState", "Verse", gameObject); //Set which section to play
        //playingIDGlobal =  //Start Music
        playingIDGlobal = MusicTrack.Post(gameObject, (uint)(AkCallbackType.AK_MusicSyncAll | AkCallbackType.AK_EnableGetMusicPlayPosition), MusicCallbackFunction);
        CrowdLoop.Post(gameObject);
        
        // set timer
        _timer = hitEffectiveTime;
    }

    // Update is called once per frame
    void Update()
    {
        //Update our volume 
        AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
        AkSoundEngine.SetRTPCValue("CrowdVolume", crowdVolume); 
        AkSoundEngine.SetRTPCValue("Intensity", Intensity); //Set Intensity

        if (Impressed && !impressionCalled)
        {
            StartChorus(gameObject);
            impressionCalled = true;
        }

        if (DebugMode)
        {
            Test();
        }

        MusicChange();
        
    }

    public static void StartChorus(GameObject musicManager)
    {
        if (lockedOut)
        {
            return;
        }
        else
        {
            AkSoundEngine.SetSwitch("MusicState", "chorus", musicManager); //Set which section to play
            //Declare our music manager
            MusicManager ourMusicComponent = musicManager.GetComponent<MusicManager>();
            //Set Values
            ourMusicComponent.inVerse = false;
            ourMusicComponent.inChorus = true;

            if (increaseIntensityAfterChorus)
            {
                if (ourMusicComponent.Intensity < 3 && !ourMusicComponent.intensityIncremented)
                {
                    ourMusicComponent.Intensity++;
                    ourMusicComponent.intensityIncremented = true;
                }
            }
        }
    }
    
    
    public static void StartVerse(GameObject musicManager)
    {
        AkSoundEngine.SetSwitch("MusicState", "verse", musicManager); //Set which section to play
        //Declare our music manager
        MusicManager ourMusicComponent = musicManager.GetComponent<MusicManager>();
        //Set Values
        ourMusicComponent.inVerse = true;
        ourMusicComponent.inChorus = false;
    }


    public static void IncreaseIntensity(GameObject musicManager)
    {
        MusicManager ourMusicComponent = musicManager.GetComponent<MusicManager>();
        if (ourMusicComponent.Intensity < 3)
        {
            ourMusicComponent.Intensity++;
        }
    }
    
    public static void DecreaseIntensity(GameObject musicManager)
    {
        MusicManager ourMusicComponent = musicManager.GetComponent<MusicManager>();
        if (ourMusicComponent.Intensity > 1)
        {
            ourMusicComponent.Intensity--;
        }
    }

    private void Test()
    {
        if (Input.GetKeyDown("o"))
        {
            StartChorus(ourMusicManager);
        }
        
        if (Input.GetKeyDown("p"))
        {
            IncreaseIntensity(gameObject);
        }
        
        if (Input.GetKeyDown("i"))
        {
            DecreaseIntensity(gameObject);
        }
    }
    
    void MusicCallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        AkMusicSyncCallbackInfo _musicInfo = (AkMusicSyncCallbackInfo) in_info;
        switch (_musicInfo.musicSyncType)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                CustomCues(_musicInfo.userCueName, _musicInfo);
                break;
        }
    }
    
    public void CustomCues(string cueName, AkMusicSyncCallbackInfo _musicInfo)
    {
        switch (cueName)
        {
            case "LockOut":
                Debug.Log("LockOut");
                lockedOut = true;
                Impressed = true;
                break;
            case "End":
                Debug.Log("Ended");
                lockedOut = false;
                break;
            case "Finished":
                Debug.Log("Ended");
                lockedOut = false;
                Impressed = false;
                impressionCalled = false;
                intensityIncremented = false;
                break;
        }
    }
    
    
    // interactive music
    void MusicChange()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            hitCountP1 = 0;
            hitCountP2 = 0;
        }
        hitCountTotal = hitCountP1 + hitCountP2;

        switch (hitCountTotal)
        {
            case var hitCountTotal when hitCountTotal < intensityTierTwoCountRequirement:
                Intensity = 1;
                crowdVolume = .25f;
                break;
            case var hitCountTotal when hitCountTotal < intensityTierThreeCountRequirement:
                Intensity = 2;
                crowdVolume = .6f;
                break;
            default:
                Intensity = 3;
                crowdVolume = 1f;
                break;
        }
    }

    public void ResetHitEffectiveTimer()
    {
        _timer = hitEffectiveTime;
    }

    public void CheckCombo()
    {
        if (thisHit == lastHit)
        {
            currentSamePlayerCombo++;
        }
        else
        {
            lastSamePlayerCombo = currentSamePlayerCombo;
            currentSamePlayerCombo = 0;
        }

        if (lastSamePlayerCombo > 5 && currentSamePlayerCombo >= 3)
        {
            Impressed = true;
            CrowdHype.Post(gameObject);
        }

        lastHit = thisHit;
    }
}
