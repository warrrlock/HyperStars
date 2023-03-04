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
    public Action onReady;
    
    [SerializeField] private Character _character;
    [SerializeField] private bool _ready;
    [SerializeField] private BuildSettingIndices _indices;
    
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
        CheckEnterResetPlayerScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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

        StartCoroutine(GetReady());
    }

    public IEnumerator GetReady()
    {
        yield return new WaitForFixedUpdate();
        _ready = true;
        onReady?.Invoke();
        //TODO: move input to character colours
    }
    
    public void UnReady()
    {
        _ready = false;
        onReady?.Invoke();
        //TODO: move input to character selection
    }
    
    public void SelectBot(Character character)
    {
        Services.Characters[PlayerInput.playerIndex^1] = character;
        Services.Players[PlayerInput.playerIndex ^ 1]._ready = true;
        Services.Players[PlayerInput.playerIndex ^ 1].onReady?.Invoke();
    }

    private void ReadyStartingGame()
    {
        if (_character) Services.Characters[PlayerInput.playerIndex] = _character;
        if (!FighterObject) FighterObject = Instantiate(Services.Characters[PlayerInput.playerIndex].CharacterPrefab, transform);
        PlayerInput.uiInputModule = null;
    }

    private void ResetPlayer()
    {
        if (FighterObject) Destroy(FighterObject);
        _character = null;
        _ready = false;
    }

    private void SetSelectionSceneValues()
    {
        CharacterButtonsPlayer[] selections = FindObjectsOfType<CharacterButtonsPlayer>();
        foreach (var selection in selections)
        {
            if (selection.PlayerId == PlayerInput.playerIndex)
            {
                selection.SetPlayer(this);
                PlayerInput.uiInputModule = selection.GetComponent<InputSystemUIInputModule>();
                break;
            }
        }
        
        PlayerInput.SwitchCurrentActionMap("UI");
        UIInputManager[] inputManagers = FindObjectsOfType<UIInputManager>();
        foreach (var inputManager in inputManagers)
        {
            if (inputManager.PlayerId == PlayerInput.playerIndex)
            {
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
            PlayerInput.SwitchCurrentActionMap(PlayerInput.defaultActionMap);
            ReadyStartingGame();
        }
    }
}