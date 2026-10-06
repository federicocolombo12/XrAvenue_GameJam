using System.Collections;
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
        [Tooltip("Velocità con cui la manovella compie lo sweep graduale ad ogni colpo (gradi/sec).")]
        public float smoothTurnSpeed = 750f;

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

        private float _targetAngle = 0f;
        private float _currentAngle = 0f;
        private float _audioStepCounter = 0f;
        private bool _isRotatingThisFrame = false;
        private float _currentRotationSpeed = 0f;
        private float _lastImpulseTime = -10f;
        private Vector3 _baseScale = Vector3.one;
        private Coroutine _punchScaleCoroutine;

        public float CurrentAngle => _currentAngle;

        private void Awake()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (visualTransform == null) visualTransform = transform;
            _baseScale = visualTransform.localScale;
            
            if (localAudioSource != null && tickSound != null)
            {
                localAudioSource.clip = tickSound;
                localAudioSource.loop = true;
                localAudioSource.playOnAwake = false;
                localAudioSource.volume = tickVolume;
            }

            _targetAngle = 0f;
            _currentAngle = 0f;
        }

        private void Update()
        {
            HandleSpringBack();
            UpdateSmoothRotation();
            UpdateAudioResponsiveness();
        }

        private void HandleSpringBack()
        {
            if (!enableSpringBack || _targetAngle <= 0.001f) return;

            // Se è trascorso il tempo di delay dall'ultimo smash, riavvolgiamo l'angolo target verso 0
            if (Time.time - _lastImpulseTime > springBackDelay)
            {
                _targetAngle = Mathf.MoveTowards(_targetAngle, 0f, springBackSpeed * Time.deltaTime);
            }
        }

        private void UpdateSmoothRotation()
        {
            float prevAngle = _currentAngle;

            if (Mathf.Abs(_targetAngle - _currentAngle) > 0.001f)
            {
                // Sweep rapido e progressivo verso _targetAngle (non scatta istantaneamente)
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, smoothTurnSpeed * Time.deltaTime);
                float deltaThisFrame = _currentAngle - prevAngle;

                _isRotatingThisFrame = true;
                _currentRotationSpeed = Mathf.Abs(deltaThisFrame) / Mathf.Max(Time.deltaTime, 0.001f);

                if (visualTransform != null)
                {
                    visualTransform.localRotation = Quaternion.AngleAxis(_currentAngle, rotationAxis);
                }

                if (totalRotationVariable != null) totalRotationVariable.Value += deltaThisFrame;
                OnRotationDelta?.Invoke(deltaThisFrame);

                _audioStepCounter += Mathf.Abs(deltaThisFrame);
                if (_audioStepCounter >= 15f)
                {
                    onRotationStep?.Raise(_audioStepCounter);
                    _audioStepCounter = 0f;
                }
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
        /// Applica un impulso discreto alla manovella (sweep progressivo con punch scale e audio in crescendo).
        /// </summary>
        public void ApplyImpulse(float impulseDegrees)
        {
            _lastImpulseTime = Time.time;
            
            float adjustedImpulse = impulseDegrees * sensitivity;
            if (invertRotation) adjustedImpulse *= -1f;

            _targetAngle = Mathf.Max(0f, _targetAngle + adjustedImpulse);

            // Punch scale d'impatto visivo
            TriggerPunchScale();

            // Riproduzione Audio con pitch crescendo verso il 100%
            if (localAudioSource != null && tickSound != null)
            {
                float progress = Mathf.Clamp01(_targetAngle / 360f);
                float pitch = Random.Range(0.95f, 1.05f) + (progress * 0.35f);
                localAudioSource.pitch = pitch;
                localAudioSource.PlayOneShot(tickSound, tickVolume);
            }
        }

        private void TriggerPunchScale()
        {
            if (visualTransform == null) return;

            if (_punchScaleCoroutine != null)
            {
                StopCoroutine(_punchScaleCoroutine);
            }
            _punchScaleCoroutine = StartCoroutine(PunchScaleRoutine());
        }

        private IEnumerator PunchScaleRoutine()
        {
            Vector3 squashedScale = Vector3.Scale(_baseScale, new Vector3(1.12f, 0.88f, 1.12f));
            float duration = 0.09f;
            float elapsed = 0f;

            visualTransform.localScale = squashedScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                visualTransform.localScale = Vector3.Lerp(squashedScale, _baseScale, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            visualTransform.localScale = _baseScale;
            _punchScaleCoroutine = null;
        }

        /// <summary>
        /// Ruota la manovella di un delta angolare (es. da input continuo).
        /// </summary>
        public void RotateManual(float deltaAngle)
        {
            _lastImpulseTime = Time.time;
            float adjustedDelta = deltaAngle * sensitivity;
            if (invertRotation) adjustedDelta *= -1f;

            _targetAngle = Mathf.Max(0f, _targetAngle + adjustedDelta);
        }

        /// <summary>
        /// Resetta l'angolo accumulato al completamento del ciclo di smaciullamento.
        /// </summary>
        public void ResetAccumulatedAngle()
        {
            /*
            _targetAngle = 0f;
            _currentAngle = 0f;
            _lastImpulseTime = -10f;
            if (visualTransform != null)
            {
                visualTransform.localRotation = Quaternion.identity;
                visualTransform.localScale = _baseScale;
            }
            */
        }
    }
}
