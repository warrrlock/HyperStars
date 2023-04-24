using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenuSelection: MonoBehaviour
    {
        [Serializable]
        private class TabAssets
        {
            public Button tabButton;
            public Button firstSelected;
            public GameObject page;
        }

        [SerializeField] private TabAssets[] _tabAssets;
        [SerializeField] private GameObject _menu;
        private Fighter _opener;
        private InputSystemUIInputModule _inputModule;
        private int _currentTab;
            
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

        public void NavigateInDirection(int dir)
        {
            int tab = Math.Clamp(_currentTab+dir, 0, _tabAssets.Length - 1);
            SetToTab(tab);
        }
        
        public void SetToTab(int tab)
        {
            _tabAssets[_currentTab].tabButton.animator.Play("Normal");
            _tabAssets[_currentTab].page.SetActive(false);
            
            _currentTab = tab;
            _tabAssets[_currentTab].page.SetActive(true);
            _tabAssets[_currentTab].tabButton.animator.Play("Selected");
            
            EventSystem.current.SetSelectedGameObject(_tabAssets[_currentTab].firstSelected.gameObject);
        }

        private void SubscribeEvents()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.InputManager.Actions["Esc"].perform += (a) => DisplayMenuSelection(fighter);
                fighter.InputManager.Actions["Cancel"].perform += (a) => DisplayMenuSelection(fighter);
            }
        }

        private void UnsubscribeEvents()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.InputManager.Actions["Esc"].perform -= (a) => DisplayMenuSelection(fighter);
                fighter.InputManager.Actions["Cancel"].perform -= (a) => DisplayMenuSelection(fighter);
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
                foreach (Fighter fighter in Services.Fighters)
                    fighter.PlayerInput.SwitchCurrentActionMap(fighter.PlayerInput.defaultActionMap);

                _menu.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                _opener = f;
                Time.timeScale = 0;
                foreach (Fighter fighter in Services.Fighters)
                    fighter.PlayerInput.SwitchCurrentActionMap("UI");

                _menu.SetActive(true);
                _opener.PlayerInput.uiInputModule = _menu.GetComponent<InputSystemUIInputModule>();
                SetToTab(0);
            }
        }

        public void UnpauseGame()
        {
            // Debug.Log("unpause");
            DisplayMenuSelection(_opener);
        }
    }
}