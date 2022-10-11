using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _fighterDebugTextBoxes = new TextMeshProUGUI[4];

    private void Update()
    {
        for (int i = 0; i < _fighterDebugTextBoxes.Length; i++)
        {
            StringBuilder debugText = new();
        }
    }
}
