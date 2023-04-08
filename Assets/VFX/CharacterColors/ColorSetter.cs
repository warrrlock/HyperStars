using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSetter : MonoBehaviour
{
    private ColorPicker colorPicker;
    [SerializeField] private ColorSeven newColor;

    [ContextMenu("Update Colors")]
    private void UpdateColors()
    {
        colorPicker = GetComponent<ColorPicker>();
        if (colorPicker != null)
        {
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color1, newColor.Color1);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color2, newColor.Color2);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color3, newColor.Color3);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color4, newColor.Color4);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color5, newColor.Color5);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color6, newColor.Color6);
            colorPicker.characterSprite.material.SetColor(ColorPicker.Color7, newColor.Color7);
        }
    }

    [ContextMenu("Add Color to ColorScriptable")]
    private void AddColor()
    {
        int index = colorPicker.characterColors.Palette.Length;
        colorPicker.characterColors.Palette[index] = newColor;
    }
}
