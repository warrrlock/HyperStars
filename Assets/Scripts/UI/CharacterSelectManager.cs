using System;
using System.Collections.Generic;
using Managers;
using SFX;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Util;

namespace UI
{
    [Serializable]
    class CharacterButtonPair
    {
        public CharacterManager.CharacterSelection character;
        public CharacterButtonAssets button;
    }

    [RequireComponent(typeof(PostWwiseUIEvent))]
    public class CharacterSelectManager: MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private TextMeshProUGUI[] _playerCharacterNames;
        [SerializeField] private Sprite[] _singleSelectSprites = new Sprite[2];
        [SerializeField] private Sprite _doubleSelectSprite;
        
        [Header("References")]
        [SerializeField] private UIInputManager[] _playerInputManagers;
        [SerializeField] private CharacterButtonsPlayer[] _playerButtons = new CharacterButtonsPlayer[2];
        [SerializeField] private List<CharacterButtonPair> _characterButtons;
        
        private CharacterButtonAssets[] _currentSelectedButton = new CharacterButtonAssets[2];
        [SerializeField] private BuildSettingIndices _indices;
        private bool _isTraining;
        private PostWwiseUIEvent _wwiseUIEvent;
        public PostWwiseUIEvent WwiseUIEvents => _wwiseUIEvent;

        private void Start()
        {
            _wwiseUIEvent = GetComponent<PostWwiseUIEvent>();
            if (SceneManager.GetActiveScene().buildIndex == _indices.trainingSelectionScene)
            {
                _isTraining = true;
                Services.Players[0].onReady += SetBotSelection;
            }
        }

        private void OnDestroy()
        {
            if (_isTraining)
            {
                Services.Players[0].onReady -= SetBotSelection;
            }
        }

        public void UpdateSelection(string character, CharacterManager.CharacterSelection button, int player)
        {
            _playerCharacterNames[player].text = character;
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
        
        public void SetBotSelection()
        {
            // Debug.Log("set bot selection");
            _playerInputManagers[0].SetBotSelection();
            CharacterButtonsPlayer selection = _playerButtons[1];
            
            selection.SetPlayer(Services.Players[0]);
            InputSystemUIInputModule input = selection.GetComponent<InputSystemUIInputModule>();
            input.AssignDefaultActions();
            Services.Players[0].PlayerInput.uiInputModule = input;
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