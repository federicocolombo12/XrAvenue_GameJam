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
        
        [Header("Rebellion Logic")]
        [Tooltip("Punti accumulati durante la giornata attuale.")]
        public int currentDailyRebellionPoints = 0;
        
        [Header("Butter Events")]
        public DayDataEvent onDayStart;
        public GameEvent onDayEnd;
        public DayDataEvent onFinaleReached; // Nuovo evento per i finali
        public StringEvent onBossSpeech; // Canale Schermo (Quick feedback)
        public BoolEvent onMoralChoiceMade;
        public DialogueDataEvent onDialogueStart; // Per sequenze lunghe

        void Start()
        {
            _currentDay = firstDay;
            currentDailyRebellionPoints = 0;
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
            if (_currentDay == null) return;

            // Reset dei punti all'inizio di ogni giorno
            currentDailyRebellionPoints = 0;
            Debug.Log($"Inizio { _currentDay.dayLabel}");
            
            if (onDayStart != null) onDayStart.Raise(_currentDay);
        }

        private void HandleMoralChoice(bool isRebellion)
        {
            // Se isRebellion è true, aumentiamo i punti sospetto/ribellione
            if (isRebellion) currentDailyRebellionPoints++;
            else currentDailyRebellionPoints--; // Se obbedisce a un ordine orribile, diminuiscono i punti

            currentDailyRebellionPoints = Mathf.Max(0, currentDailyRebellionPoints);
            
            string feedback = isRebellion ? "ATTENZIONE: Comportamento anomalo rilevato." : "Ottimo lavoro, cittadino.";
            if (onBossSpeech != null) onBossSpeech.Raise(feedback);
            
            Debug.Log($"[GameStateManager] Punti Ribellione Attuali: {currentDailyRebellionPoints}");
        }

        private IEnumerator TransitionToNextDayRoutine()
        {
            Debug.Log("[GameStateManager] Giorno finito. Calcolo esito narrativo...");
            yield return new WaitForSeconds(1.0f);

            // Se il giorno appena concluso era un finale, fermiamo qui la progressione
            if (_currentDay.isFinale)
            {
                Debug.Log($"[GameStateManager] FINALE RAGGIUNTO: {_currentDay.endingTitle}");
                if (onFinaleReached != null) onFinaleReached.Raise(_currentDay);
                yield break; 
            }

            // Calcolo se il giocatore ha superato la soglia di ribellione del giorno
            bool hasRebelled = currentDailyRebellionPoints >= _currentDay.rebellionThreshold;
            Debug.Log($"[GameStateManager] Esito Giorno: {(hasRebelled ? "RIBELLIONE" : "OBBEDIENZA")} (Punti: {currentDailyRebellionPoints}, Soglia: {_currentDay.rebellionThreshold})");

            if (hasRebelled)
            {
                if (_currentDay.nextDayRebel != null)
                    _currentDay = _currentDay.nextDayRebel;
                else
                    Debug.LogWarning("[GameStateManager] Nessun 'Next Day Rebel' impostato!");
            }
            else
            {
                if (_currentDay.nextDayObedient != null)
                    _currentDay = _currentDay.nextDayObedient;
                else
                    Debug.LogWarning("[GameStateManager] Nessun 'Next Day Obedient' impostato!");
            }

            StartDay();
        }

        public void CompleteDay()
        {
            if (onDayEnd != null) onDayEnd.Raise();
        }
    }
}
