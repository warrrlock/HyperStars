using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
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
    [Tooltip("time to wait in slow motion.")]
    [SerializeField] private float _slomoTime;
    [Tooltip("Float between 0 and 1, where 1 is normal time, and 0 is paused.")]
    [SerializeField] private float _slomoSpeed = 1;

    [Header("Text Info")]
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private string _startText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] int _countdown;
    
    [Header("Round UI")]
    [SerializeField] private FightersManager _fightersManager;
    [SerializeField] private RectTransform[] _roundUIParents;
    [SerializeField] private GameObject _roundUIPrefab;
    [SerializeField] private AK.Wwise.Switch[] _roundSwitches;
    [SerializeField] private AK.Wwise.Event[] _roundAnnouncerSFXEvents;
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
        if (_roundEnded) return;
        Debug.Log("round ended");
        _roundEnded = true;
        
        DisableAllInput();
        
        data.TryGetValue("winnerId", out object winnerId);
        int winner = winnerId == null ? -1 : (int)winnerId;

        StartCoroutine(HandleRoundEnd(winner));
    }

    private IEnumerator HandleRoundEnd(int winner)
    {
        yield return new WaitForEndOfFrame();
        //if a player has queued another attack, it doesn't play
        foreach (Fighter fighter in Services.Fighters)
        {
            fighter.BaseStateMachine.ClearQueues();
        }
        
        HandleAddWinTo(winner);
        HandleLoseAnimation(winner^1);
        StartCoroutine(HandleSlomoSequence(winner));
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

    private void HandleLoseAnimation(int player)
    {
        BaseStateMachine machine = Services.Fighters[player].BaseStateMachine;
        machine.DisableTime = _slomoTime;
        machine.ExecuteDisableTime();
        machine.IgnoreExecuteState = true;
        
        if (machine.CurrentState is HurtState { HurtType: KeyHurtStatePair.HurtStateName.HitStun })
        {
            machine.BypassQueuing(Services.Characters[player].LoseState);
            Services.Fighters[player].MovementController.ApplyForce(Vector3.up, 30f, 1f, true);
            Services.Characters[player].LoseState.Execute(machine, "");
        }
    }

    private IEnumerator HandleStartNextRound()
    {
        //TODO: any ui updates
        SetNewRoundVariables();
        
        yield return new WaitForSeconds(2f);
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
        _roundSwitches[Mathf.Clamp(_round - 1, 0, 2)].SetValue(gameObject);
        _roundAnnouncerSFXEvents[0].Post(gameObject);
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
        _roundAnnouncerSFXEvents[1].Post(gameObject);

        yield return new WaitForSeconds(1.0f);
        _countdownText.gameObject.SetActive(false);
        
        EnableAllInput();
        StartCoroutine(HandleRoundStart());
    }

    private IEnumerator HandleSlomoSequence(int winner)
    {
        float time = _slomoTime;
        
        while (time > 0)
        {
            Time.timeScale = _slomoSpeed; //TODO: fix timescale getting reset somewhere during hit (cancel that)
            time -= Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1;
        
        RoundInformation.SetIfMatchPoint(_neededWins, winner);
        if (RoundInformation.CheckWinner(_neededWins, winner))
        {
            EndGame();
            yield break;
        }
        
        if (_roundText)
        {
            _roundText.gameObject.SetActive(true);
            _roundText.text = winner == -1 ? "Tie!" : $"Player{winner+1} won the round!";
        }
        
        StartCoroutine(HandleStartNextRound());
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
        {
            fighter.InputManager.IgnoreQueuePerform = true;
            StartCoroutine(fighter.DisableAllInput(
                () => _disabledInput == false, 
                () => { fighter.InputManager.IgnoreQueuePerform = false; }
                ));
        }
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
