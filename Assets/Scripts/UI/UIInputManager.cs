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

            if (!context.performed) return;
            
            switch (context.action.name)
            {
                case "Submit":
                    // Debug.Log("submitting");
                    _menu.StartGame();
                    break;
                case "Cancel":
                    // Debug.Log("cancelling");
                    if (_player.Ready) _player.UnReady();
                    else _menu.StartMainMenu();
                    break;
            }
        }
    }
}