using UnityEngine;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce l'audio dei passi per gli NPC basandosi sul loro NPCController.
    /// Da posizionare sui prefab/modelli degli NPC gestiti dall'NPCVisualManager.
    /// </summary>
    public class NPCFootstepManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Il controller che gestisce lo stato dell'NPC.")]
        public NPCController controller;
        [Tooltip("La sorgente audio per i passi. Se vuota, verrà cercata o creata.")]
        public AudioSource audioSource;

        [Header("Audio Settings")]
        public AudioClip[] footstepClips;
        [Range(0f, 1f)]
        public float volume = 0.5f;
        public float pitchMin = 0.8f;
        public float pitchMax = 1.2f;

        [Header("Automatic Mode")]
        [Tooltip("Se vero, riproduce i passi basandosi sulla distanza percorsa mentre lo stato è 'Walking'.")]
        public bool useAutomaticSteps = true;
        public float stepDistance = 0.8f;

        private Vector3 _lastPosition;
        private float _distanceAccumulated = 0f;

        private void Start()
        {
            // Cerca il controller nel genitore se non assegnato
            if (controller == null) controller = GetComponentInParent<NPCController>();
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                
                audioSource.spatialBlend = 1.0f; // Audio 3D
                audioSource.playOnAwake = false;
            }

            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (!useAutomaticSteps || controller == null) return;

            // Utilizza la State Machine esistente nell'NPCController
            if (controller.CurrentState == NPCState.Walking)
            {
                // Calcola la distanza percorsa effettiva
                float moveMagnitude = Vector3.Distance(transform.position, _lastPosition);
                _distanceAccumulated += moveMagnitude;

                if (_distanceAccumulated >= stepDistance)
                {
                    PlayFootstep();
                    _distanceAccumulated = 0f;
                }
            }
            else
            {
                // Reset accumulo se l'NPC si ferma
                _distanceAccumulated = 0f;
            }

            _lastPosition = transform.position;
        }

        /// <summary>
        /// Riproduce il suono del passo. Può anche essere chiamato da Animation Events.
        /// </summary>
        public void PlayFootstep()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(pitchMin, pitchMax);
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
