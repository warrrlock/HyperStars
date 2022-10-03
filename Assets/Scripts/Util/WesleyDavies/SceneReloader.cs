using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class SceneReloader : MonoBehaviour
{
    private PlayerInput _playerInput;

    public static SceneReloader Singleton;

    private void Awake()
    {
        AssignComponents();
        CreateSingleton();
    }

    private void Start()
    {
        InitializeInputs();
    }

    private void OnDestroy()
    {
        TerminateInputs();
    }

    private void ResolveActions(InputAction.CallbackContext context)
    {
        if (context.action == _playerInput.actions["Reload Scene"])
        {
            ReloadScene();
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CreateSingleton()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Singleton = this;
        }
    }

    private void AssignComponents()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void InitializeInputs()
    {
        _playerInput.onActionTriggered += ResolveActions;
    }

    private void TerminateInputs()
    {
        _playerInput.onActionTriggered -= ResolveActions;
    }
}
