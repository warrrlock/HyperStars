using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ColorSetter))]
public class ColorPicker : MonoBehaviour
{
    [SerializeField] public SpriteRenderer characterSprite;
    [SerializeField] public int currentColorIndex;
    [SerializeField] public CharacterColorScriptable characterColors;
    public static readonly int Color1 = Shader.PropertyToID("_Color1");
    public static readonly int Color2 = Shader.PropertyToID("_Color2");
    public static readonly int Color3 = Shader.PropertyToID("_Color3");
    public static readonly int Color4 = Shader.PropertyToID("_Color4");
    public static readonly int Color5 = Shader.PropertyToID("_Color5");
    public static readonly int Color6 = Shader.PropertyToID("_Color6");
    public static readonly int Color7 = Shader.PropertyToID("_Color7");
    
    private void Awake()
    {
        SetMaterialColors(currentColorIndex);
    }

    public void SetMaterialColors(int index)
    {
        currentColorIndex = index;
        characterSprite.material.SetColor(Color1, characterColors.Palette[index].Color1);
        characterSprite.material.SetColor(Color2, characterColors.Palette[index].Color2);
        characterSprite.material.SetColor(Color3, characterColors.Palette[index].Color3);
        characterSprite.material.SetColor(Color4, characterColors.Palette[index].Color4);
        characterSprite.material.SetColor(Color5, characterColors.Palette[index].Color5);
        characterSprite.material.SetColor(Color6, characterColors.Palette[index].Color6);
        characterSprite.material.SetColor(Color7, characterColors.Palette[index].Color7);
    }
}
