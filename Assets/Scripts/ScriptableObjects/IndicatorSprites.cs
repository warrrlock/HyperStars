using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Indicator Sprites", menuName = "ScriptableObjects/Indicator Sprites")]
public class IndicatorSprites : ScriptableObject
{
    public Sprite Lisa => _lisa;
    [SerializeField] private Sprite _lisa;
    public Sprite Bluk => _bluk;
    [SerializeField] private Sprite _bluk;
}
