using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Wwise_CombatEnvironmentSounds {
    GroundSplat, WallBounce
}

public enum Wwise_CombatSwings
{
    LightSwing1, LightSwing2, MediumSwing1, MediumSwing2, ParrySwing
}

public enum Wwise_CombatHits {
    Light, Medium, Heavy
}

public class SoundManagerCombat : MonoBehaviour
{
    public static SoundManagerCombat Instance { get; private set; }
    
    public AK.Wwise.Event[] swingEvents;
    public AK.Wwise.Event[] hitEvents;
    public AK.Wwise.Event[] environmentHits;
    public AK.Wwise.Event blockHit;
    
    void Awake()
    {
        Instance = this;
    }
}
