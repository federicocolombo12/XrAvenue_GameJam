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
        [Tooltip("Asse locale di rotazione dei rulli.")]
        public Vector3 rotationAxis = Vector3.right;
        
        [Tooltip("Moltiplicatore tra rotazione manovella e rotazione rulli.")]
        public float rotationMultiplier = 1.0f;

        private float _currentRotation = 0f;

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

            // Applichiamo le rotazioni opposte sull'asse scelto
            if (beltPositiveX != null)
            {
                beltPositiveX.localRotation = Quaternion.AngleAxis(_currentRotation, rotationAxis);
            }

            if (beltNegativeX != null)
            {
                beltNegativeX.localRotation = Quaternion.AngleAxis(-_currentRotation, rotationAxis);
            }
        }
    }
}
