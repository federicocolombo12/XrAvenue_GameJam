using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    [System.Serializable]
    public class WasteParticleMapping
    {
        public WasteType type;
        public GameObject particlePrefab;
    }

    [CreateAssetMenu(fileName = "WasteParticleData", menuName = "AvenueXR/Particles/Waste Particle Data")]
    public class WasteParticleData : ScriptableObject
    {
        public List<WasteParticleMapping> particleMappings = new List<WasteParticleMapping>();

        public GameObject GetParticlePrefabForType(WasteType type)
        {
            foreach (var mapping in particleMappings)
            {
                if (mapping.type == type) return mapping.particlePrefab;
            }
            return null;
        }
    }
}
