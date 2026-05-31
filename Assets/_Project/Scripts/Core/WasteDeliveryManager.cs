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
        public WasteTypeEvent onWasteReturned; // Aggiunto per la ribellione
        public GameEvent onDayEnd;
        
        [Header("Narrative Sync Events")]
        public GameEvent onDialogueFinished; 
        public WasteTypeEvent onWasteDelivered;
        public DialogueDataEvent onDialogueStart;
        public BoolEvent onMoralChoiceMade; // Per segnalare la ribellione

        [Header("Sub-Systems")]
        public NPCController npcController;
        public WasteObjectSpawner objectSpawner;
        public NPCVisualManager npcVisualManager;

        private List<WasteDeliveryStep> _currentDaySteps;
        private DayData _currentDayData;
        private int _currentStepIndex;
        private GameObject _selectedPrefab;
        
        private bool _isWaitingForIntro = false;
        private bool _isWaitingForOutro = false;
        private bool _isWaitingForStepDialogue = false;
        private bool _isWaitingForObjectProcess = false;
        private bool _isWaitingForNPCLeave = false;

        void OnEnable()
        {
            if (onDayStart != null) onDayStart.RegisterListener(StartNewDay);
            if (onWasteSorted != null) onWasteSorted.RegisterListener(HandleObjectProcessed);
            if (onWasteReturned != null) onWasteReturned.RegisterListener(HandleWasteReturned);
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
            if (onWasteReturned != null) onWasteReturned.DeregisterListener(HandleWasteReturned);
            if (onDialogueFinished != null) onDialogueFinished.DeregisterListener(_ => OnDialogueComplete());

            if (npcController != null)
            {
                npcController.OnLeftScene -= HandleNPCLeft;
            }
        }

        private void StartNewDay(DayData day)
        {
            if (day == null) return;
            
            Debug.Log($"[WasteDeliveryManager] Inizio nuovo giorno: {day.dayLabel}");
            _currentDayData = day;
            _currentDaySteps = day.deliveries;
            _currentStepIndex = 0;
            
            // Reset di sicurezza per tutti i flag
            _isWaitingForIntro = false;
            _isWaitingForOutro = false;
            _isWaitingForStepDialogue = false;
            _isWaitingForObjectProcess = false;
            _isWaitingForNPCLeave = false;

            if (day.introDialogue != null)
            {
                _isWaitingForIntro = true;
                onDialogueStart.Raise(day.introDialogue);
            }
            else
            {
                StartDayAction();
            }
        }

        /// <summary>
        /// Avvia l'azione effettiva della giornata (consegne normali o spawn speciale finale)
        /// </summary>
        private void StartDayAction()
        {
            if (_currentDayData.isFinale && _currentDayData.specialFinaleObjectPrefab != null)
            {
                SpawnSpecialFinaleObject(_currentDayData);
            }
            else
            {
                RequestNextDelivery();
            }
        }

        private void SpawnSpecialFinaleObject(DayData day)
        {
            Debug.Log($"[WasteDeliveryManager] Finale speciale rilevato. Spawning: {day.specialFinaleObjectPrefab.name}");
            
            if (objectSpawner != null)
            {
                objectSpawner.SpawnPrefab(day.specialFinaleObjectPrefab);
            }
        }

        private void OnDialogueComplete()
        {
            if (_isWaitingForIntro)
            {
                Debug.Log("[WasteDeliveryManager] Intro terminata. Avvio azione del giorno.");
                _isWaitingForIntro = false;
                StartDayAction();
            }
            else if (_isWaitingForOutro)
            {
                Debug.Log("[WasteDeliveryManager] Outro terminata. Fine giorno ufficiale.");
                _isWaitingForOutro = false;
                if (onDayEnd != null) onDayEnd.Raise();
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
            if (_currentDaySteps == null) return;

            if (_currentStepIndex < _currentDaySteps.Count)
            {
                WasteDeliveryStep step = _currentDaySteps[_currentStepIndex];
                Debug.Log($"[WasteDeliveryManager] --- Inizio Step {_currentStepIndex} ---");
                
                // Reset flag per il nuovo step
                _isWaitingForObjectProcess = true;
                _isWaitingForNPCLeave = true;
                _isWaitingForStepDialogue = false;

                // Scegliamo il prefab ADESSO per mostrarlo in mano all'NPC durante il cammino
                _selectedPrefab = objectSpawner.GetRandomPrefabForType(step.type);
                if (npcVisualManager != null && npcVisualManager.ActiveHandBinder != null) 
                    npcVisualManager.ActiveHandBinder.BindPrefab(_selectedPrefab);

                npcController.DeliverObject(step.type, () => {
                    // L'NPC è arrivato alla scrivania
                    
                    // Rimuoviamo l'oggetto dalla mano e lo facciamo apparire sulla scrivania
                    if (npcVisualManager != null && npcVisualManager.ActiveHandBinder != null) 
                        npcVisualManager.ActiveHandBinder.Clear();
                        
                    objectSpawner.SpawnPrefab(_selectedPrefab);
                    
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
                FinishDayFlow();
            }
        }

        private void FinishDayFlow()
        {
            Debug.Log("[WasteDeliveryManager] Tutte le consegne completate. Controllo Outro.");
            
            if (_currentDayData != null && _currentDayData.outroDialogue != null)
            {
                _isWaitingForOutro = true;
                onDialogueStart.Raise(_currentDayData.outroDialogue);
            }
            else
            {
                Debug.Log("[WasteDeliveryManager] Nessun Outro. Fine giorno.");
                if (onDayEnd != null) onDayEnd.Raise();
            }
        }

        private void HandleWasteReturned(WasteType type)
        {
            Debug.Log($"[WasteDeliveryManager] Oggetto {type} RESTITUITO. Atto di ribellione!");
            
            // Segnaliamo la scelta morale al GameStateManager via Butter
            if (onMoralChoiceMade != null)
            {
                onMoralChoiceMade.Raise(true); // true = Ribellione
            }

            // Se l'oggetto viene restituito, lo rimettiamo visivamente in mano all'NPC che se ne va
            if (npcVisualManager != null && npcVisualManager.ActiveHandBinder != null)
            {
                npcVisualManager.ActiveHandBinder.BindPrefab(_selectedPrefab);
            }

            // Consideriamo l'oggetto processato per il flusso del giorno
            _isWaitingForObjectProcess = false;

            if (npcController != null)
            {
                npcController.CompleteInteraction();
            }

            CheckStepCompletion();
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
