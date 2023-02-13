using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonsPlayer: MonoBehaviour
{
    [SerializeField] private CharacterManager _manager;
    [SerializeField] private int _playerId;
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
    
    public void GetReady()
    {
        Player.GetReady();
    }
}