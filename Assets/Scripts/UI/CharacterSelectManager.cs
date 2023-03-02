using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace UI
{
    [Serializable]
    class CharacterButtonPair
    {
        public CharacterManager.CharacterSelection character;
        public CharacterButtonAssets button;
    }

    public class CharacterSelectManager: MonoBehaviour
    {
        [SerializeField] private Sprite[] _singleSelectSprites = new Sprite[2];
        [SerializeField] private Sprite _doubleSelectSprite;
        [SerializeField] private List<CharacterButtonPair> _characterButtons;
        private CharacterButtonAssets[] _currentSelectedButton = new CharacterButtonAssets[2];
        
        public void UpdateSelection(CharacterManager.CharacterSelection button, int player)
        {
            CharacterButtonPair buttonPair = _characterButtons.Find(c => c.character == button);
            buttonPair.button.animator.Play("Selected");
            
            if (_currentSelectedButton[player])
            {
                _currentSelectedButton[player].players[player] = false;
                UpdateButtonVisual(_currentSelectedButton[player], player^1);
            }
            
            _currentSelectedButton[player] = buttonPair.button;
            _currentSelectedButton[player].players[player] = true;
            UpdateButtonVisual(_currentSelectedButton[player], player);
        }
        
        private void UpdateButtonVisual(CharacterButtonAssets button, int player)
        {
            // Debug.Log($"player 0: {button.players[0]} player 1: {button.players[1]}");
            if (!button.players[0] && !button.players[1])
            {
                button.animator.Play("Normal");
                return;
            }
            
            button.border.sprite = (button.players[0] && button.players[1]) ? _doubleSelectSprite : _singleSelectSprites[player];
        }
    }
}