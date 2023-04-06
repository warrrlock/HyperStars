using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character Colors")]
public class CharacterColorScriptable : ScriptableObject
{
    [field: SerializeField] public ColorSix[] Palette { private set; get; }
}

[Serializable] public class ColorSix
{
    [field: SerializeField] public Color SelectMenuColor { private set; get; }
    [field: Space(10)]
    [field: SerializeField] public Color Color1 { private set; get; }
    [field: SerializeField] public Color Color2 { private set; get; }
    [field: SerializeField] public Color Color3 { private set; get; }
    [field: SerializeField] public Color Color4 { private set; get; }
    [field: SerializeField] public Color Color5 { private set; get; }
    [field: SerializeField] public Color Color6 { private set; get; }
    [field: SerializeField, GradientUsage(true)] public Gradient EffectColor { private set; get; }
}
