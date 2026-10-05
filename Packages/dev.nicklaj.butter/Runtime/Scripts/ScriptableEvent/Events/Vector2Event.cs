using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(menuName = "Butter/Events/Vector2 Event")]
    public class Vector2Event : GameEvent<Vector2>, ISerializedRaise
    {
        public bool HasArgument => true;

        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0)
        {
            // Try to parse the input as a vector2
            var tmp = arg0.Replace("(", "").Replace(")", "").Split(',');
            if (float.TryParse(tmp[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) == false || 
                float.TryParse(tmp[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) == false)
            {
                {
                    Debug.LogError("Failed to parse the event data. Make sure the parameters for your vector2 event are correct.");
                    return;
                }
            }
            this.Raise(new Vector2(x,y), channel);
        }
    }
}