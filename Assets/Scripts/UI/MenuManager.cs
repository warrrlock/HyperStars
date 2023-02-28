using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager: MonoBehaviour
{
    private Player[] _players = new Player[2];
    [SerializeField] private Button _startButton;
    private bool _allowStart;

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
        // if (CheckReady())
            // _startButton.gameObject.SetActive(true);
    }

    private bool CheckReady()
    {
        return _players.All(p => !p || p.Ready);
    }
    
    public void StartCharacterSelection()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    
    public void StartMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    
    public void StartGame()
    {
        if (!CheckReady())
        {
            Debug.Log("players are not ready");
            return;
        }
        Time.timeScale = 1;
        SceneManager.LoadScene(2);
    }

    public void StartTraining()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(3);
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
