using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(menuName = "Butter/Events/Float Event")]
    public class FloatEvent : GameEvent<float>, ISerializedRaise
    {
        public bool HasArgument => true;

        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0)
        {
            // Try to parse the input as a float
            if (float.TryParse(arg0, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) == false) return;
            this.Raise(result, channel);
        }
    }
}
