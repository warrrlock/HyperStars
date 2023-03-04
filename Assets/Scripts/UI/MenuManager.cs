using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Util;

public class MenuManager: MonoBehaviour
{
    private Player[] _players = new Player[2];
    [SerializeField] private GameObject _playersReadyVisual;
    private bool _allowStart;
    [SerializeField] private BuildSettingIndices _indices;
    public bool IsTraining { get; private set; }

    private void Awake()
    {
        IsTraining = SceneManager.GetActiveScene().buildIndex == _indices.trainingSelectionScene;
    }

    private void OnDestroy()
    {
        foreach (Player p in _players)
        {
            if (p) p.onReady -= ShowStartGame;
        }
    }

    public void AddPlayer(Player p)
    {
        _players[p.PlayerInput.playerIndex] = p;
        p.onReady += ShowStartGame;
    }

    private void ShowStartGame()
    {
        if (CheckReady())
            if (_playersReadyVisual) _playersReadyVisual.SetActive(true);
    }

    private bool CheckReady()
    {
        // Debug.Log("checking ready");
        return Services.Players.All(p => !p || p.Ready);
    }
    
    public void StartCharacterSelection()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_indices.selectionScene);
    }
    
    public void StartMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_indices.mainMenuScene);
    }
    
    public void StartGame()
    {
        if (!CheckReady())
        {
            Debug.Log("players are not ready");
            return;
        }
        Time.timeScale = 1;
        SceneManager.LoadScene(_indices.gameScene);
    }

    public void StartTrainingSelection()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_indices.trainingSelectionScene);
    }
    
    public void StartTraining()
    {
        if (!CheckReady())
        {
            Debug.Log("players are not ready");
            return;
        }
        Time.timeScale = 1;
        SceneManager.LoadScene(_indices.trainingScene);
    }

    public void ReturnToCharacterSelect()
    {
        if (SceneManager.GetActiveScene().buildIndex == _indices.gameScene)
            StartCharacterSelection();
        else
            StartTrainingSelection();
    }

    public void OpenSettings()
    {
        //TODO: create settings
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game if not in editor");
        Application.Quit();
    }
}
