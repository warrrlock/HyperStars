using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameEventListener))]
public class RoundManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private int _maxRounds;
    
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] int _countdown;
    
    [SerializeField] List<Sprite> _backgrounds;
    
    private int _round;
    private bool _endOfRound;

    private void Awake()
    {
        //get persistent values from game manager
        //reset vars
        if (_roundText) _roundText.gameObject.SetActive(false);
    }

    private void Start()
    {
        //start coroutine counts down w/ announcer to start round
        StartCoroutine(StartRound());
    }

    private void Update()
    {
        if (_endOfRound)
            if (Input.GetKeyDown(KeyCode.R))
                ResetLevel();

    }

    public void OnRoundEnd(Dictionary<string, object> data)
    {
        data.TryGetValue("winnerId", out object winnerId);
        if (_roundText)
        {
            _roundText.gameObject.SetActive(true);
            if (winnerId != null) _roundText.text = $"Player{(int)winnerId} won the round! Press <R> to restart.";
        }
        //if done max rounds, end game
        _round++;
        //set persistent variables
        ChangePersistentVariables();
        
        // foreach (var entry in data)
        // {
        //     //debug
        //     Debug.Log($"{entry.Key}, {entry.Value}");
        // }
        
        //TODO: reset level
        // ResetLevel();
        _endOfRound = true;
    }

    private IEnumerator StartRound()
    {
        //TODO: stop time/movement (note: if we pause the game countdown should pause)
        //begin count down
        for (int i = _countdown; i > 0; i--)
        {
            HandleCountDown(i);
            yield return new WaitForSeconds(1);
        }
        //play any starting anims -> do we need to wait for it?
        
        if (_countdownText)
        {
            _countdownText.text = "Start!";
            //TODO: start time/movement
            yield return new WaitForSeconds(0.5f);
            _countdownText.gameObject.SetActive(false);
        }
        
    }

    private void HandleCountDown(int i)
    {
        //print number to screen ?
        if (_countdownText) _countdownText.text = $"{i}";
        //any other animations
    }

    private void ChangeEnvironment()
    {
        //change background in game state manager
    }

    private void ChangePersistentVariables()
    {
        //change any persistent variables in game state manager
        ChangeEnvironment();
    }

    private void ResetLevel()
    {
        SceneReloader.Instance?.ReloadScene();
        //reset level
    }
}
