using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Simulatore di manovella meccanica per PC (interagibile tramite mouse/tasti).
    /// Mantiene la compatibilità con BinCrusher e BeltManager tramite OnRotationDelta.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class XRPhysicalCrank : MonoBehaviour
    {
        [Header("Meccanica Manovella")]
        [Tooltip("L'oggetto figlio che deve ruotare visivamente.")]
        public Transform visualTransform;
        [Tooltip("Asse locale di rotazione (solitamente Vector3.forward o Vector3.up).")]
        public Vector3 rotationAxis = Vector3.forward;
        [Tooltip("Moltiplicatore di velocità di rotazione.")]
        public float sensitivity = 1.0f;
        public bool invertRotation = false;

        [Header("Butter Output")]
        public FloatVariable totalRotationVariable;
        public FloatEvent onRotationStep;

        [Header("Audio Feedback")]
        public AudioSource localAudioSource;
        public AudioClip tickSound;
        [Range(0f, 1f)]
        public float tickVolume = 0.6f;

        // --- Evento locale per il BinCrusher e BeltManager ---
        public event System.Action<float> OnRotationDelta;

        private float _accumulatedAngle = 0f;
        private float _audioStepCounter = 0f;
        private bool _isRotatingThisFrame = false;
        private float _currentRotationSpeed = 0f;

        private void Awake()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (visualTransform == null) visualTransform = transform;
            
            if (localAudioSource != null && tickSound != null)
            {
                localAudioSource.clip = tickSound;
                localAudioSource.loop = true;
                localAudioSource.playOnAwake = false;
                localAudioSource.volume = tickVolume;
            }

            _accumulatedAngle = 0f;
        }

        private void Update()
        {
            UpdateAudioResponsiveness();
        }

        private void UpdateAudioResponsiveness()
        {
            if (localAudioSource == null || tickSound == null) return;

            if (_isRotatingThisFrame)
            {
                if (!localAudioSource.isPlaying)
                {
                    localAudioSource.Play();
                }

                float targetPitch = Mathf.Clamp(0.8f + (_currentRotationSpeed / 500f), 0.7f, 1.5f);
                localAudioSource.pitch = Mathf.Lerp(localAudioSource.pitch, targetPitch, Time.deltaTime * 10f);
            }
            else
            {
                if (localAudioSource.isPlaying)
                {
                    localAudioSource.Pause();
                }
            }

            _isRotatingThisFrame = false;
        }

        /// <summary>
        /// Ruota la manovella di un delta angolare (es. da input mouse o tasto del giocatore PC).
        /// </summary>
        public void RotateManual(float deltaAngle)
        {
            if (Mathf.Abs(deltaAngle) <= 0.0001f) return;

            if (invertRotation) deltaAngle *= -1f;
            float adjustedDelta = deltaAngle * sensitivity;

            _isRotatingThisFrame = true;
            _currentRotationSpeed = Mathf.Abs(adjustedDelta) / Mathf.Max(Time.deltaTime, 0.001f);

            _accumulatedAngle += adjustedDelta;

            if (visualTransform != null)
            {
                visualTransform.localRotation = Quaternion.AngleAxis(_accumulatedAngle, rotationAxis);
            }

            if (totalRotationVariable != null) totalRotationVariable.Value += adjustedDelta;
            OnRotationDelta?.Invoke(adjustedDelta);

            _audioStepCounter += Mathf.Abs(adjustedDelta);
            if (_audioStepCounter >= 15f)
            {
                onRotationStep?.Raise(_audioStepCounter);
                _audioStepCounter = 0f;
            }
        }
    }
}
