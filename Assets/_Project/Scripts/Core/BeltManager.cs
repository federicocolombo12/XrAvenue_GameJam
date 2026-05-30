using UnityEngine;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce due rulli trasportatori che ruotano in direzioni opposte basandosi sulla rotazione della manovella.
    /// Uno ruota su X locale, l'altro su -X locale.
    /// </summary>
    public class BeltManager : MonoBehaviour
    {
        [Header("References")]
        public XRPhysicalCrank targetCrank;
        public Transform beltPositiveX;
        public Transform beltNegativeX;

        [Header("Settings")]
        [Tooltip("Asse locale di rotazione dei rulli (quello che ruota nell'inspector).")]
        public Vector3 rotationAxis = Vector3.right;
        
        [Tooltip("Moltiplicatore tra rotazione manovella e rotazione rulli.")]
        public float rotationMultiplier = 1.0f;

        private float _currentRotation = 0f;
        
        // Memorizziamo le rotazioni iniziali per non "resettare" il modello
        private Quaternion _initialRotationPos;
        private Quaternion _initialRotationNeg;

        private void Start()
        {
            if (beltPositiveX != null) _initialRotationPos = beltPositiveX.localRotation;
            if (beltNegativeX != null) _initialRotationNeg = beltNegativeX.localRotation;
        }

        private void OnEnable()
        {
            if (targetCrank != null)
            {
                targetCrank.OnRotationDelta += HandleRotationDelta;
            }
        }

        private void OnDisable()
        {
            if (targetCrank != null)
            {
                targetCrank.OnRotationDelta -= HandleRotationDelta;
            }
        }

        private void HandleRotationDelta(float delta)
        {
            float adjustedDelta = delta * rotationMultiplier;
            _currentRotation += adjustedDelta;

            // Applichiamo la rotazione cumulativa MANTENENDO la base iniziale
            if (beltPositiveX != null)
            {
                // Moltiplicazione tra Quaternioni: Applica la rotazione sull'asse locale relativo alla pos iniziale
                beltPositiveX.localRotation = _initialRotationPos * Quaternion.AngleAxis(_currentRotation, rotationAxis);
            }

            if (beltNegativeX != null)
            {
                beltNegativeX.localRotation = _initialRotationNeg * Quaternion.AngleAxis(-_currentRotation, rotationAxis);
            }
        }
    }
}
