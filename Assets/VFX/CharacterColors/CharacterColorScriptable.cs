using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterColors")]
public class CharacterColorScriptable : ScriptableObject
{
    [field: SerializeField] public ColorSix[] LisaColors { private set; get; }
}

[Serializable] public class ColorSix
{
    [field: SerializeField] public Color Color1 { private set; get; }
    [field: SerializeField] public Color Color2 { private set; get; }
    [field: SerializeField] public Color Color3 { private set; get; }
    [field: SerializeField] public Color Color4 { private set; get; }
    [field: SerializeField] public Color Color5 { private set; get; }
    [field: SerializeField] public Color Color6 { private set; get; }
    [field: SerializeField] public Gradient EffectColor { private set; get; }
}
