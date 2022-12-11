using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager: MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void StartTraining()
    {
        SceneManager.LoadScene(2);
    }

    public void OpenSettings()
    {
        //TODO: create settings
    }
}
