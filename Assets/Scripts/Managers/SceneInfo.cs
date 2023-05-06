using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

public class SceneInfo : MonoBehaviour
{
    public static SceneInfo Instance { get; private set; }
    public static bool IsTraining { get; private set; }

    [SerializeField] private BuildSettingIndices _indices;

    private void Awake()
    {
        CreateSingleton();

        int index = SceneManager.GetActiveScene().buildIndex;
        IsTraining = index == _indices.trainingScene;
    }

    private void CreateSingleton()
    {
        if (Instance)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }
}
