using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighters Manager", menuName = "ScriptableObjects/Fighters Manager")]
public class FightersManager : ScriptableObject
{
    public Vector3 player1StartPosition;
    public Vector3 player2StartPosition;
    public Color player1Color;
    public Color player2Color;
    [HideInInspector] public Color[] playerColors;

    public void OnValidate()
    {
        playerColors = new[] { player1Color, player2Color };
    }
}
