using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace SFX
{
    public class MenuMusicSingleton : MonoBehaviour
    {
        public static MenuMusicSingleton Instance;
        [SerializeField] private BuildSettingIndices _indices;
        [SerializeField] private AK.Wwise.Event _menuMusic;

        // Start is called before the first frame update
        private void Awake()
        {
            CreateSingleton();
        }

        private void Start()
        {
            _menuMusic?.Post(gameObject);
        }

        private void CreateSingleton()
        {
            if (Instance)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += CheckDestroy;
                
            }
        }

        private void CheckDestroy(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex == _indices.gameScene || scene.buildIndex == _indices.trainingScene)
            {
                // Debug.Log("will destroy menu music obj");
                SceneManager.sceneLoaded -= CheckDestroy;
                Destroy(gameObject);
            }
        }
    }
}