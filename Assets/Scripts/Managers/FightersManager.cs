using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightersManager : MonoBehaviour
{
    public int hurtboxLayer;

    private Fighter[] _fighters;

    private void Awake()
    {
        Services.FightersManager = this;
    }
}
