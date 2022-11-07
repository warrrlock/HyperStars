using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterEvents
{
    public delegate void WallBounce();
    public WallBounce wallBounce;

    public Action onLandedNeutral;
    public Action onLandedHurt;

    [Tooltip("attacker, attacked, hit point, attacker input")]
    public Action<Dictionary<string, object>> onAttackHit;
    [Tooltip("attacker, attacked (blocker), hit point, attacker input")]
    public Action<Dictionary<string, object>> onBlockHit;
}
