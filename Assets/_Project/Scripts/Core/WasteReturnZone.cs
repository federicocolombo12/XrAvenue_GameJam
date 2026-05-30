using UnityEngine;
using Dev.Nicklaj.Butter;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    /// <summary>
    /// Zona di trigger che rileva quando un oggetto viene restituito all'NPC.
    /// Ora richiede che l'oggetto sia stato afferrato dal giocatore almeno una volta per evitare attivazioni accidentali allo spawn.
    /// </summary>
    public class WasteReturnZone : MonoBehaviour
    {
        [Header("Butter Events")]
        public WasteTypeEvent onWasteReturned;

        // Teniamo traccia degli oggetti che il giocatore ha effettivamente toccato
        private HashSet<int> _handledItems = new HashSet<int>();

        private void OnTriggerStay(Collider other)
        {
            // Verifichiamo se è un oggetto di scarto
            WasteItem item = other.GetComponentInParent<WasteItem>();
            if (item == null) return;

            // Verifichiamo se l'oggetto ha un componente per l'interazione
            XRGrabInteractable interactable = item.GetComponent<XRGrabInteractable>();
            if (interactable == null) return;

            // Se il giocatore afferra l'oggetto, lo registriamo come "preso in carico"
            if (interactable.isSelected)
            {
                _handledItems.Add(item.GetInstanceID());
            }
            // Se l'oggetto NON è afferrato MA è stato impugnato in precedenza mentre era nel trigger...
            else if (_handledItems.Contains(item.GetInstanceID()))
            {
                // L'oggetto è stato rilasciato nella zona dopo essere stato toccato -> Restituito!
                Debug.Log($"[WasteReturnZone] Oggetto {item.type} restituito ufficialmente all'NPC.");
                
                if (onWasteReturned != null)
                {
                    onWasteReturned.Raise(item.type);
                }

                _handledItems.Remove(item.GetInstanceID());
                Destroy(item.gameObject);
            }
        }

        // Pulizia se l'oggetto viene rimosso dalla zona senza essere rilasciato (es. portato alla pressa)
        private void OnTriggerExit(Collider other)
        {
            WasteItem item = other.GetComponentInParent<WasteItem>();
            if (item != null)
            {
                _handledItems.Remove(item.GetInstanceID());
            }
        }
    }
}
