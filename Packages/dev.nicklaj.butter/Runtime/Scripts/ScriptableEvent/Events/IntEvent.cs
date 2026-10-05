using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(menuName = "Butter/Events/Integer Event")]
    public class IntEvent : GameEvent<int>, ISerializedRaise
    {
        public bool HasArgument => true;

        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0)
        {
            if (int.TryParse(arg0, out int result) == false) return;
            this.Raise(result, channel);
        }
    }
}