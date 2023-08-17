using System;
using System.Collections.Generic;
using Managers;
using SFX;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
class SelectionAssets
{
    public CharacterManager.CharacterName character;
    public string animStateName;
    public CharacterColorScriptable palette;
}

public class CharacterButtonsPlayer: MonoBehaviour
{
    [Header("meta")]
    [SerializeField] private bool _isBot;
    [SerializeField] private int _playerId;
    
    [Header("visuals")]
    [SerializeField] private Image _characterSelectionImage;
    [SerializeField] private List<SelectionAssets> _characterVisuals;
    [SerializeField] private GameObject _readyVisual;
    
    [Header("managers")]
    [SerializeField] private PalettePicker _palettePicker;
    [SerializeField] private CharacterManager _characterManager;
    [SerializeField] private CharacterSelectManager _selectionManager;

    [Header("Character VO Switches")]
    [SerializeField] private AK.Wwise.Switch[] characterSwitches;
    private AK.Wwise.Switch selectedCharacterSwitch;
    
    public Player Player { get; private set; }
    public bool IsBot => _isBot;
    public int PlayerId => _playerId;
    public PostWwiseUIEvent WwiseUIEvent => _wwiseUIEvent;

    private Player _originPlayer;
    private Animator _charVisualAnimator;
    private PostWwiseUIEvent _wwiseUIEvent;
    

    private void Awake()
    {
        _charVisualAnimator = _characterSelectionImage.GetComponent<Animator>();
        _wwiseUIEvent = GetComponent<PostWwiseUIEvent>();
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
        _characterManager.Characters.TryGetValue(CharacterManager.CharacterName.Lisa, out Character character);
        selectedCharacterSwitch = characterSwitches[0];
        SelectCharacter(character);
    }

    public void SelectBluk()
    {
        _characterManager.Characters.TryGetValue(CharacterManager.CharacterName.Bluk, out Character character);
        selectedCharacterSwitch = characterSwitches[1];
        SelectCharacter(character);
    }

    private void SelectCharacter(Character character)
    {
        if (!character) return;
        if (_isBot) Player.SelectBot(character);
        else Player.SelectCharacter(character);
        _wwiseUIEvent.PostSubmit();
    }
    
    public void UpdateCharacterSelect(string character)
    {
        SelectionAssets selectionAssets = _characterVisuals.Find(pair => 
            pair.character.ToString().Equals(character, StringComparison.OrdinalIgnoreCase));
        _charVisualAnimator.Play(selectionAssets.animStateName);
        _selectionManager.UpdateSelection(character, selectionAssets.character, _playerId);
        _palettePicker.SetMaterialColours(0, selectionAssets.palette);
        _wwiseUIEvent.PostHover();
    }

    private void UpdateReadyVisuals()
    {
        if (_originPlayer.Ready)
        { 
            _wwiseUIEvent.characterSwitch = selectedCharacterSwitch;
            _wwiseUIEvent.PostLockIn();
        }
        if (_readyVisual)
            _readyVisual.SetActive(_originPlayer.Ready); //TODO: replace with animations/ui input module change}
    }
    
    public void GetReady()
    {
        // Debug.Log("get ready");
        StartCoroutine(Player.GetReady());
    }
}