using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace UI
{
    public class PauseMenuSelection: MonoBehaviour
    {
        [SerializeField] private GameObject _menu;
        private Fighter _opener;
        private void Start()
        {
            if (_menu) _menu.SetActive(false);
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
            Debug.Log("called");
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
            }
        }

        public void UnpauseGame()
        {
            Debug.Log("unpause");
            DisplayMenuSelection(_opener);
        }
    }
}