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
                _activeAnimator = nextModel.GetComponent<Animator>();
                _activeHandBinder = nextModel.GetComponentInChildren<NPCHandBinder>();
                
                if (_activeAnimator != null)
                {
                    _activeAnimator.SetBool(isWalkingBool, false);
                }
            }
        }

        private void HandleStateChanged(NPCState newState)
        {
            if (_activeAnimator == null) return;

            // Semplicemente: se sta camminando isWalking = true, altrimenti false
            bool walking = (newState == NPCState.Walking);
            _activeAnimator.SetBool(isWalkingBool, walking);
        }

        private void HandleNPCLeft()
        {
            PrepareNextNPC();
        }
    }
}
