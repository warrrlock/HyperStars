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
        foreach (var f in Services.Fighters)
        {
            f.Events.onGoldenGoalEnabled += ctx => { playGoldenGoal.Post(gameObject); };
            f.Events.onGoldenGoalDisabled += ctx => { stopGoldenGoal.Post(gameObject); };
        }
    }
}
