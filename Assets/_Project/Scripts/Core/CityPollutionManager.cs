using UnityEngine;
using Dev.Nicklaj.Butter;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce la visibilità degli oggetti della città in base al livello di inquinamento definito nel DayData.
    /// Ascolta l'evento Butter 'onDayStart' per sincronizzarsi automaticamente.
    /// </summary>
    public class CityPollutionManager : MonoBehaviour
    {
        [System.Serializable]
        public class PollutionGameObject
        {
            public string description; // Solo per organizzazione nell'inspector
            public GameObject target;
            
            [Tooltip("L'oggetto sarà attivo se il pollution level è >= a questo valore")]
            public float minThreshold = 0;
            
            [Tooltip("L'oggetto sarà attivo se il pollution level è <= a questo valore")]
            public float maxThreshold = 10;
        }

        [Header("Butter Events")]
        public DayDataEvent onDayStart;
        public DayDataEvent onFinaleReached; // Aggiunto per i finali

        [Header("City Objects Configuration")]
        [Tooltip("Lista di oggetti da attivare/disattivare in base al range di inquinamento")]
        public List<PollutionGameObject> cityObjects = new List<PollutionGameObject>();

        private void OnEnable()
        {
            if (onDayStart != null)
                onDayStart.RegisterListener(HandleDayStart);

            if (onFinaleReached != null)
                onFinaleReached.RegisterListener(HandleFinaleReached);
        }

        private void OnDisable()
        {
            if (onDayStart != null)
                onDayStart.DeregisterListener(HandleDayStart);

            if (onFinaleReached != null)
                onFinaleReached.DeregisterListener(HandleFinaleReached);
        }

        private void HandleDayStart(DayData day)
        {
            if (day == null) return;
            UpdateCityVisuals(day.worldPollutionLevel);
        }

        private void HandleFinaleReached(DayData day)
        {
            if (day == null) return;
            // Usiamo il livello specifico del finale per mostrare le conseguenze ultime
            UpdateCityVisuals(day.endingPollutionLevel);
        }

        /// <summary>
        /// Cicla tutti gli oggetti configurati e li attiva/disattiva in base al livello attuale.
        /// </summary>
        public void UpdateCityVisuals(float currentPollution)
        {
            Debug.Log($"[CityPollutionManager] Sincronizzazione visual città per livello inquinamento: {currentPollution}");

            foreach (var item in cityObjects)
            {
                if (item.target == null) continue;

                // Controlla se il livello attuale rientra nel range definito per questo oggetto
                bool shouldBeActive = currentPollution >= item.minThreshold && currentPollution <= item.maxThreshold;
                
                if (item.target.activeSelf != shouldBeActive)
                {
                    item.target.SetActive(shouldBeActive);
                }
            }
        }
    }
}
