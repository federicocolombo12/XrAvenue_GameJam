using UnityEngine;

namespace AvenueXR.Core
{
    public class WasteObjectSpawner : MonoBehaviour
    {
        public Transform spawnPoint;
        public GameObject normalWastePrefab;
        public GameObject moralWastePrefab;
        public GameObject goreWastePrefab;
        public GameObject bombPrefab;

        public void Spawn(WasteType type)
        {
            GameObject prefab = type switch
            {
                WasteType.Normal => normalWastePrefab,
                WasteType.Moral => moralWastePrefab,
                WasteType.Gore => goreWastePrefab,
                WasteType.Bomb => bombPrefab,
                _ => null
            };

            if (prefab != null)
            {
                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
