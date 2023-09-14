using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class KeyCharacterPair
    {
        public CharacterManager.CharacterName characterName;
        public Character character;
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/Character Manager")]
    public class CharacterManager: ScriptableObject
    {
        public enum CharacterName {None, Lisa, Bluk, Byeol, TheHand}
        [SerializeField] private List<KeyCharacterPair> _characterObjects;

        public Dictionary<CharacterName, Character> Characters => _characterObjects.ToDictionary(
            o => o.characterName, o => o.character);
        //TODO: create new character method
    }
}