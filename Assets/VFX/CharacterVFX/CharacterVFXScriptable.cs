using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character VFX Set")]
public class CharacterVFXScriptable : ScriptableObject
{
    [field: SerializeField] public GameObject[] VFXSet { get; private set; }
}
