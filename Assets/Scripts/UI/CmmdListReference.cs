using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class ActionSpritePair
    {
        public string action;
        public Sprite sprite;
    }
    
    [CreateAssetMenu(menuName = "Command Reference")]
    public class CmmdListReference: ScriptableObject
    {
        [SerializeField] public List<ActionSpritePair> actionSpritePairs;
    }
}