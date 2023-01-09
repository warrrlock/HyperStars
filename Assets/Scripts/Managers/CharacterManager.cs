using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class KeyCharacterPair
    {
        public CharacterManager.CharacterSelection selection;
        public Character character;
    }
    public class CharacterManager: MonoBehaviour
    {
        public enum CharacterSelection {None, Lisa }
        //TODO: get all existing characters at start
        [SerializeField]private List<KeyCharacterPair> _characterObjects;
        //TODO: create new character method
    }
}