using UnityEngine;
using UnityEngine.Audio;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        public AudioMixer mainMixer;

        [Header("Audio Sources")]
        public AudioSource bossVoiceSource;  // Per dialoghi boss
        public AudioSource npcVoiceSource;   // Per dialoghi npc
        public AudioSource ambientSource;    // Per musica di sottofondo
        public AudioSource sfxSource;        // Per effetti sonori

        [Header("Butter Events - Game Flow")]
        public DayDataEvent onDayStart;
        public GameEvent onDayEnd;
        public DayDataEvent onFinaleReached;
        public DialogueDataEvent onDialogueStart;
        public GameEvent onDialogueFinished;

        [Header("Butter Events - Voice")]
        public StringEvent onBossSpeech;
        public StringEvent onNpcSpeech;

        [Header("Butter Events - Interaction")]
        public WasteTypeEvent onWasteSorted;

        [Header("Audio Clips - Ambient (Day)")]
        public AudioClip ambientDayClip;
        public AudioClip ambientNightClip;

        [Header("Audio Clips - SFX")]
        public AudioClip wasteDropClip;
        public AudioClip itemPlacedClip;
        public AudioClip errorClip;
        public AudioClip uiClickClip;

        [Header("Voice Pitch Settings")]
        public float bossPitchMin = 0.9f;
        public float bossPitchMax = 1.1f;
        public float npcPitchMin = 0.85f;
        public float npcPitchMax = 1.15f;

        private bool _isAmbientPlaying = false;
        private bool _isBossVoiceLooping = false;
        private bool _isNpcVoiceLooping = false;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(HandleDayStart);
            if (onDayEnd != null) onDayEnd.RegisterListener(_ => HandleDayEnd());
            if (onFinaleReached != null) onFinaleReached.RegisterListener(HandleFinaleReached);
            if (onBossSpeech != null) onBossSpeech.RegisterListener(PlayBossSound);
            if (onNpcSpeech != null) onNpcSpeech.RegisterListener(PlayNpcSound);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(PlayWasteSortedSFX);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(HandleDayStart);
            if (onDayEnd != null) onDayEnd.DeregisterListener(_ => HandleDayEnd());
            if (onFinaleReached != null) onFinaleReached.DeregisterListener(HandleFinaleReached);
            if (onBossSpeech != null) onBossSpeech.DeregisterListener(PlayBossSound);
            if (onNpcSpeech != null) onNpcSpeech.DeregisterListener(PlayNpcSound);
            if (onWasteSorted != null) onWasteSorted.DeregisterListener(PlayWasteSortedSFX);
        }

        // ========== GAME FLOW AUDIO ==========
        private void HandleDayStart(DayData day)
        {
            Debug.Log($"[AudioManager] Inizio giorno: {day.dayLabel}");
            
            if (day.dayAmbientMusic != null)
            {
                PlayAmbientLoop(day.dayAmbientMusic);
            }
            else
            {
                PlayAmbientLoop(ambientDayClip);
            }
        }

        private void HandleDayEnd()
        {
            Debug.Log("[AudioManager] Fine giorno. Fermo ambient.");
            StopAmbient();
        }

        private void HandleFinaleReached(DayData day)
        {
            if (day == null || day.endingSoundClip == null) return;
            
            Debug.Log($"[AudioManager] Finale raggiunto: {day.endingTitle}. Riproduzione audio finale.");
            StopAmbient();
            PlayDirectSFX(day.endingSoundClip);
        }

        // ========== VOICE ==========
        private void PlayBossSound(string audioClipName)
        {
            PlayVoiceLoop(bossVoiceSource, audioClipName, bossPitchMin, bossPitchMax, ref _isBossVoiceLooping, "Audio/Voice");
        }

        private void PlayNpcSound(string audioClipName)
        {
            PlayVoiceLoop(npcVoiceSource, audioClipName, npcPitchMin, npcPitchMax, ref _isNpcVoiceLooping, "Audio/Voice");
        }

        // ========== INTERACTION SFX ==========
        private void PlayWasteSortedSFX(WasteType type)
        {
            PlayOneShot(sfxSource, "item_placed");
        }

        // ========== PUBLIC METHODS ==========
        /// <summary>
        /// Riproduce un audio in loop con pitch variabile
        /// </summary>
        public void PlayVoiceLoop(AudioSource source, string clipName, float pitchMin, float pitchMax, ref bool isLooping, string folder = "Audio/SFX")
        {
            AudioClip clip = Resources.Load<AudioClip>($"{folder}/{clipName}");
            if (clip == null || source == null)
            {
                Debug.LogWarning($"[AudioManager] Clip non trovato: {folder}/{clipName}");
                return;
            }

            // Applica pitch random
            float randomPitch = Random.Range(pitchMin, pitchMax);
            source.pitch = randomPitch;

            // Riproduce il clip in loop
            source.clip = clip;
            source.loop = true;
            source.Play();
            isLooping = true;

            Debug.Log($"[AudioManager] Voice loop iniziato: {clipName} (pitch: {randomPitch:F2})");
        }

        /// <summary>
        /// Ferma il loop voice del boss
        /// </summary>
        public void StopBossVoiceLoop()
        {
            if (bossVoiceSource != null && _isBossVoiceLooping)
            {
                bossVoiceSource.Stop();
                bossVoiceSource.clip = null;
                bossVoiceSource.loop = false;
                _isBossVoiceLooping = false;
                Debug.Log("[AudioManager] Boss voice loop fermato");
            }
        }

        /// <summary>
        /// Ferma il loop voice dell'NPC
        /// </summary>
        public void StopNpcVoiceLoop()
        {
            if (npcVoiceSource != null && _isNpcVoiceLooping)
            {
                npcVoiceSource.Stop();
                npcVoiceSource.clip = null;
                npcVoiceSource.loop = false;
                _isNpcVoiceLooping = false;
                Debug.Log("[AudioManager] NPC voice loop fermato");
            }
        }

        /// <summary>
        /// Ferma entrambi i loop voice
        /// </summary>
        public void StopAllVoiceLoops()
        {
            StopBossVoiceLoop();
            StopNpcVoiceLoop();
        }

        /// <summary>
        /// Riproduce un suono una sola volta (one-shot)
        /// </summary>
        public void PlayOneShot(AudioSource source, string clipName, string folder = "Audio/SFX")
        {
            AudioClip clip = Resources.Load<AudioClip>($"{folder}/{clipName}");
            if (clip != null && source != null)
            {
                source.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Riprodotto one-shot: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Clip non trovato: {folder}/{clipName}");
            }
        }

        /// <summary>
        /// Riproduce un audio in loop finché non viene fermato
        /// </summary>
        public void PlayAmbientLoop(AudioClip clip)
        {
            if (clip == null || ambientSource == null)
            {
                Debug.LogWarning("[AudioManager] Clip o AmbientSource nullo");
                return;
            }

            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.Play();
            _isAmbientPlaying = true;
            Debug.Log($"[AudioManager] Inizio ambient loop: {clip.name}");
        }

        /// <summary>
        /// Ferma l'audio ambient in riproduzione
        /// </summary>
        public void StopAmbient()
        {
            if (ambientSource != null && _isAmbientPlaying)
            {
                ambientSource.Stop();
                _isAmbientPlaying = false;
                Debug.Log("[AudioManager] Ambient fermato");
            }
        }

        /// <summary>
        /// Cambia il volume di un gruppo del mixer
        /// </summary>
        public void SetVolume(string mixerGroup, float volume)
        {
            if (mainMixer != null)
            {
                // Converte volume lineare (0-1) a dB
                float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
                mainMixer.SetFloat(mixerGroup, dB);
                Debug.Log($"[AudioManager] Volume {mixerGroup} = {volume} ({dB:F1} dB)");
            }
        }

        /// <summary>
        /// Fade in/out smooth del volume
        /// </summary>
        public void FadeAmbient(float targetVolume, float duration)
        {
            StartCoroutine(FadeAmbientRoutine(targetVolume, duration));
        }

        private System.Collections.IEnumerator FadeAmbientRoutine(float targetVolume, float duration)
        {
            if (ambientSource == null) yield break;

            float startVolume = ambientSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            ambientSource.volume = targetVolume;
        }

        /// <summary>
        /// Riproduce un effetto sonoro diretto (da Inspector)
        /// </summary>
        public void PlayDirectSFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Riprodotto SFX diretto: {clip.name}");
            }
        }
    }
}