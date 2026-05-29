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

        [Header("UI Controllers")]
        public DialogueUIController bossUI;
        public DialogueUIController npcUI;

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
                DialogueUIController targetUI = (line.speaker == DialogueSpeaker.Boss) ? bossUI : npcUI;
                DialogueUIController otherUI = (line.speaker == DialogueSpeaker.Boss) ? npcUI : bossUI;

                // Nascondi l'altro pannello se attivo
                if (otherUI != null) otherUI.Hide();

                bool lineFinished = false;
                if (targetUI != null)
                {
                    targetUI.ShowLine(line.text, _currentData.secondsPerCharacter, () => {
                        lineFinished = true;
                    });
                }
                else
                {
                    lineFinished = true;
                }

                yield return new WaitUntil(() => lineFinished);

                // Delay di lettura prima della prossima linea
                float pauseTime = Mathf.Clamp(line.text.Length * 0.05f, _currentData.minPauseSeconds, _currentData.maxPauseSeconds);
                yield return new WaitForSeconds(pauseTime + _currentData.basePauseSeconds);
            }

            // Fine del dialogo
            if (bossUI != null) bossUI.Hide();
            if (npcUI != null) npcUI.Hide();

            _isProcessing = false;
            
            Debug.Log("DialogueManager: Sequenza terminata.");
            if (onDialogueFinished != null) onDialogueFinished.Raise();
        }
    }
}
