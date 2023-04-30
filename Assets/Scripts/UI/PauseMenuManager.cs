using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Util;

namespace UI
{
    [RequireComponent(typeof(MenuManager))]
    public class PauseMenuManager: MonoBehaviour
    {
        [Serializable]
        private class TabAssets
        {
            public Button tabButton;
            public Button firstSelected;
            public GameObject page;
        }

        [Header("Sound")]
        [SerializeField] private AK.Wwise.Event pauseSfx;
        [SerializeField] private AK.Wwise.Event exitSfx;
        
        [Header("GameObjects")]
        [SerializeField] private Slider _sliderMaster;
        [SerializeField] private Slider _sliderMusic;
        [SerializeField] private Slider _sliderCrowd;
        [SerializeField] private Slider _sliderVoice;
        [SerializeField] private Slider _sliderSfx;
        [SerializeField] private TMP_Dropdown _resDropdown;

        [Header("References")]
        [Tooltip("Keep training tab at the end")]
        [SerializeField] private TabAssets[] _tabAssets;
        [SerializeField] private GameObject _menu;
        
        private Fighter _opener;
        private InputSystemUIInputModule _inputModule;
        private int _currentTab;
        private MenuManager _menuManager;
        private int _maxTabs;
        private MultiplayerEventSystem _multiplayerEventSystem;
        private Slider[] _sliders = new Slider[5];
        private FullScreenMode[] _fullScreenModes = new FullScreenMode[4];

        private void Awake()
        {
            _menuManager = GetComponent<MenuManager>();
            _multiplayerEventSystem = GetComponentInChildren<MultiplayerEventSystem>();

            _sliders[0] = _sliderMaster;
            _sliders[1] = _sliderMusic;
            _sliders[2] = _sliderCrowd;
            _sliders[3] = _sliderVoice;
            _sliders[4] = _sliderSfx;

            _fullScreenModes[0] = FullScreenMode.ExclusiveFullScreen;
            _fullScreenModes[1] = FullScreenMode.FullScreenWindow;
            _fullScreenModes[2] = FullScreenMode.MaximizedWindow;
            _fullScreenModes[3] = FullScreenMode.Windowed;
        }

        private void Start()
        {
            if (_menu)
            {
                _menu.SetActive(false);
                _inputModule = _menu.GetComponent<InputSystemUIInputModule>();
            }
            
            //if not in training room, turn on training room manager and training tab
            if (_menuManager.IsTraining)
            {
                _tabAssets[^1].tabButton.gameObject.SetActive(true);
                GetComponentInChildren<TrainingRoomManager>(true)?.gameObject.SetActive(true);
            }
            _maxTabs = Math.Max(0, _tabAssets.Length - (_menuManager.IsTraining ? 1 : 2));
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            for (int i = 0; i < 2; i++)
            {
                PlayerInput input = Services.Fighters[i].PlayerInput;
                input.actions["Esc"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["Cancel"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["LBRB"].performed += NavigateTabController;
            }
        }

        private void UnsubscribeEvents()
        {
            for (int i = 0; i < 2; i++)
            {
                if (!Services.Fighters[i]) continue;
                Services.Fighters[i].PlayerInput.actions["Esc"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
                Services.Fighters[i].PlayerInput.actions["Cancel"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
            }
        }

        public void SetSettingValues()
        {
            _sliderMaster.value = Services.MusicManager.masterVolume;
            _sliderMusic.value = Services.MusicManager.musicVolume;
            _sliderCrowd.value = Services.MusicManager.crowdVolume;
            _sliderVoice.value = Services.MusicManager.voVolume;
            _sliderSfx.value = Services.MusicManager.sfxVolume;
            _resDropdown.value = Array.IndexOf(_fullScreenModes, Screen.fullScreenMode);
        }

        [SerializeField] private GameObject _commandListPrefab;
        [SerializeField] private GameObject _commandListParent;
        private GameObject _commandListObjects;
        public void SetControlListValues()
        {
            foreach (CharacterCmmdList.CommandList list in Services.Characters[_opener.PlayerInput.playerIndex].CommandList.commandList)
            {
                //instantiate prefab of command
                foreach (var VARIABLE in list.commands)
                {
                    
                }
            }
        }

        public void SetMasterVolume(float newValue)
        {
            Services.MusicManager.masterVolume = newValue;
        }
        public void SetMusicVolume(float newValue)
        {
            Services.MusicManager.musicVolume = newValue;
        }
        public void SetCrowdVolume(float newValue)
        {
            Services.MusicManager.crowdVolume = newValue;
        }
        public void SetVoiceVolume(float newValue)
        {
            Services.MusicManager.voVolume = newValue;
        }
        public void SetSfxVolume(float newValue)
        {
            Services.MusicManager.sfxVolume = newValue;
        }

        public void SetScreen(int mode)
        {
            Debug.Log($"would set fullscreen mode to {_fullScreenModes[mode]}");
            Screen.fullScreenMode = _fullScreenModes[mode];
        }

        private void NavigateTabController(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            int direction = callbackContext.action.ReadValue<float>() < 0 ? -1 : 1;
            NavigateInDirection(direction);
        }
        
        public void NavigateInDirection(int dir)
        {
            int tab = Math.Clamp(_currentTab+dir, 0, _maxTabs);
            SetToTab(tab);
        }
        
        public void SetToTab(int tab)
        {
            if (tab != _currentTab)
            {
                _tabAssets[_currentTab].tabButton.animator.Play("Normal");
                _tabAssets[_currentTab].page.SetActive(false);
            }
            
            _currentTab = tab;
            _tabAssets[_currentTab].page.SetActive(true);
            _tabAssets[_currentTab].tabButton.animator.Play("Selected");
            _tabAssets[_currentTab].tabButton.onClick.Invoke();
            _multiplayerEventSystem.SetSelectedGameObject(_tabAssets[_currentTab].firstSelected.gameObject);
        }

        private void DisplayP1(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            Fighter fighter = Services.Fighters[0];
            // Debug.Log($"fighter {fighter.PlayerInput.currentActionMap}");
            
            ToggleMenuSelection(fighter);
        }
        
        private void DisplayP2(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            Fighter fighter = Services.Fighters[1];
            // Debug.Log($"fighter {fighter.PlayerInput.currentActionMap}");
            ToggleMenuSelection(fighter);
        }

        private void ToggleMenuSelection(Fighter f)
        {
            // Debug.Log($"{f.name} opening pause menu");
            if (!_menu) return;
            if (_opener && f != _opener) return;
            
            if (_menu.activeSelf) //close menu
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
                exitSfx?.Post(gameObject);
                Time.timeScale = 1;
            }
            else //open menu
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
                pauseSfx?.Post(gameObject);
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
            ToggleMenuSelection(_opener);
        }
    }
}