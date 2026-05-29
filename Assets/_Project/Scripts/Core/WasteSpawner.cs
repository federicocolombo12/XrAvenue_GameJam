using UnityEngine;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteSpawner : MonoBehaviour
    {
        [Header("References Butter")]
        public DayDataEvent onDayStart;
        public WasteTypeEvent onWasteSorted; // Ascolta quando finisci un oggetto
        public GameEvent onDayEnd; // Lancia questo quando la scrivania è vuota

        [Header("Spawn Settings")]
        public Transform spawnPoint;
        public float delayBetweenSpawns = 1.0f;

        [Header("Prefabs")]
        public GameObject normalWastePrefab;
        public GameObject moralWastePrefab;
        public GameObject goreWastePrefab;
        public GameObject bombPrefab;

        private Queue<WasteType> _spawnQueue = new Queue<WasteType>();
        private int _totalObjectsThisDay;
        private int _objectsProcessed;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(PrepareDay);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(HandleObjectProcessed);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(PrepareDay);
            if (onWasteSorted != null) onWasteSorted.DeregisterListener(HandleObjectProcessed);
        }

        private void PrepareDay(DayData day)
        {
            _spawnQueue.Clear();
            _objectsProcessed = 0;

            // 1. Aggiungi i rifiuti normali
            for (int i = 0; i < day.normalWasteCount; i++)
                _spawnQueue.Enqueue(WasteType.Normal);

            // 2. Aggiungi oggetti speciali (puoi randomizzare l'ordine se vuoi)
            if (day.hasMoralObject) _spawnQueue.Enqueue(WasteType.Moral);
            if (day.hasGoreObject) _spawnQueue.Enqueue(WasteType.Gore);
            if (day.hasBomb) _spawnQueue.Enqueue(WasteType.Bomb);

            _totalObjectsThisDay = _spawnQueue.Count;
            
            // Inizia lo spawn del primo oggetto
            SpawnNext();
        }

        private void SpawnNext()
        {
            if (_spawnQueue.Count == 0) return;

            WasteType nextType = _spawnQueue.Dequeue();
            GameObject prefabToSpawn = GetPrefabForType(nextType);

            if (prefabToSpawn != null && spawnPoint != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            }
        }

        private void HandleObjectProcessed(WasteType type)
        {
            _objectsProcessed++;

            if (_objectsProcessed >= _totalObjectsThisDay)
            {
                Debug.Log("Giornata finita, scrivania pulita.");
                if (onDayEnd != null) onDayEnd.Raise();
            }
            else
            {
                Invoke(nameof(SpawnNext), delayBetweenSpawns);
            }
        }

        private GameObject GetPrefabForType(WasteType type)
        {
            return type switch
            {
                WasteType.Normal => normalWastePrefab,
                WasteType.Moral => moralWastePrefab,
                WasteType.Gore => goreWastePrefab,
                WasteType.Bomb => bombPrefab,
                _ => null
            };
        }
    }
}
