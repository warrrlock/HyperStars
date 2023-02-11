using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadManager : MonoBehaviour
{
    private Gamepad[] _gamepads;

    void Start()
    {
        _gamepads[0].SetMotorSpeeds(0.25f, 0.75f);
    }

    void Update()
    {
        
    }
}
