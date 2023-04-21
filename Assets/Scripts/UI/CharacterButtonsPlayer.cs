using System;
using System.Collections.Generic;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
class SelectionAssets
{
    public CharacterManager.CharacterSelection character;
    public string animStateName;
}

public class CharacterButtonsPlayer: MonoBehaviour
{
    [SerializeField] private bool _isBot;
    [SerializeField] private CharacterManager _characterManager;
    [SerializeField] private CharacterSelectManager _selectionManager;
    [SerializeField] private int _playerId;
    [SerializeField] private Image _characterSelectionImage;
    [SerializeField] private PalettePicker _palettePicker;
    private Animator _charVisualAnimator;
    [FormerlySerializedAs("_images")] [SerializeField] private List<SelectionAssets> _characterVisuals;
    [SerializeField] private GameObject _readyVisual;
    
    public Player Player { get; private set; }
    public bool IsBot => _isBot;
    public int PlayerId => _playerId;

    private Player _originPlayer;

    private void Awake()
    {
        _charVisualAnimator = _characterSelectionImage.GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        _originPlayer.onReady -= UpdateReadyVisuals;
        _originPlayer.unReady -= UpdateReadyVisuals;
    }

    public void SetPlayer(Player p)
    {
        if (!Player)
        {
            _originPlayer = p;
            _originPlayer.onReady += UpdateReadyVisuals;
            _originPlayer.unReady += UpdateReadyVisuals;
        }
        Player = p;
    }
    
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
        SelectionAssets selectionAssets = _characterVisuals.Find(pair => 
            pair.character.ToString().Equals(character, StringComparison.OrdinalIgnoreCase));
        if (_characterSelectionImage) _charVisualAnimator.Play(selectionAssets.animStateName);
        if (_selectionManager) _selectionManager.UpdateSelection(character, selectionAssets.character, _playerId);
    }

    private void UpdateReadyVisuals()
    {
        if (_readyVisual)
            _readyVisual.SetActive(_originPlayer.Ready); //TODO: replace with animations/ui input module change}
    }
    
    public void GetReady()
    {
        // Debug.Log("get ready");
        StartCoroutine(Player.GetReady());
    }
}