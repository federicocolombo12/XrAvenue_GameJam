using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Una manovella fisica che può essere ruotata con le mani in VR/MR.
    /// Invia il valore della rotazione tramite Butter per attivare meccanismi.
    /// </summary>
    public class XRPhysicalCrank : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
    {
        [Header("Crank Settings")]
        public Transform visualTransform; // L'oggetto che ruota visivamente
        public Vector3 rotationAxis = Vector3.forward;
        public float sensitivity = 1.0f;

        [Header("Butter Output")]
        public FloatVariable totalRotationVariable; // Accumula la rotazione totale (es. giri fatti)
        public FloatEvent onRotationStep; // Lancia un evento ogni X gradi (es. per suono "click")

        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor _grabbingInteractor;
        private Quaternion _initialLocalRotation;
        private Vector3 _previousHandDirection;
        private float _currentAngleAccumulator;

        protected override void Awake()
        {
            base.Awake();
            if (visualTransform == null) visualTransform = transform;
            _initialLocalRotation = visualTransform.localRotation;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            _grabbingInteractor = args.interactorObject;
            
            // Calcola la direzione iniziale della mano rispetto al centro della manovella
            _previousHandDirection = GetHandDirection();
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            _grabbingInteractor = null;
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
            Vector3 currentHandDirection = GetHandDirection();
            
            // Calcola l'angolo tra la vecchia direzione e la nuova
            float angleDelta = Vector3.SignedAngle(_previousHandDirection, currentHandDirection, rotationAxis);
            
            // Applica la rotazione visiva
            visualTransform.Rotate(rotationAxis, angleDelta * sensitivity, Space.Self);
            
            // Aggiorna la variabile di Butter
            if (totalRotationVariable != null)
            {
                totalRotationVariable.Value += angleDelta;
            }

            // Gestione "Click" sonoro ogni 30 gradi
            _currentAngleAccumulator += Mathf.Abs(angleDelta);
            if (_currentAngleAccumulator >= 30f)
            {
                onRotationStep?.Raise(_currentAngleAccumulator);
                _currentAngleAccumulator = 0f;
            }

            _previousHandDirection = currentHandDirection;
        }

        private Vector3 GetHandDirection()
        {
            if (_grabbingInteractor == null) return Vector3.up;

            // Vettore dalla manovella alla mano dell'interactor
            Vector3 direction = _grabbingInteractor.transform.position - transform.position;
            
            // Proietta il vettore sul piano di rotazione della manovella
            return Vector3.ProjectOnPlane(direction, transform.TransformDirection(rotationAxis)).normalized;
        }
    }
}
