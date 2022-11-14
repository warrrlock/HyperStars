using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(FighterHealth))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(BaseStateMachine))]
public class Fighter : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction FacingDirection { get; private set; }
    public Direction MovingDirection
    {
        get => MovementController.MovingDirection;
    }

    public PlayerInputState CurrentState { get; private set; }
    public MovementController MovementController { get; private set; }
    public FighterHealth FighterHealth { get; private set; }
    public InputManager InputManager { get; private set; }
    public HurtAnimator HurtAnimator { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    public Fighter OpposingFighter { get; private set; }
    public BaseStateMachine BaseStateMachine { get; private set; }
    public FighterEvents Events { get; private set; }
    public SpecialMeterManager SpecialMeterManager { get; private set; }

    public int PlayerId { get; private set; }

    [NonSerialized] public int invulnerabilityCount;
    public FightersManager FightersManager
    {
        get => _fightersManager;
        private set => _fightersManager = value;
    }
    [SerializeField] private FightersManager _fightersManager;
    
    private void Awake()
    {
        AssignComponents();
        PlayerId = PlayerInput.playerIndex;
        Debug.Log(PlayerId);
        if (PlayerId > 1)
        {
            Destroy(gameObject);
            return;
        }
        Services.Fighters[PlayerId] = this;
        Events = new();
        DontDestroyOnLoad(gameObject);
        SceneReloader.OnSceneLoaded += ResetValues;
    }

    private void Start()
    {
        OpposingFighter = Array.Find(Services.Fighters, x => x.PlayerId != PlayerId);
        SpecialMeterManager?.Initiate();
        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
        invulnerabilityCount = 0;
        SubscribeActions();
        //transform.position = PlayerId == 0 ? FightersManager.player1StartPosition : FightersManager.player2StartPosition;
        GetComponent<SpriteRenderer>().color = PlayerId == 0 ? FightersManager.player1Color : FightersManager.player2Color;
        //FacingDirection = OpposingFighter.transform.position.x > transform.position.x ? Direction.Right : Direction.Left;
    }

    private void ResetValues()
    {
        transform.position = PlayerId == 0 ? FightersManager.player1StartPosition : FightersManager.player2StartPosition;
        BaseStateMachine.ResetStateMachine();
    }

    private void OnDestroy()
    {
        if (PlayerId > 1)
        {
            return;
        }
        UnsubscribeActions();
    }

    private void AssignComponents()
    {
        MovementController = GetComponent<MovementController>();
        FighterHealth = GetComponent<FighterHealth>();
        InputManager = GetComponent<InputManager>();
        HurtAnimator = GetComponent<HurtAnimator>();
        PlayerInput = GetComponent<PlayerInput>();
        BaseStateMachine = GetComponent<BaseStateMachine>();
        SpecialMeterManager = GetComponent<SpecialMeterManager>();
    }

    public void DisableAllInput(Func<bool> enableCondition)
    {
        StartCoroutine(InputManager.Disable(enableCondition, InputManager.Actions.Values.ToArray()));
    }

    public void ResetFighterHurtboxes()
    {
        invulnerabilityCount--;
    }

    public void FlipCharacter(Direction newDirection)
    {
        FacingDirection = newDirection;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1f;
        transform.localScale = newScale;
    }

    private void SubscribeActions()
    {
        if (SceneReloader.Instance != null)
        {
            InputManager.Actions["Reload Scene"].perform += SceneReloader.Instance.ReloadScene;
        }
    }

    private void UnsubscribeActions()
    {
        if (SceneReloader.Instance != null)
        {
            InputManager.Actions["Reload Scene"].perform -= SceneReloader.Instance.ReloadScene;
        }
    }
}
