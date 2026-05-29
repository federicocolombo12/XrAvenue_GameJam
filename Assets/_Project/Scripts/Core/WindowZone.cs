using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WindowZone : MonoBehaviour
    {
        [Header("Butter Events")]
        public WasteTypeEvent onWasteSorted; // Usiamo questo per far avanzare lo spawner
        public BoolEvent onMoralChoiceMade;

        private void OnTriggerEnter(Collider other)
        {
            WasteItem item = other.GetComponentInParent<WasteItem>();
            
            if (item != null)
            {
                ProcessRebellion(item);
            }
        }

        private void ProcessRebellion(WasteItem item)
        {
            Debug.Log($"Oggetto {item.type} lanciato fuori dalla finestra! RIBELLIONE.");

            // Se lanci un oggetto speciale fuori, stai RIBELLANDOTI
            // Gli oggetti "Normali" (Paper, Plastic, etc.) non contano come ribellione se buttati fuori, 
            // ma vengono comunque processati per far avanzare lo spawner.
            bool isSpecialObject = item.type == WasteType.Moral || item.type == WasteType.Gore || item.type == WasteType.Bomb;

            if (isSpecialObject)
            {
                if (onMoralChoiceMade != null) 
                    onMoralChoiceMade.Raise(true); // True = Ribellione
            }

            // Conta comunque come oggetto processato per far andare avanti la giornata
            if (onWasteSorted != null) 
                onWasteSorted.Raise(item.type);

            // Distruggi l'oggetto
            Destroy(item.gameObject);
        }
    }
}
