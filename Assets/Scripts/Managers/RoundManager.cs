using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameEventListener))]
public class RoundManager : MonoBehaviour
{
    [SerializeField] private int _maxRounds;
    [SerializeField] int _countDownNumber;
    [SerializeField] List<Sprite> _backgrounds;
    
    private int _round;

    public void Start()
    {
        //start coroutine counts down w/ announcer to start round
        StartCoroutine(StartRound());
    }

    public void OnRoundEnd(Dictionary<string, object> data)
    {
        
        //reset scene, set next background
        foreach (var entry in data)
        {
            _round++;
            //if done max rounds, end game
            
            //change environment
            ChangeEnvironment();

            ChangePersistentVariables();
            
            //debug
            Debug.Log($"{entry.Key}, {entry.Value}");
            ResetLevel();
        }
    }

    private IEnumerator StartRound()
    {
        //begin count down
        for (int i = _countDownNumber; i > 0; i--)
        {
            HandleCountDown(i);
            yield return new WaitForSeconds(1);
        }
        
        //play any starting anims -> do we need to wait for it?
    }

    private void HandleCountDown(int i)
    {
        //print number to screen ?
        
        //any other animations
    }

    private void ChangeEnvironment()
    {
        //change background in game state manager
    }

    private void ChangePersistentVariables()
    {
        //change any persistent variables in game state manager
    }

    private void ResetLevel()
    {
        //reset level
    }
}
