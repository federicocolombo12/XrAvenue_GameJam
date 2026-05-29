using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    [System.Serializable]
    public class WasteDeliveryStep
    {
        public string stepLabel = "Consegna";
        public WasteType type;
        
        [Header("Dialoghi Opzionali")]
        public DialogueData npcDialogue;  // Cosa dice l'NPC quando arriva
        public DialogueData bossDialogue; // Cosa dice il Boss quando l'NPC arriva
    }

    [CreateAssetMenu(fileName = "NewDay", menuName = "AvenueXR/Day Data")]
    public class DayData : ScriptableObject
    {
        [Header("Info Giorno")]
        public string dayLabel;
        
        [Header("Dialogo Iniziale (Opzionale)")]
        public DialogueData introDialogue;

        [Header("Sequenza Consegne")]
        public List<WasteDeliveryStep> deliveries = new List<WasteDeliveryStep>();

        [Header("Bivi Narrativi")]
        public DayData nextDayObedient;
        public DayData nextDayRebel;

        [Header("Feedback Visivo")]
        public float worldPollutionLevel; 
    }
}
