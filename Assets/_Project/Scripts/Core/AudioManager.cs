using UnityEngine;
using UnityEngine.Audio;
using Dev.Nicklaj.Butter;
using System.Collections;

namespace AvenueXR.Core
{
    [System.Serializable]
    public class SpatialSFX
    {
        public AudioClip clip;
        public AudioSource targetSource;

        public void Play(float pitchMin = 1f, float pitchMax = 1f)
        {
            if (clip == null || targetSource == null) return;
            
            targetSource.pitch = Random.Range(pitchMin, pitchMax);
            targetSource.PlayOneShot(clip);
        }
    }

    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        public AudioMixer mainMixer;

        [Header("Audio Sources - Global")]
        public AudioSource bossVoiceSource;  // Per dialoghi boss
        public AudioSource npcVoiceSource;   // Per dialoghi npc
        public AudioSource ambientSource;    // Per musica di sottofondo
        
        [Header("Spatialized SFX")]
        public SpatialSFX wasteDropSFX;
        public SpatialSFX itemPlacedSFX;
        public SpatialSFX errorSFX;
        public SpatialSFX uiClickSFX;
        public SpatialSFX finaleSFX; // Sorgente per i finali

        [Header("Butter Events - Game Flow")]
        public DayDataEvent onDayStart;
        public GameEvent onDayEnd;
        public DayDataEvent onFinaleReached;
        public DialogueDataEvent onDialogueStart;
        public GameEvent onDialogueFinished;

        [Header("Butter Events - Voice")]
        public AudioClipEvent onBossVoiceTriggered; // Nuovo evento per audio Boss
        public AudioClipEvent onNpcVoiceTriggered;  // Nuovo evento per audio NPC
        public GameEvent onStopBossVoice;
        public GameEvent onStopNpcVoice;

        [Header("Butter Events - Interaction")]
        public WasteTypeEvent onWasteSorted;

        [Header("Voice Pitch Settings")]
        public float bossPitchMin = 0.9f;
        public float bossPitchMax = 1.1f;
        public float npcPitchMin = 0.85f;
        public float npcPitchMax = 1.15f;

        private bool _isAmbientPlaying = false;
        private Coroutine _bossVoiceCoroutine;
        private Coroutine _npcVoiceCoroutine;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(HandleDayStart);
            if (onDayEnd != null) onDayEnd.RegisterListener(_ => HandleDayEnd());
            if (onFinaleReached != null) onFinaleReached.RegisterListener(HandleFinaleReached);
            if (onBossVoiceTriggered != null) onBossVoiceTriggered.RegisterListener(PlayBossVoiceDirectly);
            if (onNpcVoiceTriggered != null) onNpcVoiceTriggered.RegisterListener(PlayNpcVoiceDirectly);
            if (onStopBossVoice != null) onStopBossVoice.RegisterListener(_ => StopBossVoice());
            if (onStopNpcVoice != null) onStopNpcVoice.RegisterListener(_ => StopNpcVoice());
            if (onWasteSorted != null) onWasteSorted.RegisterListener(PlayWasteSortedSFX);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(HandleDayStart);
            if (onDayEnd != null) onDayEnd.DeregisterListener(_ => HandleDayEnd());
            if (onFinaleReached != null) onFinaleReached.DeregisterListener(HandleFinaleReached);
            if (onBossVoiceTriggered != null) onBossVoiceTriggered.DeregisterListener(PlayBossVoiceDirectly);
            if (onNpcVoiceTriggered != null) onNpcVoiceTriggered.DeregisterListener(PlayNpcVoiceDirectly);
            if (onStopBossVoice != null) onStopBossVoice.DeregisterListener(_ => StopBossVoice());
            if (onStopNpcVoice != null) onStopNpcVoice.DeregisterListener(_ => StopNpcVoice());
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
        }

        private void HandleDayEnd()
        {
            Debug.Log("[AudioManager] Fine giorno. Fermo ambient.");
            StopAmbient();
        }

        private void HandleFinaleReached(DayData day)
        {
            if (day == null || day.endingSoundClip == null) return;
            
            Debug.Log($"[AudioManager] Finale raggiunto: {day.endingTitle}. Preparazione audio finale.");
            
            // Fermiamo subito l'ambient per creare il vuoto
            StopAmbient();
            
            // Facciamo partire l'audio del finale. 
            // Se vogliamo che parta ESATTAMENTE quando appare il testo, 
            // potremmo usare un delay, ma l'utente ha chiesto che parta subito dopo il fade.
            if (finaleSFX.targetSource != null)
            {
                finaleSFX.targetSource.PlayOneShot(day.endingSoundClip);
            }
        }

        // ========== VOICE ==========
        private void PlayBossVoiceDirectly(AudioClip clip)
        {
            if (clip == null || bossVoiceSource == null) return;
            
            StopBossVoice();
            _bossVoiceCoroutine = StartCoroutine(VoiceLoopRoutine(bossVoiceSource, clip, bossPitchMin, bossPitchMax));
            Debug.Log($"[AudioManager] Inizio loop voce Boss: {clip.name}");
        }

        private void PlayNpcVoiceDirectly(AudioClip clip)
        {
            if (clip == null || npcVoiceSource == null) return;
            
            StopNpcVoice();
            _npcVoiceCoroutine = StartCoroutine(VoiceLoopRoutine(npcVoiceSource, clip, npcPitchMin, npcPitchMax));
            Debug.Log($"[AudioManager] Inizio loop voce NPC: {clip.name}");
        }

        private IEnumerator VoiceLoopRoutine(AudioSource source, AudioClip clip, float minPitch, float maxPitch)
        {
            source.clip = clip;
            source.loop = false; // Gestiamo il loop manualmente

            while (true)
            {
                source.pitch = Random.Range(minPitch, maxPitch);
                source.Play();
                
                // Attendiamo la fine della clip prima di ripartire col prossimo "giro" e nuovo pitch
                yield return new WaitForSeconds(clip.length / Mathf.Abs(source.pitch));
            }
        }

        private void StopBossVoice()
        {
            if (_bossVoiceCoroutine != null)
            {
                StopCoroutine(_bossVoiceCoroutine);
                _bossVoiceCoroutine = null;
            }

            if (bossVoiceSource != null)
            {
                bossVoiceSource.Stop();
                bossVoiceSource.clip = null;
            }
        }

        private void StopNpcVoice()
        {
            if (_npcVoiceCoroutine != null)
            {
                StopCoroutine(_npcVoiceCoroutine);
                _npcVoiceCoroutine = null;
            }

            if (npcVoiceSource != null)
            {
                npcVoiceSource.Stop();
                npcVoiceSource.clip = null;
            }
        }

        // ========== INTERACTION SFX ==========
        private void PlayWasteSortedSFX(WasteType type)
        {
            itemPlacedSFX.Play();
        }

        // ========== PUBLIC METHODS ==========
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
    }
}
