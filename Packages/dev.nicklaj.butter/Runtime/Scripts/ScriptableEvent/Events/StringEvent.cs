using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(menuName = "Butter/Events/String Event")]
    public class StringEvent : GameEvent<string>, ISerializedRaise
    {
        public bool HasArgument => true;

        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0)
        {
            this.Raise(arg0, channel);
        }
    }
}