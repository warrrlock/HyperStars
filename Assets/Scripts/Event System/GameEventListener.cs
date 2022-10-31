using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent _event;
    [SerializeField] private List<UnityEvent<Dictionary<string, object>>> _responses;
    private void OnEnable()
    { _event.AddListener(this); }

    private void OnDisable()
    { _event.RemoveListener(this); }

    public void AddAction(UnityEvent<Dictionary<string, object>> response)
    {
        _responses.Add(response);
    }
    
    public void RemoveAction(UnityEvent<Dictionary<string, object>> response)
    {
        _responses.Remove(response);
    }
    
    public void OnEventRaised(Dictionary<string, object> data)
    {
        for (int i = _responses.Count-1; i >=0; i--)
            _responses[i].Invoke(data);
    }
    
}
