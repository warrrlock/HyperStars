using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Button _settingsButton;

        [Header("References")]
        [Tooltip("Keep training tab at the end")]
        [SerializeField] private TabAssets[] _tabAssets;
        [SerializeField] private GameObject _menu;
        [SerializeField] private GameObject _mainMenuSelectablesParent;
        
        [Header("Command List")]
        [SerializeField] private CmmdListReference _commandRef;
        [SerializeField] private TextMeshProUGUI _characterName; 
        [SerializeField] private GameObject _commandListParent;
        [SerializeField] private GameObject _commandContainerPrefab;
        [SerializeField] private GameObject _actionPrefab;
        
        private Player _opener;
        private InputSystemUIInputModule _inputModule;
        private int _currentTab;
        private MenuManager _menuManager;
        private int _maxTabs;
        private EventSystem _multiplayerEventSystem;
        private Slider[] _sliders = new Slider[5];
        private FullScreenMode[] _fullScreenModes = new FullScreenMode[4];
        private List<GameObject> _commandObjects;
        private Dictionary<string, Sprite> _commandDictionary;

        private Selectable[] _mainMenuSelectables;
        private GameObject _menuSelected;

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

            _commandObjects = new List<GameObject>();
            _commandDictionary = new Dictionary<string, Sprite>();
            foreach (ActionSpritePair pair in _commandRef.actionSpritePairs)
            {
                _commandDictionary.TryAdd(pair.action, pair.sprite);
            }
        }

        private void Start()
        {
            if (_menu)
            {
                _menu.SetActive(false);
                _inputModule = _menu.GetComponent<InputSystemUIInputModule>();
            }

            if (_menuManager.IsMainMenu)
            {
                _mainMenuSelectables = _mainMenuSelectablesParent.GetComponentsInChildren<Selectable>();
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
                if (!Services.Players[i]) continue;
                PlayerInput input = Services.Players[i].PlayerInput;
                input.actions["Submit"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["Esc"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["Cancel"].performed += (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["LBRB"].performed += NavigateTabController;
            }
        }

        private void UnsubscribeEvents()
        {
            for (int i = 0; i < 2; i++)
            {
                if (!Services.Players[i]) continue;
                PlayerInput input = Services.Players[i].PlayerInput;
                input.actions["Submit"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["Esc"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["Cancel"].performed -= (i == 0 ? DisplayP1 : DisplayP2);
                input.actions["LBRB"].performed -= NavigateTabController;
            }
        }

        public void SetSettingValues()
        {
            if(!Services.MusicManager) return;
            _sliderMaster.value = Services.MusicManager.masterVolume;
            _sliderMusic.value = Services.MusicManager.musicVolume;
            _sliderCrowd.value = Services.MusicManager.crowdVolume;
            _sliderVoice.value = Services.MusicManager.voVolume;
            _sliderSfx.value = Services.MusicManager.sfxVolume;
            _resDropdown.value = Array.IndexOf(_fullScreenModes, Screen.fullScreenMode);
        }

        public void SetControlListValues()
        {
            CharacterCmmdList characterCombos = Services.Characters[_opener.PlayerInput.playerIndex].CommandList;
            _characterName.text = Services.Characters[_opener.PlayerInput.playerIndex].name;
            //create missing gameobjects
            if (characterCombos.commandList.Count > _commandObjects.Count)
            {
                for (int i = _commandObjects.Count; i < characterCombos.commandList.Count; i++)
                {
                    _commandObjects.Add(Instantiate(_commandContainerPrefab, _commandListParent.transform));
                }
            }
            else
            {
                for (int i = characterCombos.commandList.Count; i < _commandObjects.Count; i++)
                {
                    _commandObjects[i].SetActive(false);
                }
            }

            for (int i = 0; i < characterCombos.commandList.Count; i++)
            {
                CharacterCmmdList.CommandList list = characterCombos.commandList[i];
                //create missing or disable extra
                GameObject container = _commandObjects[i];
                container.SetActive(true);
                container.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = list.name;
                Transform actionContainer = container.transform.GetChild(1);
                
                if (actionContainer.childCount < list.commands.Count)
                {
                    for (int j = actionContainer.childCount; j < list.commands.Count; j++)
                    {
                        Instantiate(_actionPrefab, actionContainer);
                    }
                }
                else if (actionContainer.childCount > list.commands.Count)
                {
                    for (int j = list.commands.Count; j < actionContainer.childCount; j++)
                    {
                        actionContainer.GetChild(j).gameObject.SetActive(false);
                    }
                }

                int index = 0;
                foreach (var command in list.commands)
                {
                    _commandDictionary.TryGetValue(command, out Sprite sprite);
                    if (!sprite) continue;
                    GameObject actionObj = actionContainer.GetChild(index).gameObject;
                    actionObj.SetActive(true);
                    actionObj.GetComponent<Image>().sprite = sprite;
                    index++;
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
            // Debug.Log($"would set fullscreen mode to {_fullScreenModes[mode]}");
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
            if (!CheckOpenSettings(callbackContext)) return;
            Player player = Services.Players[0];
            ToggleMenuSelection(player);
        }
        
        private void DisplayP2(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.action.WasPerformedThisFrame()) return;
            if (!CheckOpenSettings(callbackContext)) return;
            Player player = Services.Players[1];
            ToggleMenuSelection(player);
        }

        private bool CheckOpenSettings(InputAction.CallbackContext callbackContext)
        {
            if (_menuManager.IsMainMenu)
            {
                if (callbackContext.action.name == "Submit")
                {
                    // Debug.Log($"{_menu.activeSelf} {EventSystem.current.currentSelectedGameObject != _settingsButton.gameObject}");
                    if (_menu.activeSelf) return false;
                    if (EventSystem.current.currentSelectedGameObject != _settingsButton.gameObject) return false;
                }
            }
            return true;
        }

        private void ToggleMenuSelection(Player p)
        {
            // Debug.Log($"{f.name} opening pause menu");
            if (!_menu) return;
            if (_opener && p != _opener) return;
            
            if (_menu.activeSelf) //close menu
            {
                if (!_opener) return;
                _opener.PlayerInput.uiInputModule = null;
                _opener = null;
                if (p)
                {
                    foreach (Player player in Services.Players)
                    {
                        if (!player) continue;
                        player.PlayerInput.currentActionMap.Disable();
                        player.PlayerInput.ActivateInput();
                        if (!_menuManager.IsMainMenu) player.PlayerInput.SwitchCurrentActionMap(player.PlayerInput.defaultActionMap);
                    }
                }

                ResetValues();
                _menu.SetActive(false);
                exitSfx?.Post(gameObject);
                Time.timeScale = 1;
                if (_menuManager.IsMainMenu)
                {
                    foreach (var selectable in _mainMenuSelectables)
                        selectable.interactable = true;
                    EventSystem.current.SetSelectedGameObject(_menuSelected);
                }
            }
            else //open menu
            {
                if (_menuManager.IsMainMenu)
                {
                    _menuSelected = EventSystem.current.currentSelectedGameObject;
                    EventSystem.current.SetSelectedGameObject(null);
                    foreach (var selectable in _mainMenuSelectables)
                        selectable.interactable = false;
                }
                _opener = p;
                Time.timeScale = 0;
                if (p)
                {
                    foreach (Player player in Services.Players)
                    {
                        if (!player) continue;
                        if (!_menuManager.IsMainMenu) player.PlayerInput.SwitchCurrentActionMap("UI");
                        player.PlayerInput.currentActionMap.Enable();
                        if (player.PlayerInput.playerIndex != _opener.PlayerInput.playerIndex) player.PlayerInput.DeactivateInput();
                    }
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