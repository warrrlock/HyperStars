using System;
using System.Collections.Generic;
using Managers;
using UI;
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
    [SerializeField] private bool _isBot;
    [SerializeField] private CharacterManager _characterManager;
    [SerializeField] private CharacterSelectManager _selectionManager;
    [SerializeField] private int _playerId;
    [SerializeField] private Image _characterSelectionImage;
    [SerializeField] private List<SelectionSpritePair> _images;
    
    public Player Player { get; set; }
    public bool IsBot => _isBot;
    public int PlayerId => _playerId;
    
    public void SelectLisa()
    {
        _characterManager.Characters.TryGetValue(CharacterManager.CharacterSelection.Lisa, out Character character);
        SelectCharacter(character);
    }

    public void SelectBluk()
    {
        _characterManager.Characters.TryGetValue(CharacterManager.CharacterSelection.Bluk, out Character character);
        SelectCharacter(character);
    }

    private void SelectCharacter(Character character)
    {
        if (!character) return;
        if (_isBot) Player.SelectBot(character);
        else Player.SelectCharacter(character);
    }
    
    public void UpdateCharacterSelect(string character)
    {
        SelectionSpritePair selectionSpritePair = _images.Find(pair => 
            pair.character.ToString().Equals(character, StringComparison.OrdinalIgnoreCase));
        if (_characterSelectionImage) _characterSelectionImage.sprite = selectionSpritePair.image;
        if (_selectionManager) _selectionManager.UpdateSelection(selectionSpritePair.character, _playerId);
    }
    
    public void GetReady()
    {
        StartCoroutine(Player.GetReady());
    }
}