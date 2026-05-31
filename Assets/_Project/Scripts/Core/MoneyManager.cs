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
                // Uniamo etichetta e valore per essere sicuri che appaiano nel corpo del testo animato
                string fullMessage = $"{headerLabel}{currentBalance:F2}{currencySymbol}";
                
                // Passiamo una stringa vuota come speakerName per evitare doppioni
                displayPopup.ShowDialogue("", fullMessage); 
            }
        }
    }
}
