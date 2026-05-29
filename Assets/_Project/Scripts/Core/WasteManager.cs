using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteManager : MonoBehaviour
    {
        public DayDataEvent onDayStart;
        public WasteTypeEvent onWasteSorted;
        
        [Header("Debug")]
        public GameEvent completeDayEvent; 

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(OnDayStarted);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(OnDayStarted);
        }

        private void OnDayStarted(DayData day)
        {
            Debug.Log($"WasteManager: Generazione rifiuti per {day.dayLabel}");
        }

        public void SortWaste(int wasteTypeIndex)
        {
            WasteType type = (WasteType)wasteTypeIndex;
            if (onWasteSorted != null) onWasteSorted.Raise(type);
        }
    }
}
