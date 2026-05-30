using UnityEngine;
using Dev.Nicklaj.Butter;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    /// <summary>
    /// Versione con Debug avanzato per diagnosticare problemi di collisione.
    /// </summary>
    public class WasteReturnZone : MonoBehaviour
    {
        [Header("Butter Events")]
        public WasteTypeEvent onWasteReturned;

        private static HashSet<int> _globallyHandledItems = new HashSet<int>();

        private void OnTriggerEnter(Collider other)
        {
            // QUESTO LOG DEVE APPARIRE PER FORZA SE C'È CONTATTO FISICO
            Debug.Log($"[ReturnZone DEBUG] QUALCOSA è entrato nel trigger: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            
            WasteItem item = other.GetComponentInParent<WasteItem>();
            if (item != null)
                Debug.Log($"[ReturnZone DEBUG] Rilevato componente WasteItem: {item.type} su {item.gameObject.name}");
            else
                Debug.Log($"[ReturnZone DEBUG] L'oggetto {other.name} (o i suoi genitori) NON ha un componente WasteItem.");
        }

        private void OnTriggerStay(Collider other)
        {
            WasteItem item = other.GetComponentInParent<WasteItem>();
            if (item == null) return;

            XRGrabInteractable interactable = item.GetComponent<XRGrabInteractable>();
            if (interactable == null) interactable = item.GetComponentInChildren<XRGrabInteractable>();
            
            if (interactable == null) return;

            // Se il giocatore lo sta tenendo
            if (interactable.isSelected)
            {
                if (!_globallyHandledItems.Contains(item.GetInstanceID()))
                {
                    Debug.Log($"[ReturnZone] Oggetto {item.name} maneggiato correttamente. Pronto per il reso.");
                    _globallyHandledItems.Add(item.GetInstanceID());
                }
            }
            // Se lo rilascia nella zona ed è stato marcato
            else if (_globallyHandledItems.Contains(item.GetInstanceID()))
            {
                Debug.Log($"[ReturnZone] CONDIZIONI SODDISFATTE. Restituisco l'oggetto {item.type} all'NPC.");
                
                if (onWasteReturned != null)
                    onWasteReturned.Raise(item.type);

                _globallyHandledItems.Remove(item.GetInstanceID());
                Destroy(item.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[ReturnZone DEBUG] Oggetto uscito: {other.name}");
        }
    }
}
