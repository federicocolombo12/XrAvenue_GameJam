using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    [CreateAssetMenu(fileName = "NewDay", menuName = "AvenueXR/Day Data")]
    public class DayData : ScriptableObject
    {
        [Header("Info Giorno")]
        public string dayLabel;
        [TextArea] public string introBossMessage;

        [Header("Configurazione Rifiuti")]
        public int normalWasteCount = 5;
        public bool hasMoralObject = false;
        public bool hasGoreObject = false;
        public bool hasBomb = false;

        [Header("Bivi Narrativi")]
        public DayData nextDayObedient;
        public DayData nextDayRebel;

        [Header("Feedback Visivo")]
        public float worldPollutionLevel; // Da 0 a 1, per cambiare il look della finestra
    }
}
