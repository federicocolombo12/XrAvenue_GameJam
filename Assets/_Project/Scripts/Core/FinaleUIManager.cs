using UnityEngine;
using Dev.Nicklaj.Butter;
using System.Collections;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce la UI dei finali riutilizzando il componente DialogueBox (WorldDialoguePopup)
    /// per mantenere la coerenza visiva dell'applicazione.
    /// </summary>
    public class FinaleUIManager : MonoBehaviour
    {
        [Header("Butter Events")]
        public DayDataEvent onFinaleReached;

        [Header("UI References")]
        [Tooltip("Il componente DialogueBox (WorldDialoguePopup) da usare per mostrare il finale.")]
        public WorldDialoguePopup finaleDialoguePopup;
        public CityPollutionManager cityPollutionManager;
        
        [Header("Settings")]
        [Tooltip("Delay prima di mostrare il finale (utile per attendere il fade-out del fader).")]
        public float appearanceDelay = 2.0f; // Ridotto da 4.0 per maggiore reattività

        private void Awake()
        {
            // Assicuriamoci che il popup sia inizialmente chiuso
            if (finaleDialoguePopup != null)
            {
                finaleDialoguePopup.Close();
            }

            if (cityPollutionManager == null)
                cityPollutionManager = FindFirstObjectByType<CityPollutionManager>();
        }

        private void OnEnable()
        {
            if (onFinaleReached != null)
                onFinaleReached.RegisterListener(HandleFinaleReached);
        }

        private void OnDisable()
        {
            if (onFinaleReached != null)
                onFinaleReached.DeregisterListener(HandleFinaleReached);
        }

        private void HandleFinaleReached(DayData day)
        {
            if (day == null) return;
            
            if (finaleDialoguePopup == null)
            {
                Debug.LogError("[FinaleUIManager] ERRORE: finaleDialoguePopup non è assegnato! Impossibile mostrare il finale.");
                return;
            }

            Debug.Log($"<color=gold>[FinaleUIManager] Ricevuto finale: {day.endingTitle}. Avvio routine tra {appearanceDelay} secondi...</color>");
            StartCoroutine(ShowFinaleRoutine(day));
        }

        private IEnumerator ShowFinaleRoutine(DayData day)
        {
            // Sincronizziamo l'inquinamento subito
            if (cityPollutionManager != null)
            {
                cityPollutionManager.UpdateCityVisuals(day.endingPollutionLevel);
            }

            // Attendiamo il delay impostato
            yield return new WaitForSeconds(appearanceDelay);

            if (finaleDialoguePopup == null) yield break;

            // Assicuriamoci che l'oggetto sia attivo (il setup del canvas è manuale ora)
            finaleDialoguePopup.gameObject.SetActive(true);

            // Utilizziamo il sistema di dialogo esistente:
            finaleDialoguePopup.ShowDialogue(day.endingDescription, day.endingTitle);
            
            Debug.Log($"[FinaleUIManager] Finale '{day.endingTitle}' attivato. Setup visivo gestito manualmente.");
        }
    }
}
