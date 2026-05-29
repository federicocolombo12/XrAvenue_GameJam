using UnityEngine;

namespace AvenueXR.Core
{
    public class WasteObjectSpawner : MonoBehaviour
    {
        public Transform spawnPoint;
        
        [Header("Prefabs Normali")]
        public GameObject paperPrefab;
        public GameObject plasticPrefab;
        public GameObject glassPrefab;
        public GameObject metalPrefab;
        public GameObject organicPrefab;

        [Header("Prefabs Speciali")]
        public GameObject moralWastePrefab;
        public GameObject goreWastePrefab;
        public GameObject bombPrefab;

        public void Spawn(WasteType type)
        {
            GameObject prefab = GetPrefabForType(type);

            if (prefab != null)
            {
                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            }
        }

        private GameObject GetPrefabForType(WasteType type)
        {
            return type switch
            {
                WasteType.Paper => paperPrefab,
                WasteType.Plastic => plasticPrefab,
                WasteType.Glass => glassPrefab,
                WasteType.Metal => metalPrefab,
                WasteType.Organic => organicPrefab,
                WasteType.Moral => moralWastePrefab,
                WasteType.Gore => goreWastePrefab,
                WasteType.Bomb => bombPrefab,
                _ => null
            };
        }
    }
}
