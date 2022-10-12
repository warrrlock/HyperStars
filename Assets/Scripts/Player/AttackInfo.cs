using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackInfo : MonoBehaviour
{
    private InputManager _inputManager;
    private MovementController _movementController;
    private Fighter _fighter;

    public float knockbackDuration;
    public float knockbackDistance;
    public float hitStunDuration;
    public float damage;
    public Vector3 knockBackAngle;
    public bool causesWallBounce;

    private void Awake()
    {
        AssignComponents();
    }

    //private void Start()
    //{
    //    _fighter.InputManager.Actions["Normal"].perform += ResetInfo;
    //    _fighter.InputManager.Actions["Special"].perform += ResetInfo;
    //}

    //private void OnDestroy()
    //{
    //    _fighter.InputManager.Actions["Normal"].perform -= ResetInfo;
    //    _fighter.InputManager.Actions["Special"].perform -= ResetInfo;
    //}

    public void ResetInfo(InputManager.Action action)
    {
        knockbackDuration = knockbackDistance = hitStunDuration = damage = 0f;
        knockBackAngle = Vector3.zero;
        causesWallBounce = false;
    }

    private void AssignComponents()
    {
        _inputManager = GetComponent<InputManager>();
        _movementController = GetComponent<MovementController>();
        _fighter = GetComponent<Fighter>();
    }
}
