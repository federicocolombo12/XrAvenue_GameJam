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
        public AudioSource voiceSource;      // Per dialoghi boss/npc
        public AudioSource ambientSource;    // Per musica di sottofondo
        public AudioSource sfxSource;        // Per effetti sonori

        [Header("Butter Events")]
        public StringEvent onBossSpeech;
        public StringEvent onNpcSpeech;

        [Header("Audio Clips")]
        public AudioClip uiClickClip;
        public AudioClip errorClip;

        void OnEnable()
        {
            if (onBossSpeech != null) onBossSpeech.RegisterListener(PlayBossSound);
            if (onNpcSpeech != null) onNpcSpeech.RegisterListener(PlayNpcSound);
        }

        void OnDisable()
        {
            if (onBossSpeech != null) onBossSpeech.DeregisterListener(PlayBossSound);
            if (onNpcSpeech != null) onNpcSpeech.DeregisterListener(PlayNpcSound);
        }

        private void PlayBossSound(string audioClipName)
        {
            PlayVoiceClip(audioClipName);
        }

        private void PlayNpcSound(string audioClipName)
        {
            PlayVoiceClip(audioClipName);
        }

        public void PlayVoiceClip(string clipName)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/Voice/{clipName}");
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Riprodotto voice clip: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Voice clip non trovato: {clipName}");
            }
        }

        public void PlayAmbient(string clipName, bool loop = true)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/Ambient/{clipName}");
            if (clip != null && ambientSource != null)
            {
                ambientSource.clip = clip;
                ambientSource.loop = loop;
                ambientSource.Play();
                Debug.Log($"[AudioManager] Riprodotta ambient: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Ambient clip non trovato: {clipName}");
            }
        }

        public void PlaySFX(string clipName)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Riprodotto SFX: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX clip non trovato: {clipName}");
            }
        }

        public void StopAmbient()
        {
            if (ambientSource != null) ambientSource.Stop();
        }

        public void SetVolume(string mixerGroup, float volume)
        {
            if (mainMixer != null)
            {
                mainMixer.SetFloat(mixerGroup, Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
                Debug.Log($"[AudioManager] Volume {mixerGroup} impostato a {volume}");
            }
        }
    }
}