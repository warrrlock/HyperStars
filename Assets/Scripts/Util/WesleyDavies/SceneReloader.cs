using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WesleyDavies.UnityFunctions;

//[RequireComponent(typeof(PlayerInput))]
public class SceneReloader : MonoBehaviour
{
    public static SceneReloader Instance;

    private void Awake()
    {
        CreateSingleton();
    }

    private void Start()
    {
        SubscribeActions();
        Juice.UnfreezeTime();
    }

    private void OnDestroy()
    {
    }

    private void ResolveActions(InputAction.CallbackContext context)
    {
        //if (context.action == _playerInput.actions["Reload Scene"])
        //{
        //    ReloadScene();
        //}
    }

    public void ReloadScene(InputManager.Action action = default)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void SubscribeActions()
    {
        
    }

    private void UnsubscribeActions()
    {

    }
}
