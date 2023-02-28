using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class SelectionSpritePair
{
    public CharacterManager.CharacterSelection character;
    public Sprite image;
}

public class CharacterButtonsPlayer: MonoBehaviour
{
    [SerializeField] private CharacterManager _manager;
    [SerializeField] private int _playerId;
    [SerializeField] private Image _characterSelectionImage;
    [SerializeField] private List<SelectionSpritePair> _images;
    public Player Player { get; set; }
    public int PlayerId => _playerId;
    
    public void SelectLisa()
    {
            _manager.Characters.TryGetValue(CharacterManager.CharacterSelection.Lisa, out Character character);
            if (character) Player.SelectCharacter(character);
    }

    public void SelectBluk()
    {
        _manager.Characters.TryGetValue(CharacterManager.CharacterSelection.Bluk, out Character character);
        if (character) Player.SelectCharacter(character);
    }
    
    public void SetCharacterDisplay(string character)
    {
        if (_characterSelectionImage) _characterSelectionImage.sprite = _images.Find(pair => 
            pair.character.ToString().Equals(character, StringComparison.OrdinalIgnoreCase)).image;
    }
    
    public void GetReady()
    {
        StartCoroutine(Player.GetReady());
    }
}