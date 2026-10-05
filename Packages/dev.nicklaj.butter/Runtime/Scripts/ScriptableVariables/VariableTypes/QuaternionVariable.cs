using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(fileName = "Quaternion Variable", menuName = "Butter/Variables/Quaternion")]
    public class QuaternionVariable : ScriptableVariable<Quaternion>, IPersistentData
    {
        [field: SerializeField, HideInInspector] public string PersistencyId { get; set; }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(Value, new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
        }

        public void Deserialize(string data)
        {
            Value = JsonConvert.DeserializeObject<Quaternion>(data, new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
        }
        
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PersistencyId))
                PersistencyId = Guid.NewGuid().ToString();
        }
    }
}