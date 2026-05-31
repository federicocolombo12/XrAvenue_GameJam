using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class MoneyManager : MonoBehaviour
    {
        [Header("Butter Events")]
        public BoolEvent onSortingResult;

        [Header("UI Reference")]
        public WorldDialoguePopup displayPopup;

        [Header("Settings")]
        public string headerLabel = "SALDO: ";
        public float currentBalance = 0f;
        public float rewardAmount = 10f;
        public float penaltyAmount = 5f;
        public string currencySymbol = " $";

        [Header("Display Settings")]
        public Color positiveColor = Color.green;
        public Color negativeColor = Color.red;

        private void Start()
        {
            if (displayPopup != null)
            {
                // Avviamo il sistema
                displayPopup.Open();
                UpdateDisplay();
            }
        }

        private void OnEnable()
        {
            if (onSortingResult != null)
                onSortingResult.RegisterListener(HandleSortingResult);
        }

        private void OnDisable()
        {
            if (onSortingResult != null)
                onSortingResult.DeregisterListener(HandleSortingResult);
        }

        private void HandleSortingResult(bool isCorrect)
        {
            if (isCorrect) currentBalance += rewardAmount;
            else currentBalance -= penaltyAmount;

            UpdateDisplay();
        }

        [ContextMenu("Test Update Display")]
        private void UpdateDisplay()
        {
            if (displayPopup != null)
            {
                Color targetColor = currentBalance >= 0 ? positiveColor : negativeColor;
                string colorHex = ColorUtility.ToHtmlStringRGB(targetColor);

                // Applichiamo il colore direttamente ai componenti se possibile
                if (displayPopup.speakerNameText != null) displayPopup.speakerNameText.color = Color.white;
                if (displayPopup.dialogueText != null) displayPopup.dialogueText.color = targetColor;
                
                // Usiamo anche il Rich Text per sicurezza nel corpo del messaggio
                string fullMessage = $"<color=#{colorHex}>{currentBalance:F2}{currencySymbol}</color>";
                
                // Passiamo una stringa vuota come speakerName e il messaggio colorato nel corpo
                // NOTA: Ho invertito i parametri rispetto a prima per usare il corpo del testo (dialogueText)
                displayPopup.ShowDialogue(fullMessage, headerLabel); 
            }
        }
    }
}
