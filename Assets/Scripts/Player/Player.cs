using System;
using System.Collections;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Util;

[RequireComponent(typeof(PlayerInput))]
/*
 * player needs to not be destroyed
 * attach character object here once character is chosen via ui
 */
public class Player: MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    public bool Ready => _ready;
    public bool CharacterSelected { get; private set; }
    public Action onReady;
    public Action unReady;
    public int PaletteIndex { get; set; }
    
    [SerializeField] private Character _character;
    [SerializeField] private bool _ready;
    [SerializeField] private BuildSettingIndices _indices;

    private PalettePickerManager _palettePickerManager;
    private UIInputManager _uiInputManager;

    
    public GameObject FighterObject { get; set; }

    private void Awake()
    {
        AssignComponents();
        if (PlayerInput.playerIndex > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Services.Players[PlayerInput.playerIndex] = this;
        SubscribeActions();
        // CheckEnterResetPlayerScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    
    private void OnDestroy()
    {
        if (PlayerInput.playerIndex > 1)
        {
            return;
        }
        UnsubscribeActions();
    }

    private void AssignComponents()
    {
        PlayerInput = GetComponent<PlayerInput>();
    }

    private void SubscribeActions()
    {
        SceneManager.sceneLoaded += CheckEnterResetPlayerScene;
    }

    private void UnsubscribeActions()
    {
        SceneManager.sceneLoaded -= CheckEnterResetPlayerScene;
    }
    
    public void SelectCharacter(Character character)
    {
        _ready = false;
        _character = character;
        Services.Characters[PlayerInput.playerIndex] = _character;
        
        //Select colours before ready
        CharacterSelected = true;
        OpenPalettePicker(PlayerInput.playerIndex);
    }
    
    public void SelectBot(Character character)
    {
        Debug.Log($"{PlayerInput.playerIndex} select bot");
        Services.Characters[PlayerInput.playerIndex^1] = character;
        //choose bot colours
        Services.Players[PlayerInput.playerIndex ^ 1].CharacterSelected = true;
        OpenPalettePicker(PlayerInput.playerIndex^1);
    }

    public void CloseColourPicker(bool isBot = false)
    {
        //TODO: move input to character selection
        CharacterSelected = false;
        if (!isBot) SetSelectionUI();
        _palettePickerManager.PalettePickers[PlayerInput.playerIndex].UnsetVisuals();
    }

    private void OpenPalettePicker(int player)
    {
        //TODO: move input to character colours
        // Debug.Log($"Open palette picker for player {player}, palette picker: {_palettePickerManager.PalettePickers[player]}");
        UnsetSelectionUI();
        _palettePickerManager.PalettePickers[player].SetupVisuals(Services.Characters[player].CharacterPalettes);
        _palettePickerManager.SetColour(player, 0, 1, Services.Characters[player].CharacterPalettes.Palette.Count);
    }
    
    public void ConfirmColour()
    {
        StartCoroutine(GetReady());
    }

    public IEnumerator GetReady()
    {
        yield return new WaitForFixedUpdate();
        // Debug.Log($"{PlayerInput.playerIndex} is ready");
        _ready = true;
        onReady?.Invoke();
    }
    
    public void UnReady()
    {
        // Debug.Log($"Un readying {PlayerInput.playerIndex}");
        _ready = false;
        unReady?.Invoke();
        OpenPalettePicker(PlayerInput.playerIndex);
    }

    private void ReadyStartingGame()
    {
        if (_character) Services.Characters[PlayerInput.playerIndex] = _character;
        if (!FighterObject) FighterObject = Instantiate(Services.Characters[PlayerInput.playerIndex].CharacterPrefab, transform);
        FighterObject.GetComponent<ColorPicker>()?.SetMaterialColors(PaletteIndex);
        UnsetSelectionUI();
    }

    private void ResetPlayer()
    {
        if (FighterObject) Destroy(FighterObject);
        _character = null;
        _ready = false;
        CharacterSelected = false;
        Services.Characters[PlayerInput.playerIndex] = null;
        // Services.Fighters[PlayerInput.playerIndex] = null;
    }

    private void SetSelectionUI()
    {
        CharacterButtonsPlayer[] selections = FindObjectsOfType<CharacterButtonsPlayer>();
        foreach (var selection in selections)
        {
            if (selection.PlayerId == PlayerInput.playerIndex)
            {
                selection.SetPlayer(this);
                InputSystemUIInputModule uiModule = selection.GetComponent<InputSystemUIInputModule>();
                uiModule.AssignDefaultActions();
                PlayerInput.uiInputModule = uiModule;
                break;
            }
        }
    }

    private void UnsetSelectionUI()
    {
        if (PlayerInput.uiInputModule) PlayerInput.uiInputModule.UnassignActions();
        PlayerInput.uiInputModule = null;
        PlayerInput.ActivateInput();
    }

    private void SetSelectionSceneValues()
    {
        // Debug.Log("setting selection values");
        SetSelectionUI();

        _palettePickerManager = FindObjectOfType<PalettePickerManager>();
        
        PlayerInput.SwitchCurrentActionMap("UI");
        UIInputManager[] inputManagers = FindObjectsOfType<UIInputManager>();
        // Debug.Log($"has {inputManagers.Length} input managers");
        foreach (var inputManager in inputManagers)
        {
            if (inputManager.PlayerId == PlayerInput.playerIndex)
            {
                // Debug.Log($"setting input manager {inputManager.name}, {inputManager.PlayerId}; {PlayerInput.playerIndex}");
                _uiInputManager = inputManager;
                inputManager.Initialize(this);
            }
        }

        MenuManager manager = FindObjectOfType<MenuManager>();
        if (manager) manager.AddPlayer(this);
        else Debug.LogError("no menu manager exists in selection scene.");
    }

    private void CheckEnterResetPlayerScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == _indices.mainMenuScene)
        {
            Destroy(gameObject);
        }
        else if (scene.buildIndex == _indices.selectionScene || scene.buildIndex == _indices.trainingSelectionScene)
        {
            ResetPlayer();
            SetSelectionSceneValues();
        }
        else if (scene.buildIndex == _indices.gameScene || scene.buildIndex == _indices.trainingScene)
        {
            Debug.Log("in game scene, readying game");
            PlayerInput.SwitchCurrentActionMap(PlayerInput.defaultActionMap);
            ReadyStartingGame();
        }
    }
}