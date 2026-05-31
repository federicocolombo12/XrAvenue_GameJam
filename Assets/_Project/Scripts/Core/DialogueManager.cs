using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Butter Events")]
        public DayDataEvent onDayStart;
        public DialogueDataEvent onDialogueStart;
        public GameEvent onDialogueFinished;
        public AudioClipEvent onBossVoiceTriggered; // Evento specifico per il Boss
        public AudioClipEvent onNpcVoiceTriggered;  // Evento specifico per l'NPC
        public GameEvent onStopBossVoice; // Nuovo: Ferma voce Boss
        public GameEvent onStopNpcVoice;  // Nuovo: Ferma voce NPC

        [Header("World Space Popups")]
        public WorldDialoguePopup bossWorldPopup;
        public WorldDialoguePopup npcWorldPopup;

        [Header("Butter Events - Scene Sync")]
        public BoolEvent onTVStateChanged;

        private Queue<DialogueLine> _lineQueue = new Queue<DialogueLine>();
        private DialogueData _currentData;
        private bool _isProcessing = false;

        private AudioClip _bossFallback;
        private AudioClip _npcFallback;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(HandleDayStart);
            if (onDialogueStart != null) onDialogueStart.RegisterListener(StartDialogueSequence);
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(HandleDayStart);
            if (onDialogueStart != null) onDialogueStart.DeregisterListener(StartDialogueSequence);
        }

        private void HandleDayStart(DayData day)
        {
            if (day == null) return;
            _bossFallback = day.bossVoiceFallback;
            _npcFallback = day.npcVoiceFallback;
        }

        public void StartDialogueSequence(DialogueData data)
        {
            if (data == null || data.lines.Count == 0)
            {
                if (!_isProcessing) onDialogueFinished?.Raise();
                return;
            }

            // Se stiamo già processando, accodiamo le nuove linee alla coda esistente
            if (_isProcessing)
            {
                Debug.Log($"[DialogueManager] Sequenza in corso. Accodo {data.lines.Count} nuove linee.");
                foreach (var line in data.lines) _lineQueue.Enqueue(line);
                return;
            }

            // Nuova sequenza
            _isProcessing = true;
            _currentData = data;
            _lineQueue.Clear();
            foreach (var line in data.lines) _lineQueue.Enqueue(line);

            StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            // Rimosso _isProcessing = true da qui perché ora è settato subito in StartDialogueSequence
            while (_lineQueue.Count > 0)
            {
                DialogueLine line = _lineQueue.Dequeue();
                bool isBoss = line.speaker == DialogueSpeaker.Boss;
                WorldDialoguePopup targetPopup = isBoss ? bossWorldPopup : npcWorldPopup;
                WorldDialoguePopup otherPopup = isBoss ? npcWorldPopup : bossWorldPopup;

                // Gestione TV via Butter: Accesa se parla il Boss, spenta se parla l'NPC
                if (onTVStateChanged != null)
                {
                    onTVStateChanged.Raise(isBoss);
                }

                // Chiudi l'altro fumetto se è aperto
                if (otherPopup != null) otherPopup.Close();

                // Riproduci l'audio (priorità: line.voiceClip -> fallback del giorno)
                AudioClip clipToPlay = line.voiceClip;
                if (clipToPlay == null)
                {
                    clipToPlay = isBoss ? _bossFallback : _npcFallback;
                }

                if (clipToPlay != null)
                {
                    if (isBoss && onBossVoiceTriggered != null) onBossVoiceTriggered.Raise(clipToPlay);
                    else if (!isBoss && onNpcVoiceTriggered != null) onNpcVoiceTriggered.Raise(clipToPlay);
                }

                bool lineFinished = false;
                if (targetPopup != null)
                {
                    targetPopup.ShowDialogue(line.text, line.speakerName, () => lineFinished = true);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Manca il popup per lo speaker: {line.speaker}");
                    lineFinished = true;
                }

                yield return new WaitUntil(() => lineFinished);

                // Ferma l'audio appena finito il testo
                if (isBoss && onStopBossVoice != null) onStopBossVoice.Raise();
                else if (!isBoss && onStopNpcVoice != null) onStopNpcVoice.Raise();

                // Delay di lettura prima della prossima linea
                float pauseTime = Mathf.Clamp(line.text.Length * 0.05f, _currentData.minPauseSeconds, _currentData.maxPauseSeconds);
                yield return new WaitForSeconds(pauseTime + _currentData.basePauseSeconds);
            }

            // Fine della sequenza - Chiudiamo tutti i fumetti e spegniamo la TV via Butter
            if (bossWorldPopup != null) bossWorldPopup.Close();
            if (npcWorldPopup != null) npcWorldPopup.Close();
            if (onStopBossVoice != null) onStopBossVoice.Raise();
            if (onStopNpcVoice != null) onStopNpcVoice.Raise();
            if (onTVStateChanged != null) onTVStateChanged.Raise(false);

            _isProcessing = false;
            
            Debug.Log("[DialogueManager] Sequenza terminata. Lancio onDialogueFinished.");
            if (onDialogueFinished != null) onDialogueFinished.Raise();
        }
    }
}
