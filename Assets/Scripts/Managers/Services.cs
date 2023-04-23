using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public static class Services
{
    public static FightersManager FightersManager;
    public static Player[] Players = new Player[2];
    public static Fighter[] Fighters = new Fighter[2];
    public static Character[] Characters = new Character[2];

    public static FavorManager FavorManager;
    public static CameraManager CameraManager;
    public static RoundTimer RoundTimer;
    public static CollisionsManager CollisionsManager;
    public static AttackInfoManager AttackInfoManager;
    
    public static MusicManager MusicManager;
}
