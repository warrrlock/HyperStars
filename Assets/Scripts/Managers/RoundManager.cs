using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(GameEventListener))]
public class RoundManager : MonoBehaviour
{
    public bool InGame => !_disabledInput;

    [Header("Rounds meta")] 
    [SerializeField] private GameEvent _roundStartEvent;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private int _neededWins;
    
    [Header("Text Info")]
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private string _startText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] int _countdown;
    
    [Header("Round UI")]
    [SerializeField] private FightersManager _fightersManager;
    [SerializeField] private RectTransform[] _roundUIParents;
    [SerializeField] private GameObject _roundUIPrefab;
    private List<Image[]> _roundUI;
    private Image[] _p0RoundUI;
    private Image[] _p1RoundUI;
    
    // [SerializeField] List<Sprite> _backgrounds; //TODO: add when we have changing backgrounds
    
    private int _round;
    private bool _disabledInput;
    private bool _roundEnded;

    private void Awake()
    {
        if (_buttons) _buttons.SetActive(false);
        if (_roundText) _roundText.gameObject.SetActive(false);
        _round = RoundInformation.round;
        
        SetupInitialVisuals();
    }

    private void Start()
    {
        StartCoroutine(StartRound());
    }

    private void OnDestroy()
    {
        EnableAllInput();
    }
    
    //used in event (inspector)
    public void OnRoundEnd(Dictionary<string, object> data)
    {
        Debug.Log("round ended");
        if (_roundEnded) return;
        _roundEnded = true;
        //disable movement
        DisableAllInput();
        
        //TODO: should player's queues be cleared? so that if a player has queued another attack, it doesn't play
        foreach (Fighter fighter in Services.Fighters)
        {
            fighter.BaseStateMachine.ClearQueues();
        }
        
        data.TryGetValue("winnerId", out object winnerId);
        int winner = winnerId == null ? -1 : (int)winnerId;

        HandleAddWinTo(winner);
        RoundInformation.SetIfMatchPoint(_neededWins, winner);
        
        if (RoundInformation.CheckWinner(_neededWins, winner))
        {
            EndGame();
            return;
        }

        if (_roundText)
        {
            _roundText.gameObject.SetActive(true);
            _roundText.text = winner == -1 ? "Tie!" : $"Player{winner+1} won the round!";
        }

        StartCoroutine(HandleStartNextRound());
    }
    
    //for use in animation
    public void EnableRestartGame()
    {
        _buttons.SetActive(true);
        Services.Players[0].PlayerInput.uiInputModule = GetComponentInChildren<InputSystemUIInputModule>();
    }
    
    public void RestartGame()
    {
        Debug.Log("restarting");
        RoundInformation.ResetRounds();
    }
    
    private void SetupInitialVisuals()
    {
        _p0RoundUI = new Image[_neededWins];
        _p1RoundUI = new Image[_neededWins];
        _roundUI = new List<Image[]>{_p0RoundUI, _p1RoundUI};
        
        for (int i = 0; i < _neededWins; i++)
        {
            _p0RoundUI[i] = Instantiate(_roundUIPrefab, _roundUIParents[0]).GetComponent<Image>();
            _p1RoundUI[i] = Instantiate(_roundUIPrefab, _roundUIParents[1]).GetComponent<Image>();
        }
        for (int i = 0; i < RoundInformation.Wins[0]; i++)
            _p0RoundUI[i].color = _fightersManager.playerColors[0];
        for (int i = 0; i < RoundInformation.Wins[1]; i++)
            _p1RoundUI[i].color = _fightersManager.playerColors[1];
    }

    private void HandleAddWinTo(int player)
    {
        RoundInformation.AddWinTo(player);
        _roundUI[player][RoundInformation.Wins[player]-1].color = _fightersManager.playerColors[player];
    }

    private IEnumerator HandleStartNextRound()
    {
        //TODO: any ui updates
        SetNewRoundVariables();
        
        yield return new WaitForSeconds(1f);
        SceneReloader.Instance?.ReloadScene();
    }

    private void SetNewRoundVariables()
    {
        //change any persistent variables
        RoundInformation.round = ++_round;
        //ChangeEnvironment;
    }
    
    private IEnumerator StartRound()
    {
        if (!_countdownText) yield break;
        _countdownText.gameObject.SetActive(true);
       
        //stop time/movement
        DisableAllInput();

        //begin count down
        _countdownText.text = $"Round {_round}";
        yield return new WaitForSeconds(1.0f);
        if (RoundInformation.MatchPoint)
        {
            _countdownText.text = "Match point!";
            yield return new WaitForSeconds(0.8f);
            
            // _countdownText.text = $@"{(RoundInformation.MatchPointPlayers[0] ? 
            //     (RoundInformation.MatchPointPlayers[1] ? "Scores Tied!" : "Player 1 in the lead.")
            //     : "Player2 in the lead")}";
            // yield return new WaitForSeconds(0.8f);
        }

        for (int i = _countdown; i > 0; i--)
        {
            HandleCountDown(i);
            yield return new WaitForSeconds(1.0f);
        }
        
        //TODO: any necessary UI
        
        //start time/movement
        _countdownText.text = _startText;

        yield return new WaitForSeconds(1.0f);
        _countdownText.gameObject.SetActive(false);
        
        EnableAllInput();
        StartCoroutine(HandleRoundStart());
    }

    private IEnumerator HandleRoundStart()
    {
        _roundStartEvent.Raise(new Dictionary<string, object>());
        yield return new WaitForFixedUpdate();
        foreach (Fighter fighter in Services.Fighters)
            fighter.InputManager.ResetValues();
    }

    private void DisableAllInput()
    {
        _disabledInput = true;
        foreach (Fighter fighter in Services.Fighters)
            StartCoroutine(fighter.DisableAllInput(() => _disabledInput == false));
    }

    private void EnableAllInput()
    {
        _disabledInput = false;
    }

    private void HandleCountDown(int i)
    {
        _countdownText.text = $"{i}";
        //TODO: other UI
    }

    private void EndGame()
    {
        //TODO: disable all input
        DisableAllInput();
        AnnounceWinner();
    }

    private void AnnounceWinner()
    {
        int winner = RoundInformation.GetWinner();
        int loser = winner == 0 ? 1 : 0;
        //lose animation -> the losing character dies lol
        //Win animation -> the character walks up closer to the camera and does a victory pose. 
        
        _roundText.gameObject.SetActive(true);
        
        _roundText.text = $"Player {winner+1} is victorious!";
        EnableRestartGame(); //TODO: remove after we have win/lose animations
    }
}
