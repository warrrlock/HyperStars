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

    [Tooltip("attacker, attacked, point hit")]
    public Action<Fighter, Fighter, Vector3> onAttackHit;
    [Tooltip("attacker, blocker, point blocked/hit")]
    public Action<Fighter, Fighter, Vector3> onBlockHit;
}
