using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMeter : MonoBehaviour
{
    [SerializeField] private int _player;
    private void Start()
    {
        Services.Fighters[_player-1].SpecialMeterManager.FillMeter = GetComponent<Image>();
    }
}
