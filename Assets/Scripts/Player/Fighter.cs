using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(FighterHealth))]
[RequireComponent(typeof(InputManager))]
public class Fighter : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction FacingDirection { get; private set; }

    public PlayerInputState CurrentState { get; private set; }
    public MovementController MovementController { get; private set; }
    public FighterHealth FighterHealth { get; private set; }
    public InputManager InputManager { get; private set; }

    private void Awake()
    {
        AssignComponents();
    }

    private void Start()
    {
        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
    }

    private void AssignComponents()
    {
        MovementController = GetComponent<MovementController>();
        FighterHealth = GetComponent<FighterHealth>();
        InputManager = GetComponent<InputManager>();
    }

    public void FlipCharacter(Direction newDirection)
    {
        FacingDirection = newDirection;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1f;
        transform.localScale = newScale;
    }
}
