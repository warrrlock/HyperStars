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
[RequireComponent(typeof(BaseStateMachine))]
[RequireComponent(typeof(OverlapDetector))]
public class Fighter : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction FacingDirection { get; private set; }
    public Direction MovingDirection
    {
        get => MovementController.MovingDirection;
    }
    
    public MovementController MovementController { get; private set; }
    public FighterHealth FighterHealth { get; private set; }
    public InputManager InputManager { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    public Fighter OpposingFighter { get; private set; }
    public BaseStateMachine BaseStateMachine { get; private set; }
    public FighterEvents Events { get; private set; }
    public SpecialMeterManager SpecialMeterManager { get; private set; }
    public OverlapDetector OverlapDetector { get; private set; }

    public int PlayerId { get; private set; }
    public bool Parried { get; set; }

    [NonSerialized] public int invulnerabilityCount;
    public FightersManager FightersManager
    {
        get => _fightersManager;
        private set => _fightersManager = value;
    }
    [SerializeField] private FightersManager _fightersManager;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        AssignComponents();
        PlayerId = PlayerInput.playerIndex;
        Debug.Log(PlayerId);
        
        Services.Fighters[PlayerId] = this;
        Events = new FighterEvents();
    }

    private void Start()
    {
        _spriteRenderer.sortingOrder = PlayerId;
        OpposingFighter = Array.Find(Services.Fighters, x => x.PlayerId != PlayerId);
        SpecialMeterManager?.Initialize();
        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
        invulnerabilityCount = 0;
        SubscribeActions();
        //transform.position = PlayerId == 0 ? FightersManager.player1StartPosition : FightersManager.player2StartPosition;
        GetComponent<SpriteRenderer>().color = PlayerId == 0 ? FightersManager.player1Color : FightersManager.player2Color;
        //FacingDirection = OpposingFighter.transform.position.x > transform.position.x ? Direction.Right : Direction.Left;
        ResetValues();
    }

    private void ResetValues()
    {
        transform.position = PlayerId == 0 ? FightersManager.player1StartPosition : FightersManager.player2StartPosition;
        BaseStateMachine.ResetStateMachine();
        MovementController.ResetValues();
        OverlapDetector.ReassignFighter();
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
        PlayerInput = transform.parent.GetComponent<PlayerInput>();
        BaseStateMachine = GetComponent<BaseStateMachine>();
        SpecialMeterManager = GetComponent<SpecialMeterManager>();
        OverlapDetector = GetComponent<OverlapDetector>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator DisableAllInput(Func<bool> enableCondition, Action callback = null)
    {
        StartCoroutine(InputManager.Disable(enableCondition, InputManager.Actions.Values.ToArray()));
        yield return new WaitUntil(enableCondition);
        callback?.Invoke();
    }

    public void ResetFighterHurtboxes()
    {
        if (invulnerabilityCount > 0) invulnerabilityCount--;
        Parried = false;
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
        SceneReloader.OnSceneLoaded += ResetValues;
    }

    private void UnsubscribeActions()
    {
        if (SceneReloader.Instance != null)
        {
            InputManager.Actions["Reload Scene"].perform -= SceneReloader.Instance.ReloadScene;
        }
        SceneReloader.OnSceneLoaded -= ResetValues;
    }
}
