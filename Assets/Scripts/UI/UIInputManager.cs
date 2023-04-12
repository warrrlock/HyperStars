using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class UIInputManager: MonoBehaviour
    {
        [SerializeField] public int PlayerId;
        [SerializeField] private MenuManager _menu;
        private Player _player;
        [SerializeField] private PalettePickerManager _paletteManager;

        private void OnDestroy()
        {
            UnSubscribe();
        }

        public void Initialize(Player player)
        {
            _player = player;
            Subscribe();
        }

        private void Subscribe()
        {
            _player.PlayerInput.onActionTriggered += ResolveActions;
        }

        private void UnSubscribe()
        {
            _player.PlayerInput.onActionTriggered -= ResolveActions;
        }

        private void ResolveActions(InputAction.CallbackContext context)
        {
            if (!_player.PlayerInput.currentActionMap.Contains(context.action))
            {
                Debug.Log($"{_player.PlayerInput.currentActionMap.name} does not contain action {context.action.name} ");
                return;
            }

            if (!context.action.WasPerformedThisFrame()) return;
            
            switch (context.action.name)
            {
                case "Submit":
                    // Debug.Log("submitting");
                    if (_menu.IsTraining) _menu.StartTraining();
                    else _menu.StartGame();
                    break;
                case "Cancel":
                    // Debug.Log("cancelling");
                    if (_player.Ready) _player.UnReady();
                    else _menu.StartMainMenu();
                    break;
            }

            if (!_player.Ready) return;
            switch (context.action.name)
            {
                case "Navigate":
                    int yDirection = (int)context.action.ReadValue<Vector2>().y;
                    if (yDirection == 0) return;
                    int size = Services.Characters[PlayerId].CharacterPalettes.Palette.Count;
                    int index = (_player.PaletteIndex + yDirection + size) % size;
                    _paletteManager.SetColour(PlayerId, index);
                    Debug.Log($"navigate {index}");
                    break;
            }
        }
    }
}