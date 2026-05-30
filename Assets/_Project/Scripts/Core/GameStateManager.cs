using UnityEngine;
using System.Collections;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class GameStateManager : MonoBehaviour
    {
        [Header("Initial State")]
        public DayData firstDay;
        private DayData _currentDay;
        
        [Header("Butter Variables")]
        public BoolVariable isRebelVariable; 
        
        [Header("Butter Events")]
        public DayDataEvent onDayStart;
        public GameEvent onDayEnd;
        public StringEvent onBossSpeech; // Canale Schermo (Quick feedback)
        public StringEvent onNPCSpeech;  // Canale Cristiano (Quick feedback)
        public BoolEvent onMoralChoiceMade;
        public DialogueDataEvent onDialogueStart; // Per sequenze lunghe

        void Start()
        {
            _currentDay = firstDay;
            if (isRebelVariable != null) isRebelVariable.Value = false;
            StartDay();
        }

        void OnEnable()
        {
            if (onMoralChoiceMade != null) onMoralChoiceMade.RegisterListener(HandleMoralChoice);
            if (onDayEnd != null) onDayEnd.RegisterListener(_ => StartCoroutine(TransitionToNextDayRoutine()));
        }

        void OnDisable()
        {
            if (onMoralChoiceMade != null) onMoralChoiceMade.DeregisterListener(HandleMoralChoice);
            if (onDayEnd != null) onDayEnd.DeregisterListener(_ => StartCoroutine(TransitionToNextDayRoutine()));
        }

        private void StartDay()
        {
            if (isRebelVariable != null) isRebelVariable.Value = false;
            Debug.Log($"Inizio { _currentDay.dayLabel}");
            
            if (onDayStart != null) onDayStart.Raise(_currentDay);
        }

        private void HandleMoralChoice(bool isGood)
        {
            if (isGood && isRebelVariable != null) isRebelVariable.Value = true;
            
            string feedback = isGood ? "ATTENZIONE: Comportamento anomalo rilevato." : "Ottimo lavoro, cittadino.";
            if (onBossSpeech != null) onBossSpeech.Raise(feedback);
        }

        private IEnumerator TransitionToNextDayRoutine()
        {
            Debug.Log("[GameStateManager] Giorno finito. Attendo prima di passare al prossimo...");
            yield return new WaitForSeconds(1.0f);

            bool hasRebelled = isRebelVariable != null && isRebelVariable.Value;

            if (hasRebelled)
            {
                if (_currentDay.nextDayRebel != null)
                    _currentDay = _currentDay.nextDayRebel;
            }
            else
            {
                if (_currentDay.nextDayObedient != null)
                    _currentDay = _currentDay.nextDayObedient;
            }

            StartDay();
        }

        public void CompleteDay()
        {
            if (onDayEnd != null) onDayEnd.Raise();
        }
    }
}
