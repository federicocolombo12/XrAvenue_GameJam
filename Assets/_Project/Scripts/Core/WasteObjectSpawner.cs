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
            Debug.Log($"[WasteObjectSpawner] Tentativo di spawn per il tipo: {type}");
            GameObject prefab = GetPrefabForType(type);

            if (prefab != null)
            {
                if (spawnPoint == null)
                {
                    Debug.LogError("[WasteObjectSpawner] SpawnPoint non assegnato nell'Inspector!");
                    return;
                }

                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                Debug.Log($"[WasteObjectSpawner] Spawn riuscito: {prefab.name} a {spawnPoint.position}");
            }
            else
            {
                Debug.LogWarning($"[WasteObjectSpawner] Nessun prefab assegnato per il tipo: {type}");
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
