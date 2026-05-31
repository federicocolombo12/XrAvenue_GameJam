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
            if (onDayEnd != null) onDayEnd.RegisterListener(HandleDayEndEvent);
        }

        void OnDisable()
        {
            if (onMoralChoiceMade != null) onMoralChoiceMade.DeregisterListener(HandleMoralChoice);
            if (onDayEnd != null) onDayEnd.DeregisterListener(HandleDayEndEvent);
        }

        private void HandleDayEndEvent(Unit unit)
        {
            Debug.Log("[GameStateManager] Ricevuto evento Fine Giornata. Avvio routine di transizione.");
            StartCoroutine(TransitionToNextDayRoutine());
        }

        private void StartDay()
        {
            if (_currentDay == null)
            {
                Debug.LogError("[GameStateManager] Impossibile avviare il giorno: _currentDay è NULL!");
                return;
            }

            // Reset dei punti all'inizio di ogni giorno
            currentDailyRebellionPoints = 0;
            Debug.Log($"<color=cyan>[GameStateManager] AVVIO: {_currentDay.dayLabel} (Soglia Ribellione: {_currentDay.rebellionThreshold})</color>");
            
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
            
            Debug.Log($"<color=orange>[GameStateManager] Scelta Morale: {(isRebellion ? "RIBELLE" : "OBBEDIENTE")}. Punti attuali: {currentDailyRebellionPoints}</color>");
        }

        private IEnumerator TransitionToNextDayRoutine()
        {
            Debug.Log("[GameStateManager] Inizio calcolo esito narrativo...");
            yield return new WaitForSeconds(1.5f); // Un po' più di tempo per sicurezza

            // Se il giorno appena concluso era un finale, fermiamo qui la progressione
            if (_currentDay.isFinale)
            {
                Debug.Log($"<color=gold>[GameStateManager] FINALE RAGGIUNTO: {_currentDay.endingTitle}</color>");
                if (onFinaleReached != null) onFinaleReached.Raise(_currentDay);
                yield break; 
            }

            // Calcolo se il giocatore ha superato la soglia di ribellione del giorno
            bool hasRebelled = currentDailyRebellionPoints >= _currentDay.rebellionThreshold;
            Debug.Log($"<color=white>[GameStateManager] Calcolo fine giornata: Punti={currentDailyRebellionPoints}, Soglia={_currentDay.rebellionThreshold} -> RISULTATO: {(hasRebelled ? "RIBELLIONE" : "OBBEDIENZA")}</color>");

            DayData nextDay = hasRebelled ? _currentDay.nextDayRebel : _currentDay.nextDayObedient;

            if (nextDay != null)
            {
                _currentDay = nextDay;
                Debug.Log($"<color=green>[GameStateManager] Transizione a: {_currentDay.dayLabel}</color>");
            }
            else
            {
                Debug.LogError($"[GameStateManager] Errore critico: Nessun giorno successivo impostato per {(hasRebelled ? "Rebel" : "Obedient")}!");
                // Fallback per non bloccare il gioco completamente (opzionale)
                // _currentDay = hasRebelled ? _currentDay.nextDayObedient : _currentDay.nextDayRebel;
            }

            StartDay();
        }

        public void CompleteDay()
        {
            if (onDayEnd != null) onDayEnd.Raise();
        }
    }
}
