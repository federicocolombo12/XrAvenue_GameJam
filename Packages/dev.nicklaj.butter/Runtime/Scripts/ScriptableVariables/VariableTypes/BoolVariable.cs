using System;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(fileName = "Bool Variable", menuName = "Butter/Variables/Boolean")]
    public class BoolVariable : ScriptableVariable<bool>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }

        public string Serialize()
        {
            return Value.ToString();
        }

        public void Deserialize(string data)
        {
            Value = bool.Parse(data);
        }

        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
        
        private void Awake()
        {
            //if(string.IsNullOrEmpty(PersistencyId))
                //PersistencyId = Guid.NewGuid().ToString();
        }
    }
}