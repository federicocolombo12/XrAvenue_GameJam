using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Simulatore di manovella meccanica reale. 
    /// Ideale per rotazioni continue (verricelli, meccanismi a manovella).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class XRPhysicalCrank : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        [Header("Meccanica Manovella")]
        [Tooltip("L'oggetto figlio che deve ruotare visivamente.")]
        public Transform visualTransform;
        [Tooltip("Asse locale di rotazione (solitamente Vector3.forward o Vector3.up).")]
        public Vector3 rotationAxis = Vector3.forward;
        [Tooltip("Moltiplicatore di forza. 1 = 1:1 con la mano.")]
        public float sensitivity = 1.0f;
        public bool invertRotation = false;

        [Header("Butter Output")]
        public FloatVariable totalRotationVariable;
        public FloatEvent onRotationStep;

        // --- Evento locale per il BinCrusher ---
        public event System.Action<float> OnRotationDelta;

        private float _accumulatedAngle = 0f;
        private float _audioStepCounter = 0f;
        private float _lastHandAngle;

        protected override void Awake()
        {
            base.Awake();
            
            // Setup Rigidbody meccanico
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Configurazione Grab per rotazione pura
            trackRotation = false;
            trackPosition = false;
            movementType = MovementType.Instantaneous;
            attachEaseInTime = 0;

            if (visualTransform == null) visualTransform = transform;
            
            _accumulatedAngle = 0f; 
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            // Memorizziamo l'angolo iniziale della mano rispetto al pivot
            _lastHandAngle = GetAngleFromHand(args.interactorObject.transform.position);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (isSelected && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                ApplyMechanicalRotation();
            }
        }

        private void ApplyMechanicalRotation()
        {
            if (interactorsSelecting.Count == 0) return;

            // 1. Troviamo l'angolo attuale della mano
            var interactor = interactorsSelecting[0];
            float currentHandAngle = GetAngleFromHand(interactor.transform.position);

            // 2. Calcoliamo lo spostamento angolare (usando DeltaAngle per gestire il wrap 360)
            float deltaAngle = Mathf.DeltaAngle(_lastHandAngle, currentHandAngle);
            _lastHandAngle = currentHandAngle;

            if (Mathf.Abs(deltaAngle) > 0.001f)
            {
                if (invertRotation) deltaAngle *= -1f;
                float adjustedDelta = deltaAngle * sensitivity;

                // 3. Sommiamo lo spostamento
                _accumulatedAngle += adjustedDelta;

                // 4. Applichiamo la rotazione visiva
                visualTransform.localRotation = Quaternion.AngleAxis(_accumulatedAngle, rotationAxis);

                // 5. Notifichiamo gli altri sistemi
                if (totalRotationVariable != null) totalRotationVariable.Value += adjustedDelta;
                OnRotationDelta?.Invoke(adjustedDelta);

                // 6. Feedback sonoro
                _audioStepCounter += Mathf.Abs(adjustedDelta);
                if (_audioStepCounter >= 15f)
                {
                    onRotationStep?.Raise(_audioStepCounter);
                    _audioStepCounter = 0f;
                }
            }
        }

        private float GetAngleFromHand(Vector3 handWorldPos)
        {
            // Centro e asse in coordinate world
            Vector3 worldPivot = transform.position;
            Vector3 worldAxis = transform.TransformDirection(rotationAxis);
            
            // Direzione dal pivot alla mano
            Vector3 dirToHand = (handWorldPos - worldPivot).normalized;
            
            // Proiettiamo sul piano di rotazione della manovella
            Vector3 projectedDir = Vector3.ProjectOnPlane(dirToHand, worldAxis).normalized;
            
            // Usiamo un riferimento "Up" world per calcolare l'angolo assoluto
            Vector3 referenceUp = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(referenceUp, worldAxis)) > 0.9f) referenceUp = Vector3.forward;
            
            return Vector3.SignedAngle(referenceUp, projectedDir, worldAxis);
        }
    }
}
