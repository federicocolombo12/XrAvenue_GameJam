using UnityEngine;
using Dev.Nicklaj.Butter;
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

        private static HashSet<WasteItem> _globallyHandledItems = new HashSet<WasteItem>();

        private void OnTriggerEnter(Collider other)
        {
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

            // Se il giocatore lo sta tenendo
            if (item.IsCarried)
            {
                if (!_globallyHandledItems.Contains(item))
                {
                    Debug.Log($"[ReturnZone] Oggetto {item.name} maneggiato correttamente. Pronto per il reso.");
                    _globallyHandledItems.Add(item);
                }
            }
            // Se lo rilascia nella zona ed è stato marcato
            else if (_globallyHandledItems.Contains(item))
            {
                Debug.Log($"[ReturnZone] CONDIZIONI SODDISFATTE. Restituisco l'oggetto {item.type} all'NPC.");
                
                if (onWasteReturned != null)
                    onWasteReturned.Raise(item.type);

                _globallyHandledItems.Remove(item);
                Destroy(item.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[ReturnZone DEBUG] Oggetto uscito: {other.name}");
        }
    }
}
