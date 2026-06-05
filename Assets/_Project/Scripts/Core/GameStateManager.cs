using UnityEngine;
using System.Collections;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class GameStateManager : MonoBehaviour
    {
        [Header("Initial State")]
        public DayData firstDay;
        
        [Header("Runtime State (Debug)")]
        [Tooltip("Giorno attuale visualizzato per debug nell'Inspector.")]
        public DayData currentDayDebug;
        [Tooltip("Punti accumulati durante la giornata attuale.")]
        public int currentDailyRebellionPoints = 0;
        
        private DayData _currentDay;
        private Coroutine _transitionCoroutine;
        private bool _isFinaleReached = false;

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
            currentDayDebug = _currentDay;
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
            Debug.Log("<color=yellow>[GameStateManager] Ricevuto evento Fine Giornata.</color>");
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionToNextDayRoutine());
        }

        private void StartDay()
        {
            if (_currentDay == null)
            {
                Debug.LogError("[GameStateManager] Errore: _currentDay è NULL!");
                return;
            }

            _isFinaleReached = false; // Reset ogni volta che inizia un nuovo giorno
            currentDayDebug = _currentDay;
            currentDailyRebellionPoints = 0;
            
            Debug.Log($"<color=cyan>[GameStateManager] AVVIO: {_currentDay.dayLabel} (Soglia: {_currentDay.rebellionThreshold})</color>");
            
            if (onDayStart != null) onDayStart.Raise(_currentDay);
        }

        private void HandleMoralChoice(bool isRebellion)
        {
            if (isRebellion) currentDailyRebellionPoints++;
            else currentDailyRebellionPoints--; 

            currentDailyRebellionPoints = Mathf.Max(0, currentDailyRebellionPoints);
            
            string feedback = isRebellion ? "ATTENZIONE: Comportamento anomalo rilevato." : "Ottimo lavoro, cittadino.";
            if (onBossSpeech != null) onBossSpeech.Raise(feedback);
            
            Debug.Log($"<color=orange>[GameStateManager] Scelta Morale: {(isRebellion ? "RIBELLE" : "OBBEDIENTE")}. Punti: {currentDailyRebellionPoints}</color>");
        }

        private IEnumerator TransitionToNextDayRoutine()
        {
            Debug.Log("[GameStateManager] Calcolo transizione narratva...");
            yield return new WaitForSeconds(1.2f); 

            if (_currentDay.isFinale)
            {
                if (_isFinaleReached)
                {
                    Debug.Log("[GameStateManager] Finale già innescato. Ignoro.");
                    yield break;
                }

                _isFinaleReached = true;
                Debug.Log($"<color=gold>[GameStateManager] FINALE: {_currentDay.endingTitle}</color>");
                if (onFinaleReached != null) onFinaleReached.Raise(_currentDay);
                yield break; 
            }

            bool hasRebelled = currentDailyRebellionPoints >= _currentDay.rebellionThreshold;
            DayData nextDay = hasRebelled ? _currentDay.nextDayRebel : _currentDay.nextDayObedient;

            if (nextDay != null)
            {
                Debug.Log($"<color=green>[GameStateManager] Prossimo giorno: {nextDay.dayLabel} (Era Ribellione? {hasRebelled})</color>");
                _currentDay = nextDay;
            }
            else
            {
                Debug.LogError($"[GameStateManager] MANCA NEXT DAY per {_currentDay.dayLabel} (Esito: {hasRebelled})!");
            }

            StartDay();
            _transitionCoroutine = null;
        }

        public void CompleteDay()
        {
            if (onDayEnd != null) onDayEnd.Raise();
        }
    }
}
