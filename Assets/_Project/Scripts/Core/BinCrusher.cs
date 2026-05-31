using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce la logica di smaciullamento locale per un singolo cestino.
    /// Collega un WasteBin a una specifica XRPhysicalCrank.
    /// </summary>
    public class BinCrusher : MonoBehaviour
    {
        [Header("References")]
        public WasteBin targetBin;
        public XRPhysicalCrank targetCrank;
        public AudioSource audioSource;
        public WasteAudioData audioData;

        [Header("Butter Output")]
        public WasteTypeEvent onWasteSorted; // Per far avanzare il gioco (Manager)
        public BoolEvent onSortingResult;    // true se corretto, false se sbagliato
        public GameEvent onCrushStart;       // Feedback visivo/sonoro
        public GameEvent onCrushTick;

        [Header("Settings")]
        public float rotationNeeded = 360f; // 1 giro completo per smaciullare

        private bool _isPending = false;
        private float _accumulatedRotation = 0f;
        private WasteType _pendingType;
        private WasteItem _pendingItem;

        private void OnEnable()
        {
            if (targetBin != null)
                targetBin.OnItemReceived += HandleItemReceived;

            if (targetCrank != null)
                targetCrank.OnRotationDelta += HandleRotationDelta;
        }

        private void OnDisable()
        {
            if (targetBin != null)
                targetBin.OnItemReceived -= HandleItemReceived;

            if (targetCrank != null)
                targetCrank.OnRotationDelta -= HandleRotationDelta;
        }

        private void HandleItemReceived(WasteItem item)
        {
            // Se c'è già un oggetto in questo cestino, ignoriamo o accodiamo (qui ignoriamo per semplicità)
            if (_isPending) return;

            _isPending = true;
            _pendingType = item.type;
            _pendingItem = item;
            _accumulatedRotation = 0f;

            Debug.Log($"[BinCrusher] Cestino {targetBin.acceptedType}: Oggetto {_pendingType} pronto. Gira la manovella!");
            if (onCrushStart != null) onCrushStart.Raise();
        }

        private void HandleRotationDelta(float delta)
        {
            if (!_isPending) return;

            _accumulatedRotation += Mathf.Abs(delta);

            // Ogni 30 gradi circa possiamo lanciare un tick (feedback visivo del meccanismo)
            // if (_accumulatedRotation % 30 < Mathf.Abs(delta)) onCrushTick?.Raise();

            if (_accumulatedRotation >= rotationNeeded)
            {
                CompleteCrush();
            }
        }

        private void CompleteCrush()
        {
            Debug.Log($"[BinCrusher] Cestino {targetBin.acceptedType}: Smaciullamento completato per {_pendingType}!");
            
            // Riproduzione Audio Spazializzato basato sul tipo
            if (audioSource != null && audioData != null)
            {
                AudioClip clip = audioData.GetAudioForType(_pendingType);
                if (clip != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(clip);
                }
            }

            // Verifica correttezza
            bool isCorrect = (_pendingType == targetBin.acceptedType);
            if (onSortingResult != null) onSortingResult.Raise(isCorrect);

            // Distruzione effettiva dell'oggetto dopo lo smaciullamento
            if (_pendingItem != null)
            {
                Destroy(_pendingItem.gameObject);
            }

            _isPending = false;
            _pendingItem = null;
            _accumulatedRotation = 0f;

            if (onWasteSorted != null)
                onWasteSorted.Raise(_pendingType);
        }
    }
}
