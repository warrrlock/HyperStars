using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace UI
{
    public class LoadingScreen: MonoBehaviour
    {
        [SerializeField] private BuildSettingIndices _indices;
        private int _initialScene;
        private Animator _animator;
        private static readonly int Enter = Animator.StringToHash("enter");

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _initialScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.sceneLoaded += CheckClose;
            _animator = GetComponent<Animator>();
        }

        private void CheckClose(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex != _initialScene)
            {
                _animator.SetTrigger(Enter);
            }
        }

        public void DestroyInstance()
        {
            Destroy(gameObject);
        }
    }
}