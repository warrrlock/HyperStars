using System;
using System.Collections;
using FiniteStateMachine;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
/*
 * player needs to not be destroyed
 * attach character object here once character is chosen via ui
 */
public class Player: MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    [SerializeField] private Character _character;
    private GameObject FighterObject { get; set; }
    public bool Ready => _ready;
    [SerializeField] private bool _ready;
    public Action onReady;

    private readonly int _mainMenuSceneIndex = 0;
    private readonly int _selectionSceneIndex = 1;
    private readonly int _gameSceneIndex = 2;
    private readonly int _trainingSceneIndex = 3;

    private void Awake()
    {
        AssignComponents();
        if (PlayerInput.playerIndex > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
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

    private void ReadyStartingGame()
    {
        Services.Characters[PlayerInput.playerIndex] = _character;
        if (!FighterObject) FighterObject = Instantiate(_character.CharacterPrefab, transform);
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
                selection.Player = this;
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
        if (scene.buildIndex == _mainMenuSceneIndex)
        {
            ResetPlayer();
        }
        if (scene.buildIndex == _selectionSceneIndex)
        {
            ResetPlayer();
            SetSelectionSceneValues();
        }
        else if (scene.buildIndex == _gameSceneIndex || scene.buildIndex == _trainingSceneIndex)
        {
            PlayerInput.SwitchCurrentActionMap("Player");
            ReadyStartingGame();
        }
    }
}