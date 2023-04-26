using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{

    //Stores references to singletons that can be referenced from anywhere.

    //public static Boss Boss;
    //public static Player Player;
    //public static UIManager UIManager;
    //public static BeatIndicatorBrain BeatIndicatorBrain;
    //public static TutorialManager TutorialManager;
    //public static ParticleMaker ParticleMaker;
    //public static GameObject FailScreen;
    //public static GameObject PauseScreen;
    public static ComboIndicator comboIndicator;
    public static HitIndicator hitIndicator;
    //public static CenterEffectManager CenterEffectManager;

    //public static bool Tutorial;//Whether the tutorial is currently running
    //todo: move this to gamemanager, since it's game state?
}
