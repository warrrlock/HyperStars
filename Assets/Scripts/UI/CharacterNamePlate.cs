using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterNamePlate : MonoBehaviour
    {
        [SerializeField] private int _playerIndex;
        private void Start()
        {
            GetComponent<Image>().sprite = Services.Characters[_playerIndex].NamePlates[_playerIndex];
        }
    }
}