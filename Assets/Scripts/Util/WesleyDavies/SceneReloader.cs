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
    public delegate void SceneLoaded();
    public static SceneLoaded OnSceneLoaded;
    public Action onSceneReload;

    private void Awake()
    {
        CreateSingleton();
        OnSceneLoaded?.Invoke();
    }

    private void Start()
    {
        // OnSceneLoaded?.Invoke();
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
        // onSceneReload?.Invoke();
        if (FindObjectOfType<MusicManager>())
        {
            FindObjectOfType<MusicManager>().StopMusic();
        }
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
