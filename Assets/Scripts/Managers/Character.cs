using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class FSMFilter
    {
        public string filterName;
        public Color color;
    }
    
    public class Character: ScriptableObject
    {
        public HashSet<FSMFilter> Filters { get; private set; }
        [SerializeField] private GameObject _characterPrefab;
        
        //TODO: limit filter selection for states to its character
        //TODO: when deleting a filter, go through folder and move assets to correct folder
    }
}