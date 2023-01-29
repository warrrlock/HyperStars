using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();
        [SerializeField]
        private List<TValue> _values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach(KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }
        
        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
                this.Add(_keys[i], _values[i]);
        }
    }
}