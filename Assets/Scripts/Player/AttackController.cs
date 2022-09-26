using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(RaycastController))]
public class AttackController : MonoBehaviour
{
    public class Attack
    {
        public readonly Attack nextNormal;
        public readonly Attack nextSpecial;
    }

    public class NormalMash : Attack
    {
        public readonly NormalMash nextMash;
    }

    public class Combo
    {
        public readonly Attack[] precedingAttacks;
        public readonly Attack followingAttack;
        [Tooltip("How much time can be taken between the last attack executing and this input being pressed?")]
        public readonly float maxAttackDelay;
    }

    private PlayerInput _playerInput;
    private RaycastController _controller;

    private Attack _currentAttack;
    private Attack _queuedAttack;

    private void Awake()
    {
        AssignComponents();
    }

    private void Start()
    {
        _playerInput.onActionTriggered += ResolveActions;
    }

    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        _playerInput.onActionTriggered -= ResolveActions;
    }

    private void AssignComponents()
    {
        _playerInput = GetComponent<PlayerInput>();
        _controller = GetComponent<RaycastController>();
    }

    private void ResolveActions(InputAction.CallbackContext context)
    {
        if (context.action == _playerInput.actions["Normal"])
        {
            if (context.action.WasPerformedThisFrame())
            {
                if (_controller.CollisionData.y.isNegativeHit)
                {
                    //execute grounded neutral
                }
                else
                {
                    //execute aerial neutral
                }
            }
        }

        if (context.action == _playerInput.actions["Special"])
        {
            if (context.action.WasPerformedThisFrame())
            {

            }
        }

        if (context.action == _playerInput.actions["Side Special"])
        {
            if (context.action.WasPerformedThisFrame())
            {

            }
        }
    }
}
