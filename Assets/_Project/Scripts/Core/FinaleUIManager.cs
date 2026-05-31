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
        
        [Header("Settings")]
        [Tooltip("Delay prima di mostrare il finale (utile per attendere il fade-out del fader).")]
        public float appearanceDelay = 4.0f; // Aumentato per un fade più lungo e drammatico

        private void Awake()
        {
            // Assicuriamoci che il popup sia inizialmente chiuso
            if (finaleDialoguePopup != null)
            {
                finaleDialoguePopup.Close();
            }
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
            if (day == null || finaleDialoguePopup == null) return;

            Debug.Log($"[FinaleUIManager] Ricevuto finale: {day.endingTitle}. Preparazione visualizzazione...");
            StartCoroutine(ShowFinaleRoutine(day));
        }

        private IEnumerator ShowFinaleRoutine(DayData day)
        {
            // Attendiamo che il fader di fine giornata abbia completato la sua animazione
            yield return new WaitForSeconds(appearanceDelay);

            // FIX: Per il finale, forziamo il popup in Screen Space Overlay 
            // per assicurarci che sia sopra il fader nero (Overlay Canvas).
            Canvas canvas = finaleDialoguePopup.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
            }

            // Utilizziamo il sistema di dialogo esistente:
            // - endingTitle viene usato come nome dello speaker (in alto)
            // - endingDescription viene usato come corpo del testo (con typewriter effect)
            finaleDialoguePopup.ShowDialogue(day.endingDescription, day.endingTitle);
            
            Debug.Log("[FinaleUIManager] Finale visualizzato tramite DialogueBox (Forced Overlay).");
        }
    }
}
