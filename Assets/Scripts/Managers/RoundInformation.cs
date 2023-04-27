using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

public class RoundInformation : MonoBehaviour
{
    [SerializeField] private BuildSettingIndices _indices;
    public static RoundInformation Instance;
    public static int round = 1;
    public static readonly int[] Wins = new int[2];
    public static bool MatchPoint { get; private set;}
    public static bool[] MatchPointPlayers { get; private set;}

    // Start is called before the first frame update
    private void Start() {
        Tinylytics.AnalyticsManager.LogCustomMetric("New_Started", DateTime.Now.ToString());
    }

    private void Awake()
    {
        CreateSingleton();
        
    }
    
    private void CreateSingleton()
    {
        if (Instance)
            Destroy(gameObject);
        else
        {
            ResetRounds();
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += CheckDestroy;
        }
            
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= CheckDestroy;
    }

    private void CheckDestroy(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex != _indices.gameScene) Destroy(gameObject);
        
    }

    public static void AddWinTo(int player)
    {
        if (player != -1)
        {
            Wins[player]++;

            Tinylytics.AnalyticsManager.LogCustomMetric("New_Round_Ended", DateTime.Now.ToString());
        }
    }

    public static void SetIfMatchPoint(int neededWins, int player)
    {
        if (player != -1 && !MatchPoint)
        {
            MatchPoint = Wins[player] == neededWins - 1;
            MatchPointPlayers[player] = true;
        }
    }
    
    public static bool CheckWinner(int neededWins, int player)
    {
        return player != -1 && Wins[player] >= neededWins;
    }

    public static int GetWinner()
    {
        
        return Wins[0] > Wins[1] ? 0 : 1;
    }

    public static void ResetRounds()
    {
        round = 1;
        Wins[0] = 0;
        Wins[1] = 0;
        MatchPoint = false;
        MatchPointPlayers = new bool[2];
        FindObjectOfType<SceneReloader>().ReloadScene();
    }
}
