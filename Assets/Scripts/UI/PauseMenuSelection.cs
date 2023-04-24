using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
            for (int i = 0; i < 2; i++)
            {
                Services.Fighters[i].PlayerInput.actions["Esc"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                Services.Fighters[i].PlayerInput.actions["Cancel"].performed += (i == 0 ? DisplayP1 : DisplayP2);
            }
        }

        private void UnsubscribeEvents()
        {
            for (int i = 0; i < 2; i++)
            {
                Services.Fighters[i].PlayerInput.actions["Esc"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
                Services.Fighters[i].PlayerInput.actions["Cancel"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
            }
        }

        private void DisplayP1(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            Fighter fighter = Services.Fighters[0];
            Debug.Log($"fighter {fighter.PlayerInput.currentActionMap}");
            
            DisplayMenuSelection(fighter);
        }
        
        private void DisplayP2(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            Fighter fighter = Services.Fighters[1];
            Debug.Log($"fighter {fighter.PlayerInput.currentActionMap}");
            DisplayMenuSelection(fighter);
        }

        private void DisplayMenuSelection(Fighter f)
        {
            Debug.Log($"{f.name} opening pause menu");
            if (!_menu) return;
            if (_opener && f != _opener) return;
            
            if (_menu.activeSelf)
            {
                if (!_opener) return;
                _opener.PlayerInput.uiInputModule = null;
                _opener = null;
                foreach (Fighter fighter in Services.Fighters)
                {
                    fighter.PlayerInput.currentActionMap.Disable();
                    fighter.PlayerInput.ActivateInput();
                    fighter.PlayerInput.SwitchCurrentActionMap(fighter.PlayerInput.defaultActionMap);
                }

                ResetValues();
                _menu.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                _opener = f;
                Time.timeScale = 0;
                foreach (Fighter fighter in Services.Fighters)
                {
                    fighter.PlayerInput.SwitchCurrentActionMap("UI");
                    fighter.PlayerInput.currentActionMap.Enable();
                    if (fighter.PlayerInput.playerIndex != _opener.PlayerId) fighter.PlayerInput.DeactivateInput();
                }

                _menu.SetActive(true);
                _opener.PlayerInput.uiInputModule = _menu.GetComponent<InputSystemUIInputModule>();
                SetToTab(0);
            }
        }

        private void ResetValues()
        {
            foreach (TabAssets assets in _tabAssets)
            {
                assets.page.SetActive(false);
            }
        }

        public void UnpauseGame()
        {
            // Debug.Log("unpause");
            DisplayMenuSelection(_opener);
        }
    }
}