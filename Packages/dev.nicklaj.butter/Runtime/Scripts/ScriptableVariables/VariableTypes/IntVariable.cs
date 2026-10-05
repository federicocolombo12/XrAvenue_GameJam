using System;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter 
{
    [CreateAssetMenu(fileName = "Int Variable", menuName = "Butter/Variables/Integer")]
    public class IntVariable : ScriptableVariable<int>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }

        public string Serialize()
        {
            return Value.ToString();
        }

        public void Deserialize(string data)
        {
            Value = int.Parse(data);
        }
        
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
    }
}