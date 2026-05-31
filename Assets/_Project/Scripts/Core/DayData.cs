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

        [Header("Dialogo Finale (Opzionale)")]
        public DialogueData outroDialogue;

        [Header("Sequenza Consegne")]
        public List<WasteDeliveryStep> deliveries = new List<WasteDeliveryStep>();

        [Header("Bivi Narrativi")]
        public DayData nextDayObedient;
        public DayData nextDayRebel;
        [Tooltip("Punti ribellione necessari alla fine del giorno per attivare il ramo Rebel.")]
        public int rebellionThreshold = 1;

        [Header("Feedback Visivo")]
        public float worldPollutionLevel;

        [Header("Audio (Opzionale)")]
        public AudioClip dayAmbient;
        public AudioClip dayMusic;
        public AudioClip bossVoiceFallback;
        public AudioClip npcVoiceFallback;

        [Header("Logica Finali (Opzionale)")]
        public bool isFinale = false;
        public string endingTitle;
        [TextArea(3, 10)]
        public string endingDescription;
        public AudioClip endingSoundClip;
    }
}
