using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterPortrait: MonoBehaviour
    {
        [SerializeField] private int _playerIndex;
        private void Start()
        {
            GetComponent<Image>().sprite = Services.Characters[_playerIndex].CharacterPortrait[_playerIndex];
        }
    }
}