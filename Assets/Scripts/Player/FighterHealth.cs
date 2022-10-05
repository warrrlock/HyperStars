using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [SerializeField] private float _maxHitPoints;
    private float _hitPoints;

    private void Start()
    {
        _hitPoints = _maxHitPoints;
    }

    public void ApplyDamage(float damagePoints)
    {
        _hitPoints -= damagePoints;
    }
}
