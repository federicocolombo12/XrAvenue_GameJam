using UnityEngine;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class WasteDeliveryManager : MonoBehaviour
    {
        [Header("Butter Events")]
        public DayDataEvent onDayStart;
        public WasteTypeEvent onWasteSorted;
        public GameEvent onDayEnd;
        
        [Header("Narrative Sync Events")]
        public GameEvent onDialogueFinished; 
        public WasteTypeEvent onWasteDelivered;
        public DialogueDataEvent onDialogueStart;

        [Header("Sub-Systems")]
        public NPCController npcController;
        public WasteObjectSpawner objectSpawner;

        private List<WasteDeliveryStep> _currentDaySteps;
        private int _currentStepIndex;
        
        private bool _isWaitingForIntro = false;
        private bool _isWaitingForStepDialogue = false;
        private bool _isWaitingForObjectProcess = false;
        private bool _isWaitingForNPCLeave = false;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(HandleObjectProcessed);
            if (onDialogueFinished != null) onDialogueFinished.RegisterListener(_ => OnDialogueComplete());
            
            if (npcController != null)
            {
                npcController.OnLeftScene += HandleNPCLeft;
            }
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.DeregisterListener(HandleObjectProcessed);
            if (onDialogueFinished != null) onDialogueFinished.DeregisterListener(_ => OnDialogueComplete());

            if (npcController != null)
            {
                npcController.OnLeftScene -= HandleNPCLeft;
            }
        }

        private void StartNewDay(DayData day)
        {
            Debug.Log($"[WasteDeliveryManager] Inizio nuovo giorno: {day.dayLabel}");
            _currentDaySteps = day.deliveries;
            _currentStepIndex = 0;
            
            if (day.introDialogue != null)
            {
                _isWaitingForIntro = true;
                onDialogueStart.Raise(day.introDialogue);
            }
            else
            {
                RequestNextDelivery();
            }
        }

        private void OnDialogueComplete()
        {
            if (_isWaitingForIntro)
            {
                Debug.Log("[WasteDeliveryManager] Intro terminata. Richiedo prima consegna.");
                _isWaitingForIntro = false;
                RequestNextDelivery();
            }
            else if (_isWaitingForStepDialogue)
            {
                Debug.Log("[WasteDeliveryManager] Dialogo di consegna terminato.");
                _isWaitingForStepDialogue = false;
                CheckStepCompletion();
            }
        }

        private void RequestNextDelivery()
        {
            if (_currentStepIndex < _currentDaySteps.Count)
            {
                WasteDeliveryStep step = _currentDaySteps[_currentStepIndex];
                Debug.Log($"[WasteDeliveryManager] Richiedo consegna {_currentStepIndex}: {step.type}");
                
                _isWaitingForObjectProcess = true;
                _isWaitingForNPCLeave = true;

                npcController.DeliverObject(step.type, () => {
                    // L'NPC è arrivato alla scrivania
                    objectSpawner.Spawn(step.type);
                    if (onWasteDelivered != null) onWasteDelivered.Raise(step.type);

                    // Controlliamo dialoghi
                    bool hasDialogue = step.npcDialogue != null || step.bossDialogue != null;
                    if (hasDialogue)
                    {
                        _isWaitingForStepDialogue = true;
                        if (step.npcDialogue != null) onDialogueStart.Raise(step.npcDialogue);
                        if (step.bossDialogue != null) onDialogueStart.Raise(step.bossDialogue);
                    }
                });
            }
            else
            {
                Debug.Log("[WasteDeliveryManager] Tutte le consegne completate. Giorno finito.");
                if (onDayEnd != null) onDayEnd.Raise();
            }
        }

        private void HandleObjectProcessed(WasteType type)
        {
            Debug.Log($"[WasteDeliveryManager] Oggetto processato: {type}. Segnalo all'NPC di andarsene.");
            _isWaitingForObjectProcess = false;
            
            if (npcController != null)
            {
                npcController.CompleteInteraction();
            }
            
            CheckStepCompletion();
        }

        private void HandleNPCLeft()
        {
            Debug.Log("[WasteDeliveryManager] NPC uscito di scena.");
            _isWaitingForNPCLeave = false;
            CheckStepCompletion();
        }

        private void CheckStepCompletion()
        {
            // Per passare al prossimo step dobbiamo:
            // 1. Aver finito l'eventuale dialogo
            // 2. Aver processato l'oggetto
            // 3. L'NPC deve essere uscito di scena (per evitare sovrapposizioni)
            
            if (!_isWaitingForStepDialogue && !_isWaitingForObjectProcess && !_isWaitingForNPCLeave)
            {
                Debug.Log("[WasteDeliveryManager] Step completato. Passo al prossimo.");
                _currentStepIndex++;
                RequestNextDelivery();
            }
            else
            {
                Debug.Log($"[WasteDeliveryManager] In attesa di: " +
                          $"{(_isWaitingForStepDialogue ? "Dialogo " : "")}" +
                          $"{(_isWaitingForObjectProcess ? "Oggetto " : "")}" +
                          $"{(_isWaitingForNPCLeave ? "NPC_Leave" : "")}");
            }
        }
    }
}
