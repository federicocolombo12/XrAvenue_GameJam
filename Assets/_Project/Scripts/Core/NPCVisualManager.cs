using UnityEngine;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    public class NPCVisualManager : MonoBehaviour
    {
        [Header("References")]
        public NPCController controller;
        
        [Header("NPC Models Pool")]
        [Tooltip("Lista di GameObject (figli o prefab istanziati) che rappresentano i vari NPC.")]
        public List<GameObject> npcModels = new List<GameObject>();
        
        [Header("Animation Parameters")]
        public string isWalkingBool = "isWalking";

        private int _currentModelIndex = -1;
        private Animator _activeAnimator;
        private NPCHandBinder _activeHandBinder;

        public NPCHandBinder ActiveHandBinder => _activeHandBinder;

        void OnEnable()
        {
            if (controller != null)
            {
                controller.OnStateChanged += HandleStateChanged;
                controller.OnLeftScene += HandleNPCLeft;
            }
            
            InitializeFirstNPC();
        }

        void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStateChanged -= HandleStateChanged;
                controller.OnLeftScene -= HandleNPCLeft;
            }
        }

        private void InitializeFirstNPC()
        {
            foreach (var model in npcModels)
            {
                if (model != null) model.SetActive(false);
            }

            PrepareNextNPC();
        }

        private void PrepareNextNPC()
        {
            if (npcModels == null || npcModels.Count == 0) return;

            if (_currentModelIndex >= 0 && _currentModelIndex < npcModels.Count)
            {
                if (npcModels[_currentModelIndex] != null)
                    npcModels[_currentModelIndex].SetActive(false);
            }

            _currentModelIndex = (_currentModelIndex + 1) % npcModels.Count;

            GameObject nextModel = npcModels[_currentModelIndex];
            if (nextModel != null)
            {
                nextModel.SetActive(true);

                // Cerchiamo l'animator anche nei figli (più sicuro per prefab complessi)
                _activeAnimator = nextModel.GetComponentInChildren<Animator>();
                _activeHandBinder = nextModel.GetComponentInChildren<NPCHandBinder>();

                if (_activeAnimator != null)
                {
                    Debug.Log($"[NPCVisualManager] Attivato NPC: {nextModel.name} - Animator trovato su: {_activeAnimator.name}");
                    
                    // Reset e ricollegamento bones
                    _activeAnimator.Rebind();
                    _activeAnimator.Update(0f);
                    
                    // Sincronizzazione immediata con lo stato attuale del controller
                    // Nel caso in cui il DeliveryManager abbia già dato l'ordine di camminare
                    bool isWalking = (controller.CurrentState == NPCState.Walking);
                    bool isInteracting = (controller.CurrentState == NPCState.Interacting);

                    SetAnimatorBool(isWalkingBool, isWalking);
                    SetAnimatorBool("isInteracting", isInteracting);
                }
                else
                {
                    Debug.LogError($"[NPCVisualManager] ATTENZIONE: Nessun Animator trovato nel modello {nextModel.name}!");
                }
            }
        }

        private void HandleStateChanged(NPCState newState)
        {
            if (_activeAnimator == null) return;

            bool isWalking = (newState == NPCState.Walking);
            bool isInteracting = (newState == NPCState.Interacting);

            SetAnimatorBool(isWalkingBool, isWalking);

            // Se hai un parametro "isInteracting" nell'animator, lo settiamo qui
            SetAnimatorBool("isInteracting", isInteracting);
        }

        /// <summary>
        /// Imposta un bool nell'animator solo se il parametro esiste, evitando errori.
        /// </summary>
        private void SetAnimatorBool(string paramName, bool value)
        {
            if (_activeAnimator == null || string.IsNullOrEmpty(paramName)) return;

            foreach (AnimatorControllerParameter param in _activeAnimator.parameters)
            {
                if (param.name == paramName)
                {
                    _activeAnimator.SetBool(paramName, value);
                    return;
                }
            }
        }

        private void HandleNPCLeft()
        {
            PrepareNextNPC();
        }
    }
}
