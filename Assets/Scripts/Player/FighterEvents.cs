using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;

public class FighterEvents
{
    public delegate void WallBounce();
    public WallBounce wallBounce;
    public delegate void GroundBounce();
    public GroundBounce groundBounce;

    public Action onLandedNeutral;
    public Action onLandedHurt;

    [Tooltip("Fighter attacker, Fighter attacked, Vector3 hit point, string: attacker input, AttackInfo attackInfo")]
    public Action<Dictionary<string, object>> onAttackHit;
    [Tooltip("Fighter attacker, Fighter attacked (blocker), Vector3 hit point, string: attacker input, AttackInfo attackInfo")]
    public Action<Dictionary<string, object>> onBlockHit;
    
    [Tooltip("Get state name with state.name")]
    public Action<BaseState> onStateChange;
}
