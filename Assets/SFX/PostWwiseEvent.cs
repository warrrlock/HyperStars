using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComboEvents
{
    COMBO1_INITIAL, COMBO1_TRANSITION, COMBO2, COMBO3, COMBO4, COMBO5
}
public class PostWwiseEvent : MonoBehaviour
{
    public AK.Wwise.Event[] comboEvents;

    public void PlayComboSound(ComboEvents cEvent)
    {
        comboEvents[(int)cEvent].Post(gameObject);
    }
}
