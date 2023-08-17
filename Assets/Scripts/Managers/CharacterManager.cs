using System;
using System.Collections.Generic;
using System.Linq;
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
        public enum CharacterSelection {None, Lisa, Bluk, Byeol}
        [SerializeField] private List<KeyCharacterPair> _characterObjects;

        public Dictionary<CharacterSelection, Character> Characters => _characterObjects.ToDictionary(
            o => o.characterSelection, o => o.character);
        //TODO: create new character method
    }
}