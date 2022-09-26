using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RaycastController))]
[RequireComponent(typeof(AttackController))]
public class Character : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction FacingDirection { get; private set; }

    public PlayerInputState CurrentState { get; private set; }

    private void Start()
    {
        //TODO: change this because not all characters will start off facing right
        FacingDirection = Direction.Right;
    }

    public void FlipCharacter(Direction newDirection)
    {
        FacingDirection = newDirection;
    }
}
