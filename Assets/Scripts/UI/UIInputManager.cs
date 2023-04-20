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
        [SerializeField] private CharacterSelectManager _selectManager;
        private bool _selectingForBot;

        private void OnDestroy()
        {
            UnSubscribe();
        }

        public void Initialize(Player player)
        {
            _player = player;
            Subscribe();
        }

        public void SetBotSelection()
        {
            _selectingForBot = true;
        }

        private void Subscribe()
        {
            _player.PlayerInput.actions["Cancel"].performed += ResolveActions;
            _player.PlayerInput.actions["Submit"].performed += ResolveActions;
            _player.PlayerInput.actions["Navigate"].performed += ResolveActions;
        }

        private void UnSubscribe()
        {
            _player.PlayerInput.actions["Cancel"].performed -= ResolveActions;
            _player.PlayerInput.actions["Submit"].performed -= ResolveActions;
            _player.PlayerInput.actions["Navigate"].performed -= ResolveActions;
        }

        private void ResolveActions(InputAction.CallbackContext context)
        {
            if (!_player.PlayerInput.currentActionMap.Contains(context.action))
            {
                Debug.Log($"{_player.PlayerInput.currentActionMap.name} does not contain action {context.action.name} ");
                return;
            }

            if (!context.action.WasPressedThisFrame()) return;

            Player player = _selectingForBot ?  Services.Players[PlayerId ^ 1] : _player;
            int playerId = _selectingForBot ? PlayerId ^ 1 : PlayerId;
            
            if (player.Ready)
            {
                // Debug.Log($"player {player.PlayerInput.playerIndex}  is ready");
                switch (context.action.name)
                {
                    case "Submit":
                        if (_menu.IsTraining) _menu.StartTraining();
                        else _menu.StartGame();
                        break;

                    case "Cancel":
                        player.UnReady();
                        break;
                }
                return;
            }
            
            if (player.CharacterSelected)
            {
                // Debug.Log($"player {player.PlayerInput.playerIndex} is character selected");
                switch (context.action.name)
                {
                    case "Navigate":
                        int yDirection = (int)context.action.ReadValue<Vector2>().y;
                        if (yDirection == 0) return;
                        int size = Services.Characters[playerId].CharacterPalettes.Palette.Count;
                        int index = (player.PaletteIndex + yDirection + size) % size;
                        _paletteManager.SetColour(playerId, index, yDirection, size);
                        break;

                    case "Submit":
                        _paletteManager.ConfirmColour(playerId);
                        break;

                    case "Cancel":
                        if (_selectingForBot)
                            _selectManager.SetBotSelection();
                        player.CloseColourPicker(_selectingForBot);
                        break;
                }
                return;
            }
            
            // Debug.Log($"player {player.PlayerInput.playerIndex} is neither ready or character selected");
            switch (context.action.name)
            {
                case "Cancel":
                    if (_selectingForBot)
                    {
                        _selectingForBot = false;
                        _player.UnReady();
                    }
                    else _menu.StartMainMenu();
                    break;
            }
        }
    }
}