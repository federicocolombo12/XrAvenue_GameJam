using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public interface IPersistentData
    {
        public string PersistencyId { get; set; }
        public string Serialize();
        public void Deserialize(string data);
    }
}