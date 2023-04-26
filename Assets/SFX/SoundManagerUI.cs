using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerUI : MonoBehaviour
{
    // public static SoundManagerUI Instance { get; private set; }
    
    public AK.Wwise.Event lockBreakSound;
    public AK.Wwise.Event playGoldenGoal;
    public AK.Wwise.Event stopGoldenGoal;
    
    void Start()
    {
        // Instance = this;
        Services.RoundTimer.onStartShrink += () => { lockBreakSound.Post(gameObject); };
    }
}
