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
    public bool RoundEnded => _roundEnded;

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
    [SerializeField] private string _knockoutText;
    
    [Header("Round UI")]
    [SerializeField] private FightersManager _fightersManager;
    [SerializeField] private RectTransform[] _roundUIParents;
    [SerializeField] private GameObject _roundUIPrefab;
    [SerializeField] private AK.Wwise.Switch[] _roundSwitches;
    [SerializeField] private AK.Wwise.Event[] _roundAnnouncerSFXEvents;
    [SerializeField] private Animator _roundLive2D;
    [SerializeField] private string[] _roundAnimTriggers;
    [SerializeField] private Image _koImage;
    private List<Image[]> _roundUI;
    private Image[] _p0RoundUI;
    private Image[] _p1RoundUI;


    //Animator m_Animator;
    public GameObject showtimeObject;
    private triggerAnimation _showtimeScript;

    // [SerializeField] List<Sprite> _backgrounds; //TODO: add when we have changing backgrounds
    
    private int _round;
    private bool _disabledInput;
    private bool _roundEnded;
    
    // Sunset
    [Header("Sunset Control")] [SerializeField]
    private SunsetTransition _sunsetTransition;

    private void Awake()
    {
        if (_buttons) _buttons.SetActive(false);
        if (_roundText) _roundText.gameObject.SetActive(false);
        if (_koImage) _koImage.gameObject.SetActive(false);
        if (_roundLive2D) _roundLive2D.gameObject.SetActive(false);
        _round = RoundInformation.round;
        
        SetupInitialVisuals();
        Services.RoundManager = this;
        //m_Animator = gameObject.GetComponent<Animator>();
    }

    private void Start()
    {
        _countdownText.gameObject.SetActive(false);

        //StartCoroutine(StartRound());
        SubscribeEvents();

        //stop time/movement
        DisableAllInput();

        _showtimeScript = showtimeObject.GetComponent<triggerAnimation>();

    }

    private void SubscribeEvents()
    {
        Services.CameraManager.onCameraFinalized += Begin;
    }

    private void UnsubscribeEvents()
    {
        Services.CameraManager.onCameraFinalized -= Begin;
    }

    private void Begin()
    {
        StartCoroutine(StartRound());
    }

    private void OnDestroy()
    {
        EnableAllInput();
        UnsubscribeEvents();
        Services.RoundManager = null;
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
        Services.Fighters[winner].MovementController.ResetValues();
        foreach (Fighter fighter in Services.Fighters)
        {
            fighter.BaseStateMachine.ClearQueues();
            fighter.BaseStateMachine.SetEndOfGame();
            fighter.SpecialMeterManager.HandleRoundEnd();
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
        
        if (machine.CurrentState is HurtState { HurtType: KeyHurtStatePair.HurtStateName.HitStun } or not HurtState)
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
        
        //sunset
        if (_round == 3) _sunsetTransition.PlaySunset();

        //begin count down
        _roundLive2D.gameObject.SetActive(true);
        _roundLive2D.SetTrigger(_roundAnimTriggers[_round-1]);
        _roundSwitches[Mathf.Clamp(_round - 1, 0, 2)].SetValue(gameObject);
        _roundAnnouncerSFXEvents[0].Post(gameObject);
        
        yield return new WaitForSeconds(1.5f);
        _roundLive2D.gameObject.SetActive(false);
        
        if (_countdown > 0) _countdownText.gameObject.SetActive(true);
        for (int i = _countdown; i > 0; i--)
        {
            HandleCountDown(i);
            yield return new WaitForSeconds(1.0f);
        }

        //start time/movement
        _showtimeScript.begin = 1;
        _countdownText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        _roundAnnouncerSFXEvents[1].Post(gameObject);


        yield return new WaitForSeconds(2.0f);
        
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

        if (_koImage)
        {
            _koImage.gameObject.SetActive(true);
        }
        else if (_roundText)
        {
            _roundText.gameObject.SetActive(true);
            _roundText.text = winner == -1 ? "Tie!" : _knockoutText == "" ? $"Player{winner+1} won the round!" : _knockoutText;
        }
        
        StartCoroutine(HandleStartNextRound());
    }

    private IEnumerator HandleRoundStart()
    {
        foreach (Fighter fighter in Services.Fighters)
        {
            fighter.SpecialMeterManager.ResetValues();
        }
        _roundStartEvent.Raise(new Dictionary<string, object>());
        yield return new WaitForFixedUpdate();
    }

    private void DisableAllInput()
    {
        _disabledInput = true;
        foreach (Player player in Services.Players)
        {
            player.PlayerInput.SwitchCurrentActionMap(player.PlayerInput.defaultActionMap);
            player.PlayerInput.currentActionMap.Disable();
            player.PlayerInput.SwitchCurrentActionMap("UI");
        }
    }

    private void EnableAllInput()
    {
        foreach (Player player in Services.Players)
        {
            if (!player) continue;
            player.PlayerInput.ActivateInput();
            player.PlayerInput.SwitchCurrentActionMap(player.PlayerInput.defaultActionMap);
            player.PlayerInput.currentActionMap.Enable();
        }
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
        
        _roundText.text = $"Player {winner+1} wins!";
        EnableRestartGame(); //TODO: remove after we have win/lose animations
    }
}
