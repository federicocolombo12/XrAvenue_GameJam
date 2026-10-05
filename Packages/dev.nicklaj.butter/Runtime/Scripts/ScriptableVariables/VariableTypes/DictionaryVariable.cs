using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Dev.Nicklaj.Butter
{
    /// <summary>
    /// Extendable class for creating dictionary variables.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Obsolete]
    public class DictionaryVariable<TKey, TValue> : ScriptableVariable<Dictionary<TKey, TValue>>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }
        public UnityAction OnItemAdded = delegate { };
        public UnityAction OnItemRemoved = delegate { };

        /// <summary>
        /// Adds a new key-value pair to the dictionary and invokes the OnItemAdded event.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            Value.Add(key, value);
            OnItemAdded.Invoke();
        }

        /// <summary>
        /// Removes a key-value pair from the dictionary and invokes the OnItemRemoved event.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            Value.Remove(key);
            OnItemRemoved.Invoke();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(Value);
        }

        public void Deserialize(string data)
        {
            Value = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(data);
        }
        
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
    }
}