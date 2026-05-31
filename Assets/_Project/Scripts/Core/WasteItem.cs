using UnityEngine;

namespace AvenueXR.Core
{
    public class WasteItem : MonoBehaviour
    {
        public WasteType type;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip grabSound;
        public AudioClip dropSound;

        [Header("Respawn Settings")]
        public float killYThreshold = 0.2f;
        private Vector3 _respawnPosition;
        private Quaternion _respawnRotation;
        private Rigidbody _rb;

        private bool _wasGrabbed = false;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Registriamo la posizione iniziale come punto di respawn sicuro
            _respawnPosition = transform.position;
            _respawnRotation = transform.rotation;

            // Se non hai assegnato un AudioSource, proviamo a prenderlo o aggiungerlo
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            // Impostazioni base per audio 3D
            audioSource.spatialBlend = 1.0f; // 3D
            audioSource.playOnAwake = false;
        }

        private void Update()
        {
            // Se l'oggetto cade sotto la soglia, respawna
            if (transform.position.y < killYThreshold)
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            Debug.Log($"[WasteItem] {gameObject.name} caduto fuori mappa. Respawn a {_respawnPosition}");
            
            transform.position = _respawnPosition;
            transform.rotation = _respawnRotation;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Aggiorna il punto di respawn (es. quando viene consegnato sulla scrivania)
        /// </summary>
        public void SetRespawnPoint(Vector3 pos, Quaternion rot)
        {
            _respawnPosition = pos;
            _respawnRotation = rot;
        }

        /// <summary>
        /// Da chiamare tramite evento Unity (XR Grab Interactable -> On Select Entered)
        /// </summary>
        public void PlayGrabSound()
        {
            if (grabSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(grabSound);
                _wasGrabbed = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Se l'oggetto è stato rilasciato e urta qualcosa, suona il drop
            if (_wasGrabbed && dropSound != null && audioSource != null)
            {
                // Evitiamo che suoni per collisioni troppo deboli
                if (collision.relativeVelocity.magnitude > 0.5f)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(dropSound);
                    _wasGrabbed = false; // Reset fino al prossimo grab
                }
            }
        }
    }
}
