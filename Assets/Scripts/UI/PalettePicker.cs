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

        private CharacterColorScriptable _characterPalette;
        
        public void SetColour(int selected)
        {
            int size = _characterPalette.Palette.Count;
            int rawIndex = selected - 1;
            for (int i = 0; i < 3; i++)
            {
                int index = (rawIndex + size) % size;
                Transform icon = _paletteParent.GetChild(i);
                //base
                icon.GetChild(0).GetComponent<Image>().color =
                    _characterPalette.Palette[index].SelectMenuBaseColor;
                //highlight
                icon.GetChild(1).GetComponent<Image>().color =
                    _characterPalette.Palette[index].SelectMenuHighlightColor;
                rawIndex++;
            }
        }

        public void UnsetVisuals()
        {
            _paletteAnimation.Play("hide_palette");
            foreach (Transform child in _paletteParent)
            {
                child.GetChild(2).GetComponent<Image>().sprite = _hasColourBorder;
            }
        }

        public void SetupVisuals(CharacterColorScriptable characterPalette)
        {
            _characterPalette = characterPalette;
            _paletteAnimation.Play("show_palette");
        }

        private void DestroyIcons()
        {
            for (int i = _paletteParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_paletteParent.GetChild(i).gameObject);
            }
        }
    }
}