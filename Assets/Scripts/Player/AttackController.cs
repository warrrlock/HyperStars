using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(RaycastController))]
public class AttackController : MonoBehaviour
{
    private InputManager _inputManager;
    private RaycastController _controller;

    //public class Attack
    //{
    //    public readonly Attack nextNormal;
    //    public readonly Attack nextSpecial;
    //}

    //public class NormalMash : Attack
    //{
    //    public readonly NormalMash nextMash;
    //}

    //public class Combo
    //{
    //    public readonly Attack[] precedingAttacks;
    //    public readonly Attack followingAttack;
    //    [Tooltip("How much time can be taken between the last attack executing and this input being pressed?")]
    //    public readonly float maxAttackDelay;
    //}

    //private Attack _currentAttack;
    //private Attack _queuedAttack;

    private void Awake()
    {
        AssignComponents();
    }

    private void Start()
    {
        SubscribeActions();
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
    }

    private void AssignComponents()
    {
        _inputManager = GetComponent<InputManager>();
        _controller = GetComponent<RaycastController>();
    }

    private void SubscribeActions()
    {
        _inputManager.Actions["Normal"].perform += ExecuteNormal;
        _inputManager.Actions["Special"].perform += ExecuteSpecial;
    }

    private void UnsubscribeActions()
    {
        _inputManager.Actions["Normal"].perform -= ExecuteNormal;
        _inputManager.Actions["Special"].perform -= ExecuteSpecial;
    }

    private void ExecuteNormal(InputManager.Action action)
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

    private void ExecuteSpecial(InputManager.Action action)
    {
        if (_controller.CollisionData.y.isNegativeHit)
        {
            //execute grounded special
        }
        else
        {
            //execute aerial special
        }
    }
}
