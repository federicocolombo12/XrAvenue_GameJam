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
        public string headerText = "SALDO ATTUALE";
        public float currentBalance = 0f;
        public float rewardAmount = 10f;
        public float penaltyAmount = 5f;
        public string currencySymbol = " $";

        private void Start()
        {
            if (displayPopup != null)
            {
                // Importante: forziamo l'apertura all'inizio
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

        private void UpdateDisplay()
        {
            if (displayPopup != null)
            {
                string message = $"{currentBalance:F2}{currencySymbol}";
                displayPopup.ShowDialogue(message, headerText);
            }
        }
    }
}
