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

        [Header("Spring-Back (Ritorno all'indietro)")]
        [Tooltip("Se attivo, la manovella ruota all'indietro se non viene premuto il tasto a sufficienza.")]
        public bool enableSpringBack = true;
        [Tooltip("Velocità con cui la manovella torna indietro (gradi/sec).")]
        public float springBackSpeed = 110f;
        [Tooltip("Secondi di inattività prima che inizi il riavvolgimento.")]
        public float springBackDelay = 0.35f;

        // --- Evento locale per il BinCrusher e BeltManager ---
        public event System.Action<float> OnRotationDelta;

        private float _accumulatedAngle = 0f;
        private float _audioStepCounter = 0f;
        private bool _isRotatingThisFrame = false;
        private float _currentRotationSpeed = 0f;
        private float _lastImpulseTime = -10f;

        public float CurrentAngle => _accumulatedAngle;

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
            HandleSpringBack();
            UpdateAudioResponsiveness();
        }

        private void HandleSpringBack()
        {
            if (!enableSpringBack || _accumulatedAngle <= 0.001f) return;

            // Se è trascorso il tempo di delay dall'ultimo smash, riavvolgiamo
            if (Time.time - _lastImpulseTime > springBackDelay)
            {
                float unwindDelta = -springBackSpeed * Time.deltaTime;
                if (_accumulatedAngle + unwindDelta < 0f)
                {
                    unwindDelta = -_accumulatedAngle;
                }

                RotateInternal(unwindDelta);
            }
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
        /// Applica un impulso discreto alla manovella (es. button mashing da tasto Spazio o click).
        /// </summary>
        public void ApplyImpulse(float impulseDegrees)
        {
            _lastImpulseTime = Time.time;
            RotateInternal(impulseDegrees);

            // Trigger one-shot audio tick immediato
            if (localAudioSource != null && tickSound != null && !localAudioSource.isPlaying)
            {
                localAudioSource.pitch = Random.Range(0.95f, 1.15f);
                localAudioSource.PlayOneShot(tickSound, tickVolume);
            }
        }

        /// <summary>
        /// Ruota la manovella di un delta angolare (es. da input continuo).
        /// </summary>
        public void RotateManual(float deltaAngle)
        {
            _lastImpulseTime = Time.time;
            RotateInternal(deltaAngle);
        }

        private void RotateInternal(float deltaAngle)
        {
            if (Mathf.Abs(deltaAngle) <= 0.0001f) return;

            if (invertRotation) deltaAngle *= -1f;
            float adjustedDelta = deltaAngle * sensitivity;

            _isRotatingThisFrame = true;
            _currentRotationSpeed = Mathf.Abs(adjustedDelta) / Mathf.Max(Time.deltaTime, 0.001f);

            _accumulatedAngle = Mathf.Max(0f, _accumulatedAngle + adjustedDelta);

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

        /// <summary>
        /// Resetta l'angolo accumulato al completamento del ciclo di smaciullamento.
        /// </summary>
        public void ResetAccumulatedAngle()
        {
            _accumulatedAngle = 0f;
            _lastImpulseTime = -10f;
            if (visualTransform != null)
            {
                visualTransform.localRotation = Quaternion.identity;
            }
        }
    }
}
