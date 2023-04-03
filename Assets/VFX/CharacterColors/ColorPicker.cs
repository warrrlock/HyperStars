using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private int currentColorIndex;
    [SerializeField] private CharacterColorScriptable characterColors;
    private static readonly int Color1 = Shader.PropertyToID("_Color1");
    private static readonly int Color2 = Shader.PropertyToID("_Color2");
    private static readonly int Color3 = Shader.PropertyToID("_Color3");
    private static readonly int Color4 = Shader.PropertyToID("_Color4");
    private static readonly int Color5 = Shader.PropertyToID("_Color5");
    private static readonly int Color6 = Shader.PropertyToID("_Color6");
    private void Awake()
    {
        SetMaterialColors(currentColorIndex);
    }

    public void SetMaterialColors(int index)
    {
        currentColorIndex = index;
        characterSprite.material.SetColor(Color1, characterColors.LisaColors[index].Color1);
        characterSprite.material.SetColor(Color2, characterColors.LisaColors[index].Color2);
        characterSprite.material.SetColor(Color3, characterColors.LisaColors[index].Color3);
        characterSprite.material.SetColor(Color4, characterColors.LisaColors[index].Color4);
        characterSprite.material.SetColor(Color5, characterColors.LisaColors[index].Color5);
        characterSprite.material.SetColor(Color6, characterColors.LisaColors[index].Color6);
    }
}
