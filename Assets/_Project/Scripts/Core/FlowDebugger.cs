using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Script di utilità per testare il flusso di gioco dall'Inspector senza usare il visore.
    /// </summary>
    public class FlowDebugger : MonoBehaviour
    {
        [Header("Managers References")]
        public WasteDeliveryManager deliveryManager;
        public NPCController npcController;
        public GameStateManager gameStateManager;

        [Header("Butter Events to Trigger")]
        public WasteTypeEvent onWasteSorted;
        public BoolEvent onSortingResult;
        public GameEvent onDialogueFinished;

        [Header("Debug Settings")]
        public WasteType simulateType = WasteType.Paper;

        [ContextMenu("DEBUG: Simulate Correct Sorting")]
        public void SimulateCorrectSorting()
        {
            Debug.Log("<color=green>[FlowDebugger] Simulazione Smaciullamento CORRETTO</color>");
            // Notifichiamo il risultato positivo al MoneyManager
            if (onSortingResult != null) onSortingResult.Raise(true);
            // Notifichiamo la fine del processo al DeliveryManager per avanzare
            if (onWasteSorted != null) onWasteSorted.Raise(simulateType);
        }

        [ContextMenu("DEBUG: Simulate Wrong Sorting")]
        public void SimulateWrongSorting()
        {
            Debug.Log("<color=red>[FlowDebugger] Simulazione Smaciullamento ERRATO</color>");
            // Notifichiamo il risultato negativo
            if (onSortingResult != null) onSortingResult.Raise(false);
            // Avanziamo comunque lo step
            if (onWasteSorted != null) onWasteSorted.Raise(simulateType);
        }

        [ContextMenu("DEBUG: Skip Current Dialogue")]
        public void SkipDialogue()
        {
            Debug.Log("<color=yellow>[FlowDebugger] Simulazione Fine Dialogo</color>");
            if (onDialogueFinished != null) onDialogueFinished.Raise();
        }

        [ContextMenu("DEBUG: Force Complete Day")]
        public void ForceCompleteDay()
        {
            Debug.Log("<color=cyan>[FlowDebugger] Forzo completamento giornata</color>");
            if (gameStateManager != null) gameStateManager.CompleteDay();
        }

        [Header("Narrative Testing")]
        public bool setAsRebel = false;
        public BoolVariable isRebelVariable;

        [ContextMenu("DEBUG: Set Rebel Status and Next Day")]
        public void TestNarrativeBranch()
        {
            if (isRebelVariable != null)
            {
                isRebelVariable.Value = setAsRebel;
                Debug.Log($"[FlowDebugger] Status Ribelle impostato a: {setAsRebel}");
            }
            ForceCompleteDay();
        }
    }
}
