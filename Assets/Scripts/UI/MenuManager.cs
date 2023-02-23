using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager: MonoBehaviour
{
    private Player[] _players = new Player[2];
    [SerializeField] private Button _startButton;

    private void OnDestroy()
    {
        foreach (Player p in _players)
        {
            if (p) p.onReady -= ShowStartButton;
        }
    }

    public void AddPlayer(Player p)
    {
        _players[p.PlayerInput.playerIndex] = p;
        p.onReady += ShowStartButton;
    }

    private void ShowStartButton()
    {
        if (CheckReady())
            _startButton.gameObject.SetActive(true);
    }

    private bool CheckReady()
    {
        return _players.All(p => p != null && p.Ready);
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
        Time.timeScale = 1;
        if (!CheckReady())
        {
            Debug.Log("players are not ready");
            return;
        }
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
