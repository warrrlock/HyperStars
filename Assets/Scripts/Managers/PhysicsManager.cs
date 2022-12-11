using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Physics Manager", menuName = "ScriptableObjects/Physics Manager")]
public class PhysicsManager : ScriptableObject
{
    public float deceleration;
    public float gravity;


    private void OnEnable()
    {
        
    }
}
