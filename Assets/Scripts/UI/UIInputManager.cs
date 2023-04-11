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
        private bool _selectingForBot;
        public bool IgnoreNextInput { get; set; }

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
            _selectingForBot = !_selectingForBot;
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
            
            if (IgnoreNextInput)
            {
                IgnoreNextInput = false;
                return;
            }
   
            Player player = _selectingForBot ?  Services.Players[PlayerId ^ 1] : _player;
            if (player.Ready)
            {
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
            }
            
            if (player.CharacterSelected)
            {
                switch (context.action.name)
                {
                    case "Navigate":
                        int yDirection = (int)context.action.ReadValue<Vector2>().y;
                        if (yDirection == 0) return;
                        
                        int size = Services.Characters[PlayerId].CharacterPalettes.Palette.Count;
                        int index = (player.PaletteIndex + yDirection + size) % size;
                        _paletteManager.SetColour(_selectingForBot ? PlayerId ^ 1 : PlayerId, index, yDirection, size);
                        break;

                    case "Submit":
                        _paletteManager.ConfirmColour(_selectingForBot ? PlayerId ^ 1 : PlayerId);
                        break;

                    case "Cancel":
                        player.CloseColourPicker();
                        break;
                }
                return;
            }
            
            switch (context.action.name)
            {
                case "Cancel":
                    _menu.StartMainMenu();
                    break;
            }
        }
    }
}