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

        private Vector3 _previousHandDirection;
        private float _currentAngleAccumulator;

        protected override void Awake()
        {
            base.Awake();
            if (visualTransform == null) visualTransform = transform;
            
            // Forziamo impostazioni per rotazione fisica
            movementType = MovementType.Instantaneous;
            attachEaseInTime = 0;
            retainTransformParent = true;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            _previousHandDirection = GetHandDirection(args.interactorObject);
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
            // Prendiamo il primo interactor che ci sta afferrando
            var interactor = interactorsSelecting[0];
            Vector3 currentHandDirection = GetHandDirection(interactor);
            
            // Calcola l'angolo relativo sul piano di rotazione
            float angleDelta = Vector3.SignedAngle(_previousHandDirection, currentHandDirection, transform.TransformDirection(rotationAxis));
            
            // Applica la rotazione visiva (in locale)
            visualTransform.Rotate(rotationAxis, angleDelta * sensitivity, Space.Self);
            
            // Aggiorna Butter
            if (totalRotationVariable != null)
            {
                totalRotationVariable.Value += angleDelta;
            }

            // Click sonoro
            _currentAngleAccumulator += Mathf.Abs(angleDelta);
            if (_currentAngleAccumulator >= 30f)
            {
                onRotationStep?.Raise(_currentAngleAccumulator);
                _currentAngleAccumulator = 0f;
            }

            _previousHandDirection = currentHandDirection;
        }

        private Vector3 GetHandDirection(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            // Vettore dalla manovella alla mano
            Vector3 direction = interactor.transform.position - transform.position;
            
            // Proietta sul piano definito dall'asse di rotazione
            Vector3 planeNormal = transform.TransformDirection(rotationAxis);
            return Vector3.ProjectOnPlane(direction, planeNormal).normalized;
        }
    }
}
