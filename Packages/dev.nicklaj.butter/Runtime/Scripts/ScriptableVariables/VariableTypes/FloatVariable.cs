using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(fileName = "Float Variable", menuName = "Butter/Variables/Float")]
    public class FloatVariable : ScriptableVariable<float>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }

        public string Serialize()
        {
            return Value.ToString();
        }

        public void Deserialize(string data)
        {
            Value = float.Parse(data);
        }
        
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
    }
}