using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteBin : MonoBehaviour
    {
        [Header("Settings")]
        public WasteType acceptedType; // Quale tipo di rifiuto accetta questo cestino
        public bool isGenericBin = false; // Se vero, accetta tutto (utile per il tutorial)

        [Header("Butter Events")]
        public WasteTypeEvent onWasteSorted;
        public BoolEvent onMoralChoiceMade;

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
            Debug.Log($"Oggetto {item.type} inserito nel cestino {acceptedType}");

            // Notifica il sistema che un oggetto è stato processato (per far spawnare il prossimo)
            if (onWasteSorted != null) 
                onWasteSorted.Raise(item.type);

            // Gestione Morale: Se butti un oggetto speciale nel cestino, stai OBBEDENDO
            if (item.type != WasteType.Normal)
            {
                if (onMoralChoiceMade != null) 
                    onMoralChoiceMade.Raise(false); // False = Obbedienza
            }

            // Distruggi l'oggetto (o rimettilo nel pool)
            Destroy(item.gameObject);
            
            // Qui puoi aggiungere un feedback sonoro "Ding" o "Buildup"
        }
    }
}
