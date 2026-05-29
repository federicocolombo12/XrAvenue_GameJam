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
        public WasteTypeEvent onWasteSorted;
        public BoolEvent onMoralChoiceMade;
        public StringEvent onBossSpeech;

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
                    // Errore di smistamento (non necessariamente ribellione, forse solo incompetenza)
                    if (onBossSpeech != null) onBossSpeech.Raise("Errore di smistamento. La tua paga verrà decurtata.");
                }
            }

            // Notifica il sistema che un oggetto è stato processato per far avanzare lo spawner
            if (onWasteSorted != null) onWasteSorted.Raise(item.type);

            Destroy(item.gameObject);
        }
    }
}
