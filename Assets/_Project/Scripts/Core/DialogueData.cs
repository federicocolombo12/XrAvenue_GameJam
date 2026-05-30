using System.Collections.Generic;
using UnityEngine;

namespace AvenueXR.Core
{
    public enum DialogueSpeaker
    {
        Boss,
        Npc
    }

    [System.Serializable]
    public class DialogueLine
    {
        public DialogueSpeaker speaker = DialogueSpeaker.Boss;
        public string speakerName; // Nome visualizzato nel popup
        [TextArea(2, 5)]
        public string text;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "AvenueXR/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Linee")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Header("Timing")]
        public float basePauseSeconds = 0.6f;
        public float secondsPerCharacter = 0.04f;
        public float minPauseSeconds = 0.5f;
        public float maxPauseSeconds = 4f;
    }
}

