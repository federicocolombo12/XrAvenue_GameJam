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

        private bool _wasGrabbed = false;

        private void Start()
        {
            // Se non hai assegnato un AudioSource, proviamo a prenderlo o aggiungerlo
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            // Impostazioni base per audio 3D
            audioSource.spatialBlend = 1.0f; // 3D
            audioSource.playOnAwake = false;
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
