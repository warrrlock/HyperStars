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
            p.subscription -= ShowStartButton;
        }
    }

    public void AddPlayer(Player p)
    {
        _players[p.PlayerInput.playerIndex] = p;
        p.subscription += ShowStartButton;
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
        SceneManager.LoadScene(1);
    }
    
    public void StartGame()
    {
        if (!CheckReady()) //TODO: REMOVE, only show button when players ready
        {
            Debug.Log("players are not ready");
            return;
        }
        SceneManager.LoadScene(2);
    }

    public void StartTraining()
    {
        SceneManager.LoadScene(3);
    }

    public void OpenSettings()
    {
        //TODO: create settings
    }
}
