using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AvenueXR.Core
{
    /// <summary>
    /// Script da mettere sulla mano dell'NPC. 
    /// Visualizza l'oggetto che l'NPC sta trasportando prima di consegnarlo sul tavolo.
    /// </summary>
    public class NPCHandBinder : MonoBehaviour
    {
        private GameObject _currentObject;

        /// <summary>
        /// Crea una copia visuale del prefab e la imparenta alla mano.
        /// </summary>
        public void BindPrefab(GameObject prefab)
        {
            Clear();
            if (prefab == null) return;

            _currentObject = Instantiate(prefab, transform);
            
            // Reset trasformazione per allinearlo alla mano
            _currentObject.transform.localPosition = Vector3.zero;
            _currentObject.transform.localRotation = Quaternion.identity;

            // Pulizia componenti: l'oggetto in mano all'NPC deve essere SOLO visuale.
            // Se ha collider o script di logica, l'NPC triggererà i cestini o la finestra camminando!
            
            // 1. Rimuoviamo tutti i collider
            Collider[] colliders = _currentObject.GetComponentsInChildren<Collider>();
            foreach (var c in colliders) Destroy(c);

            // 2. Rimuoviamo la logica di gioco (WasteItem)
            WasteItem logic = _currentObject.GetComponent<WasteItem>();
            if (logic != null) Destroy(logic);

            // 3. Disabilitiamo fisica e interazione
            Rigidbody rb = _currentObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            XRGrabInteractable grab = _currentObject.GetComponent<XRGrabInteractable>();
            if (grab != null) grab.enabled = false;
            
            // Se ci sono altri script (es. WasteItem), possiamo lasciarli o disabilitarli se danno fastidio
        }

        /// <summary>
        /// Rimuove l'oggetto dalla mano.
        /// </summary>
        public void Clear()
        {
            if (_currentObject != null)
            {
                Destroy(_currentObject);
                _currentObject = null;
            }
        }
    }
}
