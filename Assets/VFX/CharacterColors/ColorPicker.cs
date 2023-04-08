using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] public SpriteRenderer characterSprite;
    [SerializeField] public CharacterColorScriptable characterColors;
    [SerializeField] public int currentColorIndex;
    private static readonly int Color1 = Shader.PropertyToID("_Color1");
    private static readonly int Color2 = Shader.PropertyToID("_Color2");
    private static readonly int Color3 = Shader.PropertyToID("_Color3");
    private static readonly int Color4 = Shader.PropertyToID("_Color4");
    private static readonly int Color5 = Shader.PropertyToID("_Color5");
    private static readonly int Color6 = Shader.PropertyToID("_Color6");
    private static readonly int Color7 = Shader.PropertyToID("_Color7");
    
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
    
    // setting color in editor
    [SerializeField] private ColorSeven newColor;

    [ContextMenu("Update Color")]
    private void UpdateColor()
    {
        characterSprite.sharedMaterial.SetColor(Color1, newColor.Color1);
        characterSprite.sharedMaterial.SetColor(Color2, newColor.Color2);
        characterSprite.sharedMaterial.SetColor(Color3, newColor.Color3);
        characterSprite.sharedMaterial.SetColor(Color4, newColor.Color4);
        characterSprite.sharedMaterial.SetColor(Color5, newColor.Color5);
        characterSprite.sharedMaterial.SetColor(Color6, newColor.Color6);
        characterSprite.sharedMaterial.SetColor(Color7, newColor.Color7);
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    [ContextMenu("Update Color to Current Index")]
    private void UpdateColorToIndex()
    {
        characterSprite.sharedMaterial.SetColor(Color1, characterColors.Palette[currentColorIndex].Color1);
        characterSprite.sharedMaterial.SetColor(Color2, characterColors.Palette[currentColorIndex].Color2);
        characterSprite.sharedMaterial.SetColor(Color3, characterColors.Palette[currentColorIndex].Color3);
        characterSprite.sharedMaterial.SetColor(Color4, characterColors.Palette[currentColorIndex].Color4);
        characterSprite.sharedMaterial.SetColor(Color5, characterColors.Palette[currentColorIndex].Color5);
        characterSprite.sharedMaterial.SetColor(Color6, characterColors.Palette[currentColorIndex].Color6);
        characterSprite.sharedMaterial.SetColor(Color7, characterColors.Palette[currentColorIndex].Color7);
    }

    [ContextMenu("Add Color to Scriptable")]
    private void AddColor()
    {
        characterColors.Palette.Add(newColor);
    }
}
