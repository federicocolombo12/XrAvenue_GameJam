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

        private Vector3 _previousLocalHandDir;
        private float _accumulatedAngle = 0f;
        private float _audioStepCounter = 0f;

        protected override void Awake()
        {
            base.Awake();
            
            // Setup Rigidbody meccanico: non cade, non si sposta, non reagisce a colpi
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Configurazione Grab per rotazione pura
            trackRotation = false;
            trackPosition = false;
            movementType = MovementType.Instantaneous;
            attachEaseInTime = 0;

            if (visualTransform == null) visualTransform = transform;
            
            // Sincronizziamo l'accumulatore con la rotazione iniziale dell'oggetto
            // (Assumiamo che ruoti sull'asse impostato)
            _accumulatedAngle = 0f; 
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            // Al momento del grab, guardiamo dove si trova la mano rispetto al centro della manovella
            _previousLocalHandDir = GetLocalDirectionToHand(args.interactorObject.transform.position);
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

            // 1. Troviamo la posizione attuale della mano
            var interactor = interactorsSelecting[0];
            Vector3 currentLocalHandDir = GetLocalDirectionToHand(interactor.transform.position);

            // 2. Calcoliamo quanto la mano si è spostata lungo il cerchio dall'ultimo frame
            float deltaAngle = Vector3.SignedAngle(_previousLocalHandDir, currentLocalHandDir, rotationAxis);

            if (Mathf.Abs(deltaAngle) > 0.01f)
            {
                if (invertRotation) deltaAngle *= -1f;
                float adjustedDelta = deltaAngle * sensitivity;

                // 3. Sommiamo lo spostamento alla rotazione totale (permette infiniti giri)
                _accumulatedAngle += adjustedDelta;

                // 4. Applichiamo la rotazione visiva
                visualTransform.localRotation = Quaternion.AngleAxis(_accumulatedAngle, rotationAxis);

                // 5. Notifichiamo gli altri sistemi (Butter, Smaciullamento)
                if (totalRotationVariable != null) totalRotationVariable.Value += adjustedDelta;
                OnRotationDelta?.Invoke(adjustedDelta);

                // 6. Feedback sonoro ogni 15 gradi di rotazione effettiva
                _audioStepCounter += Mathf.Abs(adjustedDelta);
                if (_audioStepCounter >= 15f)
                {
                    onRotationStep?.Raise(_audioStepCounter);
                    _audioStepCounter = 0f;
                }

                // 7. Prepariamo il prossimo frame
                _previousLocalHandDir = currentLocalHandDir;
            }
        }

        private Vector3 GetLocalDirectionToHand(Vector3 handWorldPos)
        {
            // Portiamo la posizione della mano nello spazio locale del ROOT (che sta fermo)
            Vector3 localPos = transform.InverseTransformPoint(handWorldPos);
            // Proiettiamo sul piano di rotazione per ignorare se la mano tira in avanti/indietro
            Vector3 direction = Vector3.ProjectOnPlane(localPos, rotationAxis);
            return direction.normalized;
        }
    }
}
