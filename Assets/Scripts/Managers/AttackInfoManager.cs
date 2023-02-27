using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInfoManager : MonoBehaviour
{
    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            int length = Services.Characters[i].AttackInfos.Count;
            for (int j = 0; j < length; j++)
            {
                Services.Characters[i].AttackInfos[j].Initialize();
            }
        }
    }
}
