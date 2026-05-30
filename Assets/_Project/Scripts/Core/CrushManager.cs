using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class CrushManager : MonoBehaviour
    {
        [Header("Butter Variables")]
        public FloatVariable totalRotationVariable;
        
        [Header("Butter Events")]
        public WasteTypeEvent onItemPendingCrush; // Da WasteBin
        public WasteTypeEvent onWasteSorted;      // Verso WasteDeliveryManager (Segnala completamento)
        public GameEvent onCrushStart;           // Feedback visivo/sonoro inizio schiacciamento
        public GameEvent onCrushTick;            // Feedback per ogni passo di schiacciamento

        [Header("Settings")]
        public float rotationNeeded = 360f; // 1 giro completo per smaciullare
        
        private bool _isPending = false;
        private float _startRotation;
        private WasteType _pendingType;

        private void OnEnable()
        {
            if (onItemPendingCrush != null) onItemPendingCrush.RegisterListener(HandleNewItem);
        }

        private void OnDisable()
        {
            if (onItemPendingCrush != null) onItemPendingCrush.DeregisterListener(HandleNewItem);
        }

        private void HandleNewItem(WasteType type)
        {
            if (_isPending) return; // Già uno in coda (non dovrebbe succedere con 1 NPC alla volta)

            _isPending = true;
            _pendingType = type;
            _startRotation = totalRotationVariable != null ? totalRotationVariable.Value : 0;
            
            Debug.Log($"[CrushManager] Oggetto {_pendingType} in attesa di smaciullamento. Serve rotazione: {rotationNeeded}");
            if (onCrushStart != null) onCrushStart.Raise();
        }

        private void Update()
        {
            if (!_isPending) return;

            float currentRot = totalRotationVariable != null ? totalRotationVariable.Value : 0;
            float delta = Mathf.Abs(currentRot - _startRotation);

            if (delta >= rotationNeeded)
            {
                CompleteCrush();
            }
        }

        private void CompleteCrush()
        {
            Debug.Log($"[CrushManager] Smaciullamento completato per {_pendingType}!");
            _isPending = false;
            
            if (onWasteSorted != null) onWasteSorted.Raise(_pendingType);
        }
    }
}
