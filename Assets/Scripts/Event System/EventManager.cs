using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private Dictionary<String, Action<Dictionary<String, object>>> _events;

    private static EventManager _eventManager;

    public static EventManager instance
    {
        get {
            if (!_eventManager) {
                _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!_eventManager) {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                } else {
                    _eventManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(_eventManager);
                } 
            }
            return _eventManager;
        }
    }

    void Init()
    {
        if (_events == null) {
            _events = new Dictionary<string, Action<Dictionary<string, object>>>();
        }
    }
}
