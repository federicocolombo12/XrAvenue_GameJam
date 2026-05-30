using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Una manovella fisica che può essere ruotata con le mani in VR/MR.
    /// Eredita da XRGrabInteractable per supportare meglio il pinch delle mani e i controller.
    /// </summary>
    public class XRPhysicalCrank : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        [Header("Crank Settings")]
        public Transform visualTransform;
        public Vector3 rotationAxis = Vector3.forward;
        public float sensitivity = 1.0f;

        [Header("Butter Output")]
        public FloatVariable totalRotationVariable;
        public FloatEvent onRotationStep;

        // --- Local Events for binding ---
        public event System.Action<float> OnRotationDelta;

        private Vector3 _previousHandDirection;
        private float _currentAngleAccumulator;
        private float _accumulatedVisualAngle;

        protected override void Awake()
        {
            base.Awake();
            
            // DISABILITIAMO il tracking automatico di Unity
            // Vogliamo solo sapere DOVE è la mano, non che Unity sposti l'oggetto
            trackRotation = false;
            trackPosition = false;
            
            if (visualTransform == null)
            {
                Debug.LogWarning($"[XRPhysicalCrank] VisualTransform non assegnata su {gameObject.name}. La rotazione potrebbe essere instabile.");
                visualTransform = transform;
            }
            
            movementType = MovementType.Instantaneous;
            attachEaseInTime = 0;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            // Inizializziamo la direzione in spazio LOCALE per essere indipendenti dalla rotazione del mondo
            _previousHandDirection = GetLocalHandDirection(args.interactorObject);
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
            var interactor = interactorsSelecting[0];
            Vector3 currentLocalDirection = GetLocalHandDirection(interactor);
            
            // Calcola l'angolo tra le direzioni locali proiettate sul piano
            // Usiamo rotationAxis come normale del piano locale
            float angleDelta = Vector3.SignedAngle(_previousHandDirection, currentLocalDirection, rotationAxis);
            
            if (Mathf.Abs(angleDelta) > 0.001f)
            {
                _accumulatedVisualAngle += angleDelta * sensitivity;
                
                if (visualTransform != null)
                {
                    // Applichiamo la rotazione LOCALE solo all'asse desiderato
                    visualTransform.localRotation = Quaternion.AngleAxis(_accumulatedVisualAngle, rotationAxis);
                }
                
                if (totalRotationVariable != null)
                {
                    totalRotationVariable.Value += angleDelta;
                }

                OnRotationDelta?.Invoke(angleDelta);

                _currentAngleAccumulator += Mathf.Abs(angleDelta);
                if (_currentAngleAccumulator >= 30f)
                {
                    onRotationStep?.Raise(_currentAngleAccumulator);
                    _currentAngleAccumulator = 0f;
                }

                _previousHandDirection = currentLocalDirection;
            }
        }

        private Vector3 GetLocalHandDirection(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            // Trasformiamo la posizione della mano nello spazio locale della manovella (il ROOT, che non ruota)
            Vector3 handWorldPos = interactor.transform.position;
            Vector3 localHandPos = transform.InverseTransformPoint(handWorldPos);
            
            // Proietta la posizione locale sul piano definito dal rotationAxis locale
            // Se rotationAxis è (0,0,1), proietta sul piano XY
            return Vector3.ProjectOnPlane(localHandPos, rotationAxis).normalized;
        }
    }
}
