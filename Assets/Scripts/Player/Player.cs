using System;
using Managers;
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
    public Action subscription;

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
        CheckEnterResetPlayerScene();
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
        SceneReloader.OnSceneLoaded += CheckEnterResetPlayerScene;
    }

    private void UnsubscribeActions()
    {
        SceneReloader.OnSceneLoaded -= CheckEnterResetPlayerScene;
    }
    

    public void SelectCharacter(Character character)
    {
        _ready = false;
        _character = character;
    }

    public void GetReady()
    {
        _ready = true;
        subscription?.Invoke();
    }

    private void ReadyStartingGame()
    {
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

        MenuManager manager = FindObjectOfType<MenuManager>();
        if (manager) manager.AddPlayer(this);
        else Debug.LogError("no menu manager exists in selection scene.");
    }

    private void CheckEnterResetPlayerScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex == _mainMenuSceneIndex)
        {
            ResetPlayer();
        }
        if (currentScene.buildIndex == _selectionSceneIndex)
        {
            ResetPlayer();
            SetSelectionSceneValues();
        }
        else if (currentScene.buildIndex == _gameSceneIndex || currentScene.buildIndex == _trainingSceneIndex)
        {
            ReadyStartingGame();
        }
    }
}