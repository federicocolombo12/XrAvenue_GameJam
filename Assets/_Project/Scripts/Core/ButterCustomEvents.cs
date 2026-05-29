using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    [CreateAssetMenu(menuName = "AvenueXR/Events/DayData Event")]
    public class DayDataEvent : GameEvent<DayData> { }

    [CreateAssetMenu(menuName = "AvenueXR/Events/WasteType Event")]
    public class WasteTypeEvent : GameEvent<WasteType> { }
}
