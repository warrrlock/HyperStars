using System;
using System.Collections;
using System.Collections.Generic;
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

    public PlayerInputState CurrentState { get; private set; }
    public MovementController MovementController { get; private set; }
    public FighterHealth FighterHealth { get; private set; }
    public InputManager InputManager { get; private set; }
    public HurtAnimator HurtAnimator { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    public Fighter OpposingFighter { get; private set; }
    public BaseStateMachine BaseStateMachine { get; private set; }
    
    public Action<Fighter, Fighter, Vector3> onAttackHit;

    public int PlayerId { get; private set; }

    public bool canBeHurt;
    
    private void Awake()
    {
        AssignComponents();
        PlayerId = PlayerInput.playerIndex;
        Debug.Log(PlayerId);
        Services.Fighters[PlayerId] = this;
    }

    private void Start()
    {
        OpposingFighter = Array.Find(Services.Fighters, x => x.PlayerId != PlayerId);
        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
        canBeHurt = true;
        SubscribeActions();
    }

    private void OnDestroy()
    {
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
    }

    public void ResetFighterHurtboxes()
    {
        canBeHurt = true;
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
