using UnityEngine;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce la riproduzione dei suoni dei passi basandosi sul movimento del giocatore.
    /// Da posizionare sull'XR Origin (o dove risiede il CharacterController/ContinuousMoveProvider).
    /// </summary>
    public class FootstepManager : MonoBehaviour
    {
        [Header("References")]
        public NPCController npcController;
        public AudioSource audioSource;

        [Header("Settings")]
        public float stepDistance = 1.0f; // Distanza tra un passo e l'altro
        
        [Header("Audio")]
        public AudioClip[] footstepClips;
        [Range(0f, 1f)]
        public float volume = 0.4f;
        public float pitchMin = 0.9f;
        public float pitchMax = 1.1f;

        private Vector3 _lastPosition;
        private float _distanceAccumulated = 0f;

        private void Start()
        {
            if (npcController == null)
                npcController = GetComponentInParent<NPCController>();

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                audioSource.spatialBlend = 1.0f; // 3D Audio
                audioSource.playOnAwake = false;
            }

            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (npcController == null) return;

            // Riproduciamo i passi solo se l'NPC è nello stato Walking
            if (npcController.CurrentState == NPCState.Walking)
            {
                Vector3 currentPos = transform.position;
                // Calcoliamo lo spostamento effettivo (3D o orizzontale, qui usiamo 3D per semplicità)
                float moveMagnitude = Vector3.Distance(currentPos, _lastPosition);
                _distanceAccumulated += moveMagnitude;

                if (_distanceAccumulated >= stepDistance)
                {
                    PlayFootstep();
                    _distanceAccumulated = 0f;
                }
                
                _lastPosition = currentPos;
            }
            else
            {
                // Reset accumulo e posizione quando non cammina
                _distanceAccumulated = 0f;
                _lastPosition = transform.position;
            }
        }

        private void PlayFootstep()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            // Seleziona una clip casuale
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(pitchMin, pitchMax);
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
