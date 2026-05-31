using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    [System.Serializable]
    public class WasteAudioMapping
    {
        public WasteType type;
        public AudioClip crushSound;
    }

    [CreateAssetMenu(fileName = "WasteAudioData", menuName = "AvenueXR/Audio/Waste Audio Data")]
    public class WasteAudioData : ScriptableObject
    {
        public List<WasteAudioMapping> audioMappings = new List<WasteAudioMapping>();

        public AudioClip GetAudioForType(WasteType type)
        {
            foreach (var mapping in audioMappings)
            {
                if (mapping.type == type) return mapping.crushSound;
            }
            return null;
        }
    }
}
