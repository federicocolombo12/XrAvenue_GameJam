using UnityEngine;

namespace AvenueXR.Core
{
    [CreateAssetMenu(fileName = "NewDay", menuName = "AvenueXR/Day Data")]
    public class DayData : ScriptableObject
    {
        [Header("Info Giorno")]
        public string dayLabel;
        
        [Header("Sistema Dialoghi (Esterno)")]
        [Tooltip("Trascina qui l'asset dei dialoghi creato dall'altro sistema")]
        public ScriptableObject dialogueData;

        [Header("Configurazione Rifiuti")]
        public int normalWasteCount = 5;
        public bool hasMoralObject = false;
        public bool hasGoreObject = false;
        public bool hasBomb = false;

        [Header("Bivi Narrativi")]
        public DayData nextDayObedient;
        public DayData nextDayRebel;

        [Header("Feedback Visivo")]
        public float worldPollutionLevel; 
    }
}
