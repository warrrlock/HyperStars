using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PalettePicker: MonoBehaviour
    {
        [SerializeField] private Transform _paletteParent;
        [SerializeField] private Animator _paletteAnimation;
        [SerializeField] private Sprite _noColourSprite;
        [SerializeField] private Sprite _hasColourBorder;
        [SerializeField] private Image _characterImage;

        private CharacterColorScriptable _characterPalette;

        private void OnDestroy()
        {
            SetMaterialColours(0);
        }

        public void SetupVisuals(CharacterColorScriptable characterPalette)
        {
            // Debug.Log($"setting up visuals for {characterPalette.name}");
            _characterPalette = characterPalette;
            _paletteAnimation.Play("show_palette");
        }
        
        public void SetColour(int selected, int player)
        {
            SetMaterialColours(selected);
            int size = _characterPalette.Palette.Count;
            bool[] seen = new bool[size];
            
            //middle
            int rawIndex = selected;
            int index = GetIndex(rawIndex, size);
            Transform icon = _paletteParent.GetChild(1);
            SetIconColours(icon, index);
            CheckHasNoColour(ref seen, index, icon, player);
            //above
            rawIndex = selected + 1;
            index = GetIndex(rawIndex, size);
            icon = _paletteParent.GetChild(0);
            SetIconColours(icon, index);
            CheckHasNoColour(ref seen, index, icon, player);

            //below
            rawIndex = selected - 1;
            index = GetIndex(rawIndex, size);
            icon = _paletteParent.GetChild(2);
            SetIconColours(icon, index);
            CheckHasNoColour(ref seen, index, icon, player);
        }

        private void CheckHasNoColour(ref bool[] seen, int index, Transform icon, int player)
        {
            Image img = icon.GetChild(2).GetComponent<Image>();
            if (seen[index] || !ColourAvailable(player, index))
            {
                img.sprite = _noColourSprite;
            }
            else img.sprite = _hasColourBorder;
            seen[index] = true;
        }
        
        private bool ColourAvailable(int player, int index)
        {
            return Services.Characters[player ^ 1] != Services.Characters[player] 
                   || Services.Players[player ^ 1].PaletteIndex != index;
        }

        private void SetIconColours(Transform icon, int index)
        {
            //base
            icon.GetChild(0).GetComponent<Image>().color =
                _characterPalette.Palette[index].SelectMenuBaseColor;
            //highlight
            icon.GetChild(1).GetComponent<Image>().color =
                _characterPalette.Palette[index].SelectMenuHighlightColor;
        }

        private int GetIndex(int rawIndex, int size)
        {
            return (rawIndex + size) % size;
        }

        public void UnsetVisuals()
        {
            _paletteAnimation.Play("hide_palette");
            foreach (Transform child in _paletteParent)
            {
                child.GetChild(2).GetComponent<Image>().sprite = _hasColourBorder;
            }
        }

        private void DestroyIcons()
        {
            for (int i = _paletteParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_paletteParent.GetChild(i).gameObject);
            }
        }

        
        private static readonly int Color1 = Shader.PropertyToID("_Color1");
        private static readonly int Color2 = Shader.PropertyToID("_Color2");
        private static readonly int Color3 = Shader.PropertyToID("_Color3");
        private static readonly int Color4 = Shader.PropertyToID("_Color4");
        private static readonly int Color5 = Shader.PropertyToID("_Color5");
        private static readonly int Color6 = Shader.PropertyToID("_Color6");
        private static readonly int Color7 = Shader.PropertyToID("_Color7");
        private void SetMaterialColours(int index)
        {
            if (!_characterPalette) return;
            // Debug.Log($"checking null: image{_characterImage}, palette: {_characterPalette}");
            _characterImage.material.SetColor(Color1, _characterPalette.Palette[index].Color1);
            _characterImage.material.SetColor(Color2, _characterPalette.Palette[index].Color2);
            _characterImage.material.SetColor(Color3, _characterPalette.Palette[index].Color3);
            _characterImage.material.SetColor(Color4, _characterPalette.Palette[index].Color4);
            _characterImage.material.SetColor(Color5, _characterPalette.Palette[index].Color5);
            _characterImage.material.SetColor(Color6, _characterPalette.Palette[index].Color6);
            _characterImage.material.SetColor(Color7, _characterPalette.Palette[index].Color7);
        }
    }
}