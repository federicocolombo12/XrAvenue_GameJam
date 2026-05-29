using UnityEngine;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteDeliveryManager : MonoBehaviour
    {
        public DayDataEvent onDayStart;
        public WasteTypeEvent onWasteSorted;
        public GameEvent onDayEnd;

        [Header("Sub-Systems")]
        public NPCController npcController;
        public WasteObjectSpawner objectSpawner;

        private Queue<WasteType> _spawnQueue = new Queue<WasteType>();
        private int _totalObjectsThisDay;
        private int _objectsProcessed;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(HandleObjectProcessed);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.DeregisterListener(HandleObjectProcessed);
        }

        private void StartNewDay(DayData day)
        {
            _spawnQueue.Clear();
            _objectsProcessed = 0;

            for (int i = 0; i < day.normalWasteCount; i++) _spawnQueue.Enqueue(WasteType.Normal);
            if (day.hasMoralObject) _spawnQueue.Enqueue(WasteType.Moral);
            if (day.hasGoreObject) _spawnQueue.Enqueue(WasteType.Gore);
            if (day.hasBomb) _spawnQueue.Enqueue(WasteType.Bomb);

            _totalObjectsThisDay = _spawnQueue.Count;
            RequestNextDelivery();
        }

        private void RequestNextDelivery()
        {
            if (_spawnQueue.Count > 0)
            {
                WasteType nextType = _spawnQueue.Dequeue();
                // Chiediamo all'NPC di portarci l'oggetto
                npcController.DeliverObject(nextType, () => {
                    // Quando l'NPC arriva, lo spawner crea l'oggetto fisico
                    objectSpawner.Spawn(nextType);
                });
            }
        }

        private void HandleObjectProcessed(WasteType type)
        {
            _objectsProcessed++;
            if (_objectsProcessed >= _totalObjectsThisDay)
            {
                if (onDayEnd != null) onDayEnd.Raise();
            }
            else
            {
                RequestNextDelivery();
            }
        }
    }
}
