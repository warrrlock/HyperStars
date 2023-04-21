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

    public Action onHardKnockdown;
    public Action exitHardKnockdown;
    
    [Tooltip("[Fighter] attacker, [Fighter] attacked, [Vector3] hit point, [string] attacker input, " +
             "[AttackInfo] attack info, [AttackInfo.AttackType] attack type")]
    public Action<Dictionary<string, object>> onAttackHit;
    [Tooltip("[Fighter] attacker, [Fighter] attacked (blocker), [Vector3] hit point, [string] 'attacker input', " +
             "[AttackInfo] attack info, [AttackInfo.AttackType] attack type")]
    public Action<Dictionary<string, object>> onBlockHit;
    
    [Tooltip("Get state name with state.name")]
    public Action<BaseState> onStateChange;
}
