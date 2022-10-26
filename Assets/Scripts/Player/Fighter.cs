using System;
using System.Collections;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(FighterHealth))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(HurtAnimator))]
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
    public BaseStateMachine BaseStateMachine { get; private set; }
    
    public Action<Fighter, Fighter, Vector3> onAttackHit;

    public int PlayerId { get; private set; }
    
    private void Awake()
    {
        AssignComponents();
    }

    private void Start()
    {
        Services.Fighters.Add(this);

        PlayerId = PlayerInput.playerIndex;
        Debug.Log(PlayerId);

        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
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

    public void FlipCharacter(Direction newDirection)
    {
        FacingDirection = newDirection;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1f;
        transform.localScale = newScale;
    }
}
