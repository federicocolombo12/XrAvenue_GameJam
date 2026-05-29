using UnityEngine;
using TMPro;
using Dev.Nicklaj.Butter;
using System.Collections;

namespace AvenueXR.Core
{
    public class BossScreenController : MonoBehaviour
    {
        public TextMeshProUGUI bossTextField;
        public TextMeshProUGUI speakerTextField;
        public TextMeshProUGUI contentTextField;
        public StringEvent onBossSpeech;
        public DialogueDataEvent onDialogueStart;

        private Coroutine _playRoutine;

        void OnEnable()
        {
            if (onBossSpeech != null) onBossSpeech.RegisterListener(UpdateBossText);
            if (onDialogueStart != null) onDialogueStart.RegisterListener(PlayDialogue);
        }

        void OnDisable()
        {
            if (onBossSpeech != null) onBossSpeech.DeregisterListener(UpdateBossText);
            if (onDialogueStart != null) onDialogueStart.DeregisterListener(PlayDialogue);
            StopDialogueRoutine();
        }

        private void UpdateBossText(string text)
        {
            SetSpeakerAndContent("CAPO", text);
            Debug.Log($"Schermo Capo: {text}");
        }

        private void PlayDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.lines == null || dialogue.lines.Count == 0) return;

            StopDialogueRoutine();
            _playRoutine = StartCoroutine(PlayDialogueRoutine(dialogue));
        }

        private IEnumerator PlayDialogueRoutine(DialogueData dialogue)
        {
            foreach (DialogueLine line in dialogue.lines)
            {
                string speakerLabel = line.speaker == DialogueSpeaker.Boss ? "CAPO" : "NPC";
                SetSpeakerAndContent(speakerLabel, line.text);

                float pause = dialogue.basePauseSeconds + (line.text != null ? line.text.Length : 0) * dialogue.secondsPerCharacter;
                pause = Mathf.Clamp(pause, dialogue.minPauseSeconds, dialogue.maxPauseSeconds);
                yield return new WaitForSeconds(pause);
            }
        }

        private void StopDialogueRoutine()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
        }

        private void SetSpeakerAndContent(string speaker, string content)
        {
            if (speakerTextField != null && contentTextField != null)
            {
                speakerTextField.text = speaker;
                contentTextField.text = content;
                return;
            }

            if (bossTextField != null)
            {
                bossTextField.text = $"{speaker}: {content}";
            }
        }
    }
}
