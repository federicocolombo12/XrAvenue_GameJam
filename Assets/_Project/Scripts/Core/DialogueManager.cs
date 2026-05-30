using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Butter Events")]
        public DialogueDataEvent onDialogueStart;
        public GameEvent onDialogueFinished;

        [Header("World Space Popups")]
        public WorldDialoguePopup bossWorldPopup;
        public WorldDialoguePopup npcWorldPopup;

        private Queue<DialogueLine> _lineQueue = new Queue<DialogueLine>();
        private DialogueData _currentData;
        private bool _isProcessing = false;

        void OnEnable()
        {
            if (onDialogueStart != null) onDialogueStart.RegisterListener(StartDialogueSequence);
        }

        void OnDisable()
        {
            if (onDialogueStart != null) onDialogueStart.DeregisterListener(StartDialogueSequence);
        }

        public void StartDialogueSequence(DialogueData data)
        {
            if (data == null || data.lines.Count == 0)
            {
                onDialogueFinished?.Raise();
                return;
            }

            _currentData = data;
            _lineQueue.Clear();
            foreach (var line in data.lines) _lineQueue.Enqueue(line);

            if (!_isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            _isProcessing = true;

            while (_lineQueue.Count > 0)
            {
                DialogueLine line = _lineQueue.Dequeue();
                bool isBoss = line.speaker == DialogueSpeaker.Boss;
                WorldDialoguePopup targetPopup = isBoss ? bossWorldPopup : npcWorldPopup;
                WorldDialoguePopup otherPopup = isBoss ? npcWorldPopup : bossWorldPopup;

                // Chiudi l'altro fumetto se è aperto
                if (otherPopup != null) otherPopup.Close();

                if (targetPopup != null)
                {
                    // Calcolo approssimativo della durata del typewriter
                    float estimatedTime = line.text.Length * targetPopup.typewriterDelay + targetPopup.animDuration;
                    
                    targetPopup.ShowDialogue(line.text, line.speakerName);
                    
                    // Aspettiamo che il testo finisca di scriversi
                    yield return new WaitForSeconds(estimatedTime);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Manca il popup per lo speaker: {line.speaker}");
                }

                // Delay di lettura prima della prossima linea
                float pauseTime = Mathf.Clamp(line.text.Length * 0.05f, _currentData.minPauseSeconds, _currentData.maxPauseSeconds);
                yield return new WaitForSeconds(pauseTime + _currentData.basePauseSeconds);
            }

            // Fine della sequenza - Chiudiamo tutti i fumetti
            if (bossWorldPopup != null) bossWorldPopup.Close();
            if (npcWorldPopup != null) npcWorldPopup.Close();

            _isProcessing = false;
            
            Debug.Log("[DialogueManager] Sequenza terminata. Lancio onDialogueFinished.");
            if (onDialogueFinished != null) onDialogueFinished.Raise();
        }
    }
}
