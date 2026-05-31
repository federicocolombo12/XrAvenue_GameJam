using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteBin : MonoBehaviour
    {
        [Header("Settings")]
        public WasteType acceptedType; 
        public bool isRejectBin = false; // Se vero, questo cestino è per il rifiuto del compito

        [Header("Butter Events")]
        public WasteTypeEvent onItemPendingCrush; // Nuovo evento per lo smaciullamento manuale
        public BoolEvent onMoralChoiceMade;
        public StringEvent onBossSpeech;

        // --- Local Events ---
        public event System.Action<WasteItem> OnItemReceived;

        private void OnTriggerEnter(Collider other)
        {
            WasteItem item = other.GetComponentInParent<WasteItem>();
            
            if (item != null)
            {
                ProcessWaste(item);
            }
        }

        private void ProcessWaste(WasteItem item)
        {
            Debug.Log($"Oggetto {item.type} inserito nel cestino {acceptedType} (Reject: {isRejectBin})");

            if (isRejectBin)
            {
                // REJECT BIN: Il giocatore si rifiuta di smistare correttamente l'oggetto
                if (onMoralChoiceMade != null) onMoralChoiceMade.Raise(true); // True = Ribellione
                if (onBossSpeech != null) onBossSpeech.Raise("COSA STAI FACENDO? Quel pezzo non va lì!");
            }
            else
            {
                // BIN NORMALE: Controlla se il tipo è corretto
                if (item.type == acceptedType)
                {
                    // Smistamento corretto
                    if (item.type != WasteType.Moral && item.type != WasteType.Gore && item.type != WasteType.Bomb)
                    {
                        // Feedback positivo opzionale per oggetti normali
                    }
                    else
                    {
                        // Hai smistato correttamente un oggetto "orribile" -> OBBEDIENZA
                        if (onMoralChoiceMade != null) onMoralChoiceMade.Raise(false); 
                    }
                }
                else
                {
                    // Errore di smistamento
                    if (onBossSpeech != null) onBossSpeech.Raise("Errore di smistamento. La tua paga verrà decurtata.");
                }
            }

            // Invece di far avanzare subito il gioco, mandiamo l'evento al CrushManager
            if (onItemPendingCrush != null) onItemPendingCrush.Raise(item.type);
            
            // "Catturiamo" l'oggetto per lo smistamento
            PrepareItemForCrushing(item);

            OnItemReceived?.Invoke(item);
        }

        private void PrepareItemForCrushing(WasteItem item)
        {
            // Disabilitiamo il collider e rendiamo kinematico per evitare che cada o si muova
            // mentre aspettiamo la manovella
            Collider col = item.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
