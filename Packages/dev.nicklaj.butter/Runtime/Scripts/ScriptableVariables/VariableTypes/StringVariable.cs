using System;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(fileName = "String Variable", menuName = "Butter/Variables/String")]
    public class StringVariable : ScriptableVariable<string>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }

        public string Serialize()
        {
            return Value;
        }

        public void Deserialize(string data)
        {
            Value = data;
        }
        
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
    }
}