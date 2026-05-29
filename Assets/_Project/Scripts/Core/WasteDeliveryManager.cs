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
        public DialogueDataEvent onDialogueStart; // Per lanciare i dialoghi della consegna

        [Header("Sub-Systems")]
        public NPCController npcController;
        public WasteObjectSpawner objectSpawner;

        private List<WasteDeliveryStep> _currentDaySteps;
        private int _currentStepIndex;
        private bool _isWaitingForDialogue = false;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(HandleObjectProcessed);
            if (onDialogueFinished != null) onDialogueFinished.RegisterListener(_ => OnDialogueComplete());
        }

        void OnDisable()
        {
            if (onDayStart != null) onDayStart.DeregisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.DeregisterListener(HandleObjectProcessed);
            if (onDialogueFinished != null) onDialogueFinished.DeregisterListener(_ => OnDialogueComplete());
        }

        private void StartNewDay(DayData day)
        {
            _currentDaySteps = day.deliveries;
            _currentStepIndex = 0;
            
            _isWaitingForDialogue = true;
            
            // Se c'è un intro, il manager aspetta. 
            // Se NON c'è un intro nel DayData, dobbiamo sbloccare subito.
            if (day.introDialogue == null)
            {
                OnDialogueComplete();
            }
        }

        private void OnDialogueComplete()
        {
            if (_isWaitingForDialogue)
            {
                _isWaitingForDialogue = false;
                RequestNextDelivery();
            }
        }

        private void RequestNextDelivery()
        {
            if (_currentStepIndex < _currentDaySteps.Count)
            {
                WasteDeliveryStep step = _currentDaySteps[_currentStepIndex];
                
                npcController.DeliverObject(step.type, () => {
                    objectSpawner.Spawn(step.type);
                    
                    // Notifichiamo l'arrivo
                    if (onWasteDelivered != null) onWasteDelivered.Raise(step.type);

                    // Controlliamo se questa consegna ha dei dialoghi bloccanti
                    bool hasDialogue = step.npcDialogue != null || step.bossDialogue != null;
                    
                    if (hasDialogue)
                    {
                        _isWaitingForDialogue = true;
                        if (step.npcDialogue != null) onDialogueStart.Raise(step.npcDialogue);
                        if (step.bossDialogue != null) onDialogueStart.Raise(step.bossDialogue);
                    }
                });
            }
        }

        private void HandleObjectProcessed(WasteType type)
        {
            _currentStepIndex++;

            if (_currentStepIndex >= _currentDaySteps.Count)
            {
                if (onDayEnd != null) onDayEnd.Raise();
            }
            else
            {
                // Se non stiamo aspettando un dialogo (es. il Capo che ci sgrida), 
                // chiediamo il prossimo NPC.
                if (!_isWaitingForDialogue)
                {
                    RequestNextDelivery();
                }
            }
        }
    }
}
