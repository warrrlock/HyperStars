using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "Command List")]
    public class CharacterCmmdList: ScriptableObject
    {
        [Serializable]
        public class CommandList
        {
            public string name;
            public List<string> commands;
        }
        
        [SerializeField] public List<CommandList> commandList;
    }
}