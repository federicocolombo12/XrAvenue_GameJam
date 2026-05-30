using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    public class WasteObjectSpawner : MonoBehaviour
    {
        public Transform spawnPoint;
        
        [Header("Prefabs Normali")]
        public List<GameObject> paperPrefabs = new List<GameObject>();
        public List<GameObject> plasticPrefabs = new List<GameObject>();
        public List<GameObject> glassPrefabs = new List<GameObject>();
        public List<GameObject> metalPrefabs = new List<GameObject>();
        public List<GameObject> organicPrefabs = new List<GameObject>();

        [Header("Prefabs Speciali")]
        public List<GameObject> moralWastePrefabs = new List<GameObject>();
        public List<GameObject> goreWastePrefabs = new List<GameObject>();
        public List<GameObject> bombPrefabs = new List<GameObject>();

        public void Spawn(WasteType type)
        {
            Debug.Log($"[WasteObjectSpawner] Tentativo di spawn per il tipo: {type}");
            GameObject prefab = GetRandomPrefabForType(type);

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
                Debug.LogWarning($"[WasteObjectSpawner] Nessun prefab assegnato o lista vuota per il tipo: {type}");
            }
        }

        private GameObject GetRandomPrefabForType(WasteType type)
        {
            List<GameObject> targetList = type switch
            {
                WasteType.Paper => paperPrefabs,
                WasteType.Plastic => plasticPrefabs,
                WasteType.Glass => glassPrefabs,
                WasteType.Metal => metalPrefabs,
                WasteType.Organic => organicPrefabs,
                WasteType.Moral => moralWastePrefabs,
                WasteType.Gore => goreWastePrefabs,
                WasteType.Bomb => bombPrefabs,
                _ => null
            };

            if (targetList == null || targetList.Count == 0)
                return null;

            int randomIndex = Random.Range(0, targetList.Count);
            return targetList[randomIndex];
        }
    }
}
