using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Una manovella fisica che può essere ruotata con le mani in VR/MR.
    /// Eredita da XRGrabInteractable per supportare meglio il pinch delle mani e i controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class XRPhysicalCrank : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        [Header("Crank Settings")]
        public Transform visualTransform;
        public Vector3 rotationAxis = Vector3.forward; // Asse LOCALE di rotazione (es. Z per manopola frontale)
        public float sensitivity = 1.0f;
        public bool invertRotation = false; // Toggle per invertire il senso di rotazione

        [Header("Butter Output")]
        public FloatVariable totalRotationVariable;
        public FloatEvent onRotationStep;

        // --- Local Events for binding ---
        public event System.Action<float> OnRotationDelta;

        private Vector3 _initialLocalHandDir;
        private Quaternion _initialVisualRotation;
        private float _currentAngleAccumulator;
        private float _totalSessionRotation; // Rotazione accumulata in questa sessione di grab

        protected override void Awake()
        {
            base.Awake();
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            trackRotation = false;
            trackPosition = false;
            movementType = MovementType.Instantaneous;
            attachEaseInTime = 0;

            if (visualTransform == null) visualTransform = transform;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            // Memorizziamo lo stato iniziale al momento del grab
            _initialLocalHandDir = GetLocalDirectionToHand(args.interactorObject.transform.position);
            _initialVisualRotation = visualTransform.localRotation;
            _totalSessionRotation = 0f;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (isSelected && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                RotateCrank();
            }
        }

        private void RotateCrank()
        {
            if (interactorsSelecting.Count == 0) return;

            var interactor = interactorsSelecting[0];
            Vector3 currentLocalHandDir = GetLocalDirectionToHand(interactor.transform.position);
            
            // Calcola l'angolo tra la direzione iniziale e quella attuale
            float angleDiff = Vector3.SignedAngle(_initialLocalHandDir, currentLocalHandDir, rotationAxis);
            
            if (invertRotation) angleDiff *= -1f;

            // Applichiamo la rotazione "diretta" basata sull'offset dal grab iniziale
            float targetAngle = angleDiff * sensitivity;
            
            // Applichiamo la rotazione alla visual transform partendo dalla sua rotazione iniziale
            visualTransform.localRotation = _initialVisualRotation * Quaternion.AngleAxis(targetAngle, rotationAxis);

            // Per gli eventi di Butter e i calcoli di smaciullamento, usiamo il delta
            float delta = targetAngle - _totalSessionRotation;
            if (Mathf.Abs(delta) > 0.01f)
            {
                if (totalRotationVariable != null) totalRotationVariable.Value += delta;
                OnRotationDelta?.Invoke(delta);

                // Click sonoro ogni 20 gradi
                _currentAngleAccumulator += Mathf.Abs(delta);
                if (_currentAngleAccumulator >= 20f)
                {
                    onRotationStep?.Raise(_currentAngleAccumulator);
                    _currentAngleAccumulator = 0f;
                }
                
                _totalSessionRotation = targetAngle;
            }
        }

        private Vector3 GetLocalDirectionToHand(Vector3 handWorldPos)
        {
            // Trasforma la posizione della mano in spazio locale rispetto al ROOT della manovella
            Vector3 localHandPos = transform.InverseTransformPoint(handWorldPos);
            
            // Proietta sul piano di rotazione (definito dall'asse come normale)
            // Se rotationAxis è Vector3.forward, proietta sul piano XY locale
            Vector3 direction = Vector3.ProjectOnPlane(localHandPos, rotationAxis);
            
            return direction.normalized;
        }
    }
}
