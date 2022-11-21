using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GameEventListener))]
public class RoundManager : MonoBehaviour
{
    [SerializeField] private GameObject _restartButton;
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private string _startText;
    [SerializeField] private int _neededWins;
    
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] int _countdown;
    
    [SerializeField] private TextMeshProUGUI[] _winText; //TODO: remove when we have ui
    [SerializeField] List<Sprite> _backgrounds; //TODO: add when we have changing backgrounds
    
    private int _round;
    private bool _disabledInput;

    private void Awake()
    {
        if (_restartButton) _restartButton.SetActive(false);
        if (_roundText) _roundText.gameObject.SetActive(false);
        _round = RoundInformation.round;
        if (_winText.Length >= 2) //TODO: remove
        {
            _winText[0].text = $"Won {RoundInformation.Wins[0]}/{_neededWins}";
            _winText[1].text = $"Won {RoundInformation.Wins[1]}/{_neededWins}";
        }
    }

    private void Start()
    {
        StartCoroutine(StartRound());
    }

    private void OnDestroy()
    {
        EnableAllInput();
    }
    
    //used in event (inspector)
    public void OnRoundEnd(Dictionary<string, object> data)
    {
        Debug.Log("round ended");
        data.TryGetValue("winnerId", out object winnerId);
        int winner = winnerId == null ? -1 : (int)winnerId;

        HandleAddWinTo(winner);
        RoundInformation.SetIfMatchPoint(_neededWins, winner);
        
        if (RoundInformation.CheckWinner(_neededWins, winner))
        {
            EndGame();
            return;
        }

        if (_roundText)
        {
            _roundText.gameObject.SetActive(true);
            _roundText.text = winner == -1 ? "Tie!" : $"Player{winner+1} won the round!";
        }

        StartCoroutine(HandleStartNextRound());
    }
    
    //for use in animation
    public void EnableRestartGame()
    {
        _restartButton.SetActive(true);
    }
    
    public void RestartGame()
    {
        Debug.Log("restarting");
        RoundInformation.ResetRounds();
    }

    private void HandleAddWinTo(int player)
    {
        RoundInformation.AddWinTo(player);
        if (_winText.Length >= 2) _winText[player].text = $"Won {RoundInformation.Wins[player]}/{_neededWins}"; //TODO: remove
    }

    private IEnumerator HandleStartNextRound()
    {
        //disable movement
        DisableAllInput();
        
        //TODO: any ui updates
        SetNewRoundVariables();
        
        yield return new WaitForSeconds(1f);
        SceneReloader.Instance?.ReloadScene();
    }

    private void SetNewRoundVariables()
    {
        //change any persistent variables
        RoundInformation.round = ++_round;
        //ChangeEnvironment;
    }
    
    private IEnumerator StartRound()
    {
        if (!_countdownText) yield break;
        _countdownText.gameObject.SetActive(true);
       
        //stop time/movement
        DisableAllInput();

        //begin count down
        _countdownText.text = $"Round {_round}";
        yield return new WaitForSeconds(1.0f);
        if (RoundInformation.MatchPoint)
        {
            _countdownText.text = "Match point!";
            yield return new WaitForSeconds(0.8f);
            
            _countdownText.text = $@"{(RoundInformation.MatchPointPlayers[0] ? 
                (RoundInformation.MatchPointPlayers[1] ? "Scores Tied!" : "Player 1 in the lead.")
                : "Player2 in the lead")}";
            yield return new WaitForSeconds(0.8f);
        }

        for (int i = _countdown; i > 0; i--)
        {
            HandleCountDown(i);
            yield return new WaitForSeconds(1.0f);
        }
        
        //TODO: any necessary UI
        
        //start time/movement
        _countdownText.text = _startText;
        EnableAllInput();
        
        yield return new WaitForSeconds(1.0f);
        _countdownText.gameObject.SetActive(false);
    }

    private void DisableAllInput()
    {
        _disabledInput = true;
        foreach (Fighter fighter in Services.Fighters)
            fighter.DisableAllInput(() => _disabledInput == false);
    }

    private void EnableAllInput()
    {
        _disabledInput = false;
    }

    private void HandleCountDown(int i)
    {
        _countdownText.text = $"{i}";
        //TODO: other UI
    }

    private void EndGame()
    {
        //TODO: disable all input
        DisableAllInput();
        AnnounceWinner();
    }

    private void AnnounceWinner()
    {
        int winner = RoundInformation.GetWinner();
        int loser = winner == 0 ? 1 : 0;
        //lose animation -> the losing character dies lol
        //Win animation -> the character walks up closer to the camera and does a victory pose. 
        
        _roundText.gameObject.SetActive(true);
        
        _roundText.text = $"Player {winner+1} is victorious!";
        EnableRestartGame(); //TODO: remove after we have win/lose animations
    }
}
