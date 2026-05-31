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
        public List<GameObject> cardPrefabs = new List<GameObject>();
        public List<GameObject> babyPrefabs = new List<GameObject>();
        public List<GameObject> boxPrefabs = new List<GameObject>();

        public void Spawn(WasteType type)
        {
            GameObject prefab = GetRandomPrefabForType(type);
            SpawnPrefab(prefab);
        }

        public void SpawnPrefab(GameObject prefab)
        {
            if (prefab != null)
            {
                if (spawnPoint == null)
                {
                    Debug.LogError("[WasteObjectSpawner] SpawnPoint non assegnato nell'Inspector!");
                    return;
                }

                GameObject spawnedObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                
                // ASSICURIAMOCI CHE I COLLIDER SIANO ATTIVI
                // Se i prefabs hanno i collider disattivati per non collidere negli NPC,
                // dobbiamo riattivarli non appena appaiono sul tavolo.
                Collider[] colliders = spawnedObj.GetComponentsInChildren<Collider>(true);
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }

                // Assicuriamoci che anche la fisica sia attiva se necessario
                Rigidbody rb = spawnedObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                Debug.Log($"[WasteObjectSpawner] Spawn riuscito e Collider ATTIVATI: {prefab.name}");
            }
        }

        public GameObject GetRandomPrefabForType(WasteType type)
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
                WasteType.Card => cardPrefabs,
                WasteType.Baby => babyPrefabs,
                WasteType.Box => boxPrefabs,
                _ => null
            };

            if (targetList == null || targetList.Count == 0)
                return null;

            int randomIndex = Random.Range(0, targetList.Count);
            return targetList[randomIndex];
        }
    }
}
