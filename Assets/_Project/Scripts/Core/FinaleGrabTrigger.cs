using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Script speciale per oggetti che, se afferrati, scatenano immediatamente la fine della giornata/gioco.
    /// Usato per il finale della pistola.
    /// </summary>
    public class FinaleGrabTrigger : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd; // Scatena il completamento del giorno

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        private void OnEnable()
        {
            if (_grabInteractable != null)
                _grabInteractable.selectEntered.AddListener(HandleGrab);
        }

        private void OnDisable()
        {
            if (_grabInteractable != null)
                _grabInteractable.selectEntered.RemoveListener(HandleGrab);
        }

        private void HandleGrab(SelectEnterEventArgs args)
        {
            Debug.Log($"[FinaleGrabTrigger] Oggetto {gameObject.name} afferrato! Scateno fine gioco.");
            
            if (onDayEnd != null)
            {
                onDayEnd.Raise();
            }
            
            // Disabilitiamo per evitare doppie attivazioni
            enabled = false;
        }
    }
}
