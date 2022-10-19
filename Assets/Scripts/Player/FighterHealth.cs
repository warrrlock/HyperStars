using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FighterHealth : MonoBehaviour
{
    [SerializeField] private float _maxHitPoints;
    [SerializeField] private TextMeshProUGUI _textBox;

    private float _hitPoints;

    private void Start()
    {
        _hitPoints = _maxHitPoints;
        if (_textBox) _textBox.text = "Health: " + _hitPoints;
    }

    public void ApplyDamage(float damagePoints)
    {
        _hitPoints -= damagePoints;
        //Debug.Log("Fighter lost " + damagePoints + " hit points.");
        if (_textBox) _textBox.text = "Health: " + _hitPoints;
    }
}
