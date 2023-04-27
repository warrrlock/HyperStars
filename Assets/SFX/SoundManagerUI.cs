using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerUI : MonoBehaviour
{
    // public static SoundManagerUI Instance { get; private set; }
    
    [Header("Lock Break")]
    public AK.Wwise.Event playPreLockBreakSound;
    public AK.Wwise.Event stopPreLockBreakSound;
    public AK.Wwise.Event lockBreakSound;
    [Header("Golden Goal")]
    public AK.Wwise.Event playGoldenGoal;
    public AK.Wwise.Event stopGoldenGoal;
    
    
    void Start()
    {
        // Instance = this;
        Services.RoundTimer.onStartShrink += () => { lockBreakSound.Post(gameObject); };
        Services.FavorManager.onGoldenGoalEnabled += ctx => { playGoldenGoal.Post(gameObject); };
        Services.FavorManager.onGoldenGoalDisabled += ctx => { stopGoldenGoal.Post(gameObject); };
        Services.RoundTimer.onStartFuse += () => { playPreLockBreakSound.Post(gameObject); };
    }

    public void StopUISoundsOnEnd()
    {
        stopGoldenGoal.Post(gameObject);
        stopPreLockBreakSound.Post(gameObject);
    }
}
