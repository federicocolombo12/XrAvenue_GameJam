using UnityEngine;

namespace AvenueXR.Player
{
    public class HeadBobController : MonoBehaviour
    {
        [Header("Head Bob Settings")]
        [Tooltip("Intensità del rimbalzo della testa (regolabile via slider).")]
        [Range(0f, 0.2f)]
        public float bobAmount = 0.04f;

        [Tooltip("Frequenza del rimbalzo della testa (passi al secondo).")]
        [Range(1f, 25f)]
        public float bobFrequency = 10f;

        [Tooltip("Velocità con cui la telecamera torna al centro quando il giocatore si ferma.")]
        public float returnSpeed = 6f;

        [Header("References")]
        public PlayerController playerController;
        public Transform cameraHolder;

        private float _timer = 0f;
        private Vector3 _initialLocalPosition;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            if (cameraHolder == null)
            {
                cameraHolder = transform;
            }

            _initialLocalPosition = cameraHolder.localPosition;
        }

        private void Update()
        {
            if (playerController == null) return;

            // Se il bobAmount è 0 o disattivato, resetta dolcemente
            if (bobAmount <= 0.0001f || !playerController.IsMoving)
            {
                _timer = 0f;
                cameraHolder.localPosition = Vector3.Lerp(
                    cameraHolder.localPosition, 
                    _initialLocalPosition, 
                    Time.deltaTime * returnSpeed
                );
                return;
            }

            // Calcolo oscillazione sinusoidale basata sul movimento
            _timer += Time.deltaTime * bobFrequency;

            float bobOffsetY = Mathf.Sin(_timer) * bobAmount;
            float bobOffsetX = Mathf.Cos(_timer * 0.5f) * (bobAmount * 0.5f);

            Vector3 targetPosition = _initialLocalPosition + new Vector3(bobOffsetX, bobOffsetY, 0f);
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPosition, Time.deltaTime * 15f);
        }
    }
}
