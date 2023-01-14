using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class KeyCharacterPair
    {
        public CharacterManager.CharacterSelection characterSelection;
        public Character character;
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/Character Manager")]
    public class CharacterManager: ScriptableObject
    {
        public enum CharacterSelection {None, Lisa }
        //TODO: get all existing characters at start
        [SerializeField] private List<KeyCharacterPair> _characterObjects;

        public List<KeyCharacterPair> Characters => _characterObjects;
        //TODO: create new character method
    }
}