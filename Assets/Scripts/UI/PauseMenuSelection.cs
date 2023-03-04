using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace UI
{
    public class PauseMenuSelection: MonoBehaviour
    {
        [SerializeField] private GameObject _menu;
        private Fighter _opener;
        private InputSystemUIInputModule _inputModule;
            
        private void Start()
        {
            if (_menu)
            {
                _menu.SetActive(false);
                _inputModule = _menu.GetComponent<InputSystemUIInputModule>();
            }
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.InputManager.Actions["Esc"].perform += (a) => DisplayMenuSelection(fighter);
            }
        }
        
        private void UnsubscribeEvents()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.InputManager.Actions["Esc"].perform -= (a) => DisplayMenuSelection(fighter);
            }
        }

        private void DisplayMenuSelection(Fighter f)
        {
            // Debug.Log("opening pause menu");
            if (!_menu) return;
            if (_opener && f != _opener) return;
            
            if (_menu.activeSelf)
            {
                _opener.PlayerInput.uiInputModule = null;
                _opener = null;
                _menu.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                _opener = f;
                Time.timeScale = 0;
                _menu.SetActive(true);
                _opener.PlayerInput.uiInputModule = _menu.GetComponent<InputSystemUIInputModule>();
                
                foreach (Fighter fighter in Services.Fighters)
                {
                    StartCoroutine(fighter.DisableAllInput(() => !_menu.activeSelf));
                }
            }
        }

        public void UnpauseGame()
        {
            // Debug.Log("unpause");
            DisplayMenuSelection(_opener);
        }
    }
}