using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Script speciale per oggetti che, se afferrati/raccolti, scatenano immediatamente la fine della giornata/gioco.
    /// Usato per il finale della pistola.
    /// </summary>
    public class FinaleGrabTrigger : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd; // Scatena il completamento del giorno

        private WasteItem _wasteItem;
        private bool _hasTriggered = false;

        private void Awake()
        {
            _wasteItem = GetComponent<WasteItem>();
        }

        private void Update()
        {
            // Se l'oggetto associato è stato raccolto dal giocatore PC
            if (!_hasTriggered && _wasteItem != null && _wasteItem.IsCarried)
            {
                TriggerFinale();
            }
        }

        public void TriggerFinale()
        {
            if (_hasTriggered) return;
            _hasTriggered = true;

            Debug.Log($"[FinaleGrabTrigger] Oggetto {gameObject.name} afferrato! Scateno fine gioco.");
            
            if (onDayEnd != null)
            {
                onDayEnd.Raise();
            }
            
            enabled = false;
        }
    }
}
